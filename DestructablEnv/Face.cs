using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Face
{
   private Shape m_Owner;
   private FaceMesh m_Mesh;
   private int m_NumPoints;
   private Vector3 m_Normal;
   public Vector3 NormalWorld { get; private set; }
   public Vector3 P0World { get { return m_OwnersWorldPoints[m_PointIndicies[0]]; } }

   private IEdgeEnumerator m_LoopEnumerator;

   private Edge m_Head;

   private List<Vector3> m_Points;
   private List<int> m_PointIndicies;

   private List<Vector3> m_OwnersWorldPoints;

   public Shape OwnerShape { get { return m_Owner; } }

   public Face()
   {
      m_Points = new List<Vector3>(10);
      m_PointIndicies = new List<int>(10);
      m_LoopEnumerator = new EdgeLoopEnumerator();
   }

   public void UpdateWorldNormal()
   {
      NormalWorld = m_Owner.transform.TransformDirection(m_Normal);
   }

   public void Init(Edge head, int numPoints, Vector3 normal)
   {
      m_Head = head;
      m_NumPoints = numPoints;
      m_Normal = normal;
   }

   public bool IsAbovePoint(Vector3 Pws, out float amountAbove)
   {
      var P0 = m_OwnersWorldPoints[m_PointIndicies[0]];
      amountAbove = Vector3.Dot(P0 - Pws, NormalWorld);
      return (amountAbove > 0.0f);
   }

   public bool IsCollidedWithEdge(Vector3 P0ws, Vector3 P1ws, ref Vector3 collPoint, ref Vector3 collNormal)
   {
      var closestEdge = -1;
      var closestComp = Mathf.NegativeInfinity;

      if (Utils.LinePlaneIntersect(NormalWorld, m_OwnersWorldPoints[m_PointIndicies[0]], P0ws, P1ws, out collPoint))
      {
         for (int i = 0; i < m_PointIndicies.Count; i++)
         {
            var next = (i + 1) % m_PointIndicies.Count;

            var Pi = m_OwnersWorldPoints[m_PointIndicies[i]];
            var Pnext = m_OwnersWorldPoints[m_PointIndicies[next]];

            var n = Vector3.Cross(Pnext - Pi, NormalWorld);
            var toP = collPoint - Pi;

            var comp = Vector3.Dot(n, toP);

            if (comp <= 0.0f)
            {
               if (comp > closestComp)
               {
                  closestComp = comp;
                  closestEdge = i;
               }
            }
            else
            {
               return false;
            }
         }

         var Pc = m_OwnersWorldPoints[m_PointIndicies[closestEdge]];
         var PcNext = m_OwnersWorldPoints[m_PointIndicies[(closestEdge + 1) % m_PointIndicies.Count]];

         collNormal = Vector3.Cross(P0ws - P1ws, Pc - PcNext).normalized;
         return true;
      }
      return false;
   }

   public void OnNewOwner(Shape owner)
   {
      if (m_Owner != owner)
      {
         owner.Faces.Add(this);
      }
      m_Owner = owner;
      m_OwnersWorldPoints = owner.WorldPoints;
   }

   public void SplitInHalf(Edge e1, Edge e2) // edges that form split with next
   {
      var newFace = new Face();

      var pairForThis = new EdgePair(this, null);
      var pairForNewFace = new EdgePair(newFace, null);

      var eThis = pairForThis.Edge1;
      var eNew = pairForNewFace.Edge1;

      var e1Next = e1.Next;
      var e2Next = e2.Next;

      e1.InsertAfterAndBreak(eThis);
      e2Next.InsertBeforeAndBreak(eThis);

      e1Next.InsertBeforeAndBreak(eNew);
      e2.InsertAfterAndBreak(eNew);

      eThis.Start = e1.End;
      eThis.End = e2Next.Start;

      eNew.Start = e2.End;
      eNew.End = e1Next.Start;

      var newFacePointCount = InitOwnerFaceAndCountPoints(eNew, newFace);
      var thisPointCount = m_NumPoints - newFacePointCount + 2;

      newFace.Init(eNew, newFacePointCount, m_Normal);
      Init(eThis, thisPointCount, m_Normal);

      newFace.OnNewOwner(m_Owner);
   }

   private int InitOwnerFaceAndCountPoints(Edge eOnNew, Face newFace)
   {
      int c = 0;
      m_LoopEnumerator.Init(eOnNew);

      for (var e = m_LoopEnumerator.First(); e != null; e = m_LoopEnumerator.Next())
      {
         e.OwnerFace = newFace;
         c++;
      }
      return c;
   }

   public void DetachEdge(Edge e)
   {
      var pairForThis = new EdgePair(this, null);
      var eThis = pairForThis.Edge1;

      var ePrev = e.Prev;
      var eNext = e.Next;

      ePrev.InsertAfterAndBreak(eThis);
      eNext.InsertBeforeAndBreak(eThis);

      eThis.Start = ePrev.End;
      eThis.End = eNext.Start;

      eThis.OwnerFace = this;

      e.Clear();
   }

   public void OnEdgeSplit()
   {
      m_NumPoints++;
   }

   public static Edge FormSplitOnEdge(Edge e, Vector3 splitPoint)
   {
      var newPair = new EdgePair(e.OwnerFace, e.Other.OwnerFace);
      var newEdge = newPair.Edge1;

      newEdge.Start = new ShapePoint(splitPoint);
      newEdge.End = e.End;
      e.End = new ShapePoint(splitPoint);

      e.InsertAfter(newEdge);
      e.Other.InsertBefore(newEdge.Other);

      var next = newEdge.Other;

      next.OwnerFace.OnEdgeSplit();
      e.OwnerFace.OnEdgeSplit();

      return next;
   }

   public static Edge FormSplitAtPoint(Vector3 n, Vector3 P0, Edge edgeThatBridgesWithNext)
   {
      var e = edgeThatBridgesWithNext;

      var p = e.End.Point;
      e.End = new ShapePoint(p);
      e.Next.Start = new ShapePoint(p);

      return e.End.Split(P0, n, e);
   }

   private Edge NormalSplit(Vector3 n, Vector3 P0, Edge start, Edge end)
   {
      Edge next = null; // must form split with next

      var Pprev = start.Start.Point;
      var compPrev = Vector3.Dot(n, P0 - Pprev);

      for (Edge curr = start; curr != end; curr = curr.Next)
      {
         var P = curr.End.Point;
         var comp = Vector3.Dot(n, P0 - P);

         if (comp * compPrev <= 0.0f)
         {
            if (Mathf.Abs(comp) <= Utils.PointInPlaneTol)
            {
               next = FormSplitAtPoint(n, P0, curr);
               SplitInHalf(end, curr);
               break;
            }
            else if (Mathf.Abs(compPrev) <= Utils.PointInPlaneTol)
            {
               next = FormSplitAtPoint(n, P0, curr.Prev);
               SplitInHalf(end, curr);
               break;
            }
            else
            {
               Vector3 intPoint;
               Utils.LinePlaneIntersect(n, P0, Pprev, P, out intPoint);
               next = FormSplitOnEdge(curr, intPoint);
               SplitInHalf(end, curr);
               break;
            }
         }

         compPrev = comp;
         Pprev = P;
      }

      return next;
   }

   private Edge ParralelSplit(Vector3 n, Vector3 P0, Edge edgeThatBridgesWithNext, Edge toDetach)
   {
      var next = FormSplitAtPoint(n, P0, edgeThatBridgesWithNext);
      DetachEdge(toDetach);
      return next;
   }

   public Edge Split(Vector3 n, Vector3 P0, Edge edgeThatFormsSplitWithNext)
   {
      var e0 = edgeThatFormsSplitWithNext;

      if (Utils.PointIsInPlane(n, P0, e0.Next.End.Point))
      {
         return ParralelSplit(n, P0, e0.Next, e0.Next);
      }
      else if (Utils.PointIsInPlane(n, P0, e0.Start.Point))
      {
         return ParralelSplit(n, P0, e0.Prev, e0);
      }
      else
      {
         return NormalSplit(n, P0, e0.Next.Next, e0);
      }
   }

   private Edge NextOnOpenHole(Edge e)
   {
      return e.Other.Prev.Other.Prev.Other;
   }

   public void PutOntoOpenHole(Edge edgeOnHole, Vector3 normal)
   {
      var x = edgeOnHole;
      var xNext = NextOnOpenHole(x);
      var numPoints = 0;

      do
      {
         x.InsertAfterAndBreak(xNext);
         x.OwnerFace = this;
         numPoints++;

         x = xNext;
         xNext = NextOnOpenHole(xNext);

      } while(x != edgeOnHole);

      Init(edgeOnHole, numPoints, normal);
   }

   public void AddMesh(FaceMeshPool pool)
   {
      if (m_Mesh != null && m_Mesh.NumPoints != m_NumPoints)
      {
         pool.ReturnMesh(m_Mesh);
         m_Mesh = null;
      }

      if (m_Mesh == null)
         m_Mesh = pool.GetMesh(m_NumPoints);

      m_Mesh.transform.SetParent(m_Owner.transform, false);
      m_Mesh.transform.localPosition = Vector3.zero;
      m_Mesh.transform.localRotation = Quaternion.identity;

      m_Points.Clear();
      m_PointIndicies.Clear();

      m_LoopEnumerator.Init(m_Head);
      for (var e = m_LoopEnumerator.First(); e != null; e = m_LoopEnumerator.Next())
      {
         m_Points.Add(e.End.Point);
         m_PointIndicies.Add(e.End.Index);
      }

      m_Mesh.SetVerts(m_Points);
      m_Mesh.SetNormal(m_Normal);
   }

   public void AssignToShape(Vector3 n, Vector3 P0, Shape above, Shape below)
   {
      Shape shape = null;

      m_LoopEnumerator.Init(m_Head);
      for (var e = m_LoopEnumerator.First(); e != null; e = m_LoopEnumerator.Next())
      {
         var x = Vector3.Dot(e.End.Point - P0, n);

         if (Mathf.Abs(x) > Utils.PointInPlaneTol)
         {
            shape = x > 0.0f ? above : below;
            break;
         }
      }

      if (shape == null)
         Debug.LogError("Unable to assign shape to face!!!");

      OnNewOwner(shape);

      m_LoopEnumerator.Init(m_Head);
      for (var e = m_LoopEnumerator.First(); e != null; e = m_LoopEnumerator.Next())
      {
         e.OwnerPair.OnNewOwner(shape);
      }
   }
}
