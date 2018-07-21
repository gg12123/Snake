using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape : MonoBehaviour
{
   public List<int> EdgePoints { get; private set; }
   public List<EdgePair> EdgePairs{ get; private set; }
   public List<Vector3> Points { get; private set; }
   public List<Vector3> WorldPoints { get; private set; }
   public List<Face> Faces { get; private set; }

   private ShapePool m_ShapePool;
   private FaceMeshPool m_MeshPool;

   private void Awake()
   {
      EdgePoints = new List<int>(40);
      EdgePairs = new List<EdgePair>(20);
      Points = new List<Vector3>(10);
      WorldPoints = new List<Vector3>(10);
      Faces = new List<Face>(12);

      m_ShapePool = GetComponentInParent<ShapePool>();
      m_MeshPool = GetComponentInParent<FaceMeshPool>();
   }

   public void EnsureWorldPointsListIsBigEnough()
   {
      var currNum = WorldPoints.Count;
      var reqNum = Points.Count;

      for (int i = currNum; i < reqNum; i++)
      {
         WorldPoints.Add(Vector3.zero);
      }
   }

   public void UpdateWorldPoints()
   {
      for (int i = 0; i < Points.Count; i++)
         WorldPoints[i] = transform.TransformPoint(Points[i]);

      for (int i = 0; i < Faces.Count; i++)
         Faces[i].UpdateWorldNormal();
   }

   public bool IsCollidedWithOther(Shape other, out Vector3 collPoint)
   {
      if (EdgesCollideWithOther(other, out collPoint))
         return true;

      if (other.EdgesCollideWithOther(this, out collPoint))
         return true;

      return false;
   }

   public bool IsPointCollidedWithOthersFace(Shape other, out Vector3 collPoint, out Vector3 collNormal)
   {
      var faces = other.Faces;

      collPoint = collNormal = Vector3.zero;

      for (int i = 0; i < WorldPoints.Count; i++)
      {
         var P = WorldPoints[i];
         var coll = true;
         var smallestAmount = Mathf.Infinity;
         Face closestFace = null;

         for (int j = 0; j < faces.Count; j++)
         {
            float amount;
            if (!faces[j].IsAbovePoint(P, out amount))
            {
               coll = false;
               break;
            }

            if (amount < smallestAmount)
            {
               closestFace = faces[j];
               smallestAmount = amount;
            }
         }

         if (coll)
         {
            collPoint = P;
            collNormal = closestFace.NormalWorld;
            return true;
         }
      }
      return false;
   }

   public bool EdgesCollideWithOther(Shape other, out Vector3 collPoint)
   {
      collPoint = Vector3.zero;
      for (int i = 0; i < EdgePoints.Count; i += 2)
      {
         var P0 = transform.TransformPoint(EdgePoints[i]);
         var P1 = transform.TransformPoint(EdgePoints[i + 1]);

         for (int j = 0; j < other.Faces.Count; j++)
         {
            if (other.Faces[j].IsCollidedWithEdge(P0, P1, out collPoint))
               return true;
         }
      }
      return false;
   }

   private Vector3 CalculateCentre()
   {
      Vector3 centre = Vector3.zero;
      List<EdgePair> pairs = EdgePairs;

      for (int i = 0; i < pairs.Count; i++)
         centre += pairs[i].Midpoint();

      return centre / pairs.Count;
   }

   private void FindStartFaceAndEdge(Vector3 P0, Vector3 n, out Edge startEdge, out Edge startEdgeOnStartFace, out Face startFace)
   {
      Vector3 intPoint;

      startFace = null;
      startEdge = null;
      startEdgeOnStartFace = null;

      for (int i = 0; i < EdgePairs.Count; i++)
      {
         var edge = EdgePairs[i].Edge1;

         if (Utils.LinePlaneIntersect(n, P0, edge.Start.Point, edge.End.Point, out intPoint))
         {
            startFace = edge.OwnerFace;
            if (Utils.PointIsInPlane(n, P0, edge.Start.Point))
            {
               startEdgeOnStartFace = edge.Prev;
               startEdge = Face.FormSplitAtPoint(n, P0, startEdgeOnStartFace);
            }
            else if (Utils.PointIsInPlane(n, P0, edge.End.Point))
            {
               startEdgeOnStartFace = edge;
               startEdge = Face.FormSplitAtPoint(n, P0, startEdgeOnStartFace);
            }
            else
            {
               startEdgeOnStartFace = edge;
               startEdge = Face.FormSplitOnEdge(edge, intPoint);
            }
            break;
         }
      }
   }

   private void DoDetachEdge(Edge toDetach, Face startFace, out Edge ePointsToOpen1, out Edge ePointsToOpen2)
   {
      ePointsToOpen1 = toDetach.Next.Other;
      ePointsToOpen2 = toDetach.Other.Next.Other;

      startFace.DetachEdge(toDetach);
   }

   private void SplitFacesAndEdges(Vector3 P0, Vector3 n, out Edge ePointsToOpen1, out Edge ePointsToOpen2)
   {
      Face startFace;
      Edge startEdge;
      Edge startEdgeStartFace;

      FindStartFaceAndEdge(P0, n, out startEdge, out startEdgeStartFace, out startFace);

      var curr = startEdge;

      while (curr.OwnerFace != startFace)
      {
         curr = curr.OwnerFace.Split(n, P0, curr);
      }

      if (curr.Next == startEdgeStartFace)
      {
         DoDetachEdge(startEdgeStartFace, startFace, out ePointsToOpen1, out ePointsToOpen2);
      }
      else if (curr.Prev == startEdgeStartFace)
      {
         DoDetachEdge(curr, startFace, out ePointsToOpen1, out ePointsToOpen2);
      }
      else
      {
         startFace.SplitInHalf(curr, startEdgeStartFace);

         ePointsToOpen1 = curr;
         ePointsToOpen2 = startEdgeStartFace;
      }
   }

   public void ClearData()
   {
      Faces.Clear();
      Points.Clear();
      WorldPoints.Clear();
      EdgePoints.Clear();
      EdgePairs.Clear();
   }

   public void Split(Vector3 P0, Vector3 n)
   {
      Edge pointsToOpen1;
      Edge pointsToOpen2;

      SplitFacesAndEdges(P0, n, out pointsToOpen1, out pointsToOpen2);

      var shapeAbove = m_ShapePool.GetShape();
      var shapeBelow = m_ShapePool.GetShape();

      shapeAbove.ClearData();
      shapeBelow.ClearData();

      for (int i = 0; i < Faces.Count; i++)
         Faces[i].AssignToShape(n, P0, shapeAbove, shapeBelow);

      var f1 = new Face();
      f1.PutOntoOpenHole(pointsToOpen1.Next.Other, pointsToOpen1.OwnerFace.OwnerShape == shapeAbove ? -n : n);
      f1.OnNewOwner(pointsToOpen1.OwnerFace.OwnerShape);

      var f2 = new Face();
      f2.PutOntoOpenHole(pointsToOpen2.Next.Other, pointsToOpen2.OwnerFace.OwnerShape == shapeAbove ? -n : n);
      f2.OnNewOwner(pointsToOpen2.OwnerFace.OwnerShape);

      InitNewShape(shapeAbove);
      InitNewShape(shapeBelow);

      m_ShapePool.Return(this);
   }

   private void InitNewShape(Shape shape)
   {
      var centre = shape.CalculateCentre();

      var pairs = shape.EdgePairs;
      for (int i = 0; i < pairs.Count; i++)
         pairs[i].OnSplittingFinished(centre, shape);

      shape.transform.position = transform.TransformPoint(centre);
      shape.transform.rotation = transform.rotation;

      var faces = shape.Faces;
      for (int i = 0; i < faces.Count; i++)
         faces[i].AddMesh(m_MeshPool);

      shape.EnsureWorldPointsListIsBigEnough();
   }
}
