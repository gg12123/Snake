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
      EdgePoints = new List<Vector3>();
      EdgePairs = new List<EdgePair>();
      Points = new List<Vector3>();
      Faces = new List<Face>();

      m_ShapePool = GetComponentInParent<ShapePool>();
      m_MeshPool = GetComponentInParent<FaceMeshPool>();
   }

   private Vector3 CalculateCentre(List<EdgePair> pairs)
   {
      Vector3 centre = Vector3.zero;

      for (int i = 0; i < pairs.Count; i++)
         centre += pairs[i].Midpoint();

      return centre / pairs.Count;
   }

   private void Divide(Shape shapeAbove, Shape shapeBelow, Vector3 P0, Vector3 n)
   {
      var facesToSplit = new List<Face>();

      for (int i = 0; i < EdgePairs.Count; i++)
      {
         EdgePairs[i].Clip(n, P0, shapeBelow.EdgePairs, shapeAbove.EdgePairs, facesToSplit);
      }

      Edge eAbove = null;
      Edge eBelow = null;

      for (int i = 0; i < facesToSplit.Count; i++)
      {
         facesToSplit[i].Split(n, P0, shapeBelow.EdgePairs, shapeAbove.EdgePairs, out eAbove, out eBelow);
      }

      var f1 = new Face();
      var f2 = new Face();

      f1.PutOntoOpenHole(eAbove, -n);
      f2.PutOntoOpenHole(eBelow, n);
   }

   private void InitNewShape(Shape shape)
   {
      var centre = CalculateCentre(shape.EdgePairs);

      var pairs = shape.EdgePairs;
      for (int i = 0; i < pairs.Count; i++)
      {
         pairs[i].OnClippingFinished(centre, shape);
      }

      shape.transform.position = transform.TransformPoint(centre);
      shape.transform.rotation = transform.rotation;

      var faces = shape.Faces;
      for (int i = 0; i < faces.Count; i++)
      {
         faces[i].AddMesh(m_MeshPool);
      }
   }

   public void Split(Vector3 P0, Vector3 n)
   {
      var shapeAbove = m_ShapePool.GetShape();
      var shapeBelow = m_ShapePool.GetShape();

      Divide(shapeAbove, shapeBelow, P0, n);

      InitNewShape(shapeAbove);
      InitNewShape(shapeBelow);

      m_ShapePool.Return(this);
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
         }
      }
   }

   private void SplitFacesAndEdges(Vector3 P0, Vector3 n, out Edge eNearOpen1, out Edge eNearOpen2)
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
         startFace.DetachEdge(startEdgeStartFace);

         eNearOpen1 = startEdgeStartFace.Next.Other;
         eNearOpen2 = startEdgeStartFace.Other.Next.Other;
      }
      else
      {
         startFace.SplitInHalf(curr, startEdgeStartFace);

         eNearOpen1 = curr;
         eNearOpen2 = startEdgeStartFace;
      }
   }

   public void Split2(Vector3 P0, Vector3 n)
   {
      Edge nearOpen1;
      Edge nearOpen2;

      SplitFacesAndEdges(P0, n, out nearOpen1, out nearOpen2);

      var shapeAbove = m_ShapePool.GetShape();
      var shapeBelow = m_ShapePool.GetShape();

      for (int i = 0; i < Faces.Count; i++)
         Faces[i].AssignToShape(n, P0, shapeAbove, shapeBelow);

      var f1 = new Face();
      f1.PutOntoOpenHole(nearOpen1.Next.Other, nearOpen1.OwnerFace.OwnerShape == shapeAbove ? -n : n);
      f1.OnNewOwner(nearOpen1.OwnerFace.OwnerShape);

      var f2 = new Face();
      f2.PutOntoOpenHole(nearOpen2.Next.Other, nearOpen2.OwnerFace.OwnerShape == shapeAbove ? -n : n);
      f2.OnNewOwner(nearOpen1.OwnerFace.OwnerShape);
   }

   private void InitNewShape2(Shape shape)
   {
      var centre = CalculateCentre(shape.EdgePairs);

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
