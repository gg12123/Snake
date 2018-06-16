using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Face : IEnumerable<Edge>
{
   private Shape m_Owner;
   private FaceMesh m_Mesh;
   private int m_NumPoints;
   private Vector3 m_Normal;

   private IEdgeEnumerator m_LoopEnumerator;

   private Edge m_Head;

   private List<Vector3> m_Points;

   public Shape OwnerShape { get { return m_Owner; } }

   public Face()
   {
      m_Points = new List<Vector3>();
   }

   public void Init(Edge head, int numPoints, Vector3 normal)
   {
      m_Head = head;
      m_NumPoints = numPoints;
      m_Normal = normal;
   }

   public void ClearOwnerShape()
   {
      m_Owner = null;
   }

   public void OnNewOwner(Shape owner)
   {
      if (m_Owner != owner)
      {
         owner.Faces.Add(this);
      }
      m_Owner = owner;
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
      var thisPointCount = m_NumPoints - newFacePointCount + 4;

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

      return newEdge.Other;
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
      Vector3 intPoint;
      Edge next = null; // must form split with next

      for (Edge curr = start; curr != end; curr = curr.Next)
      {
         if (Utils.PointIsInPlane(n, P0, curr.End.Point))
         {
            next = FormSplitAtPoint(n, P0, curr);
            SplitInHalf(end, curr);
            break;
         }
         else if (Utils.LinePlaneIntersect(n, P0, curr.Start.Point, curr.End.Point, out intPoint))
         {
            next = FormSplitOnEdge(curr, intPoint);
            SplitInHalf(end, curr);
            break;
         }
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
         x.InsertAfter(xNext);
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
      {
         m_Mesh = pool.GetMesh(m_NumPoints);
         m_Mesh.transform.parent = m_Owner.transform;
      }

      foreach (var e in this)
         m_Points.Add(e.End.Point);

      m_Mesh.SetVerts(m_Points);
      m_Mesh.SetNormal(m_Normal);
   }

   public void AssignToShape(Vector3 n, Vector3 P0, Shape above, Shape below)
   {

   }

   public IEnumerator<Edge> GetEnumerator()
   {
      var e = m_Head;

      do
      {
         yield return e;
         e = e.Prev;
      } while (e != m_Head);
   }

   IEnumerator IEnumerable.GetEnumerator()
   {
      var e = m_Head;

      do
      {
         yield return e;
         e = e.Prev;
      } while (e != m_Head);
   }
}
