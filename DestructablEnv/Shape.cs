using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape : MonoBehaviour
{
   public List<Vector3> EdgePoints { get; private set; }
   public List<EdgePair> EdgePairs{ get; private set; }
   public List<Vector3> Points { get; private set; }
   public List<Face> Faces { get; private set; }

   private ShapePool m_ShapePool;
   private FaceMeshPool m_MeshPool;

   private void Awake()
   {
      EdgePoints = new List<Vector3>(40);
      EdgePairs = new List<EdgePair>(20);
      Points = new List<Vector3>(10);
      Faces = new List<Face>(12);

      m_ShapePool = GetComponentInParent<ShapePool>();
      m_MeshPool = GetComponentInParent<FaceMeshPool>();
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

   public void Split(Vector3 P0, Vector3 n)
   {
      Edge pointsToOpen1;
      Edge pointsToOpen2;

      SplitFacesAndEdges(P0, n, out pointsToOpen1, out pointsToOpen2);

      var shapeAbove = m_ShapePool.GetShape();
      var shapeBelow = m_ShapePool.GetShape();

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
   }
}
