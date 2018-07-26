using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape : MonoBehaviour
{
   public List<int> EdgePointIndicies { get; private set; }
   public List<EdgePair> EdgePairs{ get; private set; }
   public List<Vector3> Points { get; private set; }
   public List<Vector3> WorldPoints { get; private set; }
   public List<Face> Faces { get; private set; }

   private FaceMeshPool m_MeshPool;

   private void Awake()
   {
      EdgePointIndicies = new List<int>(40);
      EdgePairs = new List<EdgePair>(20);
      Points = new List<Vector3>(10);
      WorldPoints = new List<Vector3>(10);
      Faces = new List<Face>(12);

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

   public void FindCollisions(Shape other, List<Vector3> collPoints, List<Vector3> collNormals)
   {
      FindPointCollisionsWithOthersFaces(other, collPoints, collNormals);
      other.FindPointCollisionsWithOthersFaces(this, collPoints, collNormals);

      if (collNormals.Count == 0)
      {
         var P = Vector3.zero;
         var n = Vector3.zero;

         if (IsEdgeCollidedWithOthersFace(other, ref P, ref n))
         {
            collPoints.Add(P);
            collNormals.Add(n);
         }
      }
   }

   private void FindPointFaceCollision(List<Face> faces, Vector3 P, List<Vector3> collPoints, List<Vector3> collNormals)
   {
      var smallestAmount = Mathf.Infinity;
      Face closestFace = null;

      for (int j = 0; j < faces.Count; j++)
      {
         float amount;
         if (!faces[j].IsAbovePoint(P, out amount))
            return;

         if (amount < smallestAmount)
         {
            closestFace = faces[j];
            smallestAmount = amount;
         }
      }

      collPoints.Add(P);
      collNormals.Add(closestFace.NormalWorld);
   }

   public void FindPointCollisionsWithOthersFaces(Shape other,  List<Vector3> collPoints, List<Vector3> collNormals)
   {
      var faces = other.Faces;

      for (int i = 0; i < WorldPoints.Count; i++)
         FindPointFaceCollision(faces, WorldPoints[i], collPoints, collNormals);
   }

   public bool IsEdgeCollidedWithOthersFace(Shape other, ref Vector3 collPoint, ref Vector3 collNormal)
   {
      var faces = other.Faces;

      for (int i = 0; i < EdgePointIndicies.Count; i += 2)
      {
         var P0 = WorldPoints[EdgePointIndicies[i]];
         var P1 = WorldPoints[EdgePointIndicies[i + 1]];

         for (int j = 0; j < faces.Count; j++)
         {
            var face = faces[j];

            var P0comp = Vector3.Dot(face.NormalWorld, P0 - face.P0World);
            var P1comp = Vector3.Dot(face.NormalWorld, P1 - face.P0World);

            if (P0comp > 0.0f && P1comp > 0.0f)
            {
               break;
            }
            else if (P0comp < 0.0f && P1comp < 0.0f)
            {
               continue;
            }

            if (face.IsCollidedWithEdge(P0, P1, ref collPoint, ref collNormal))
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

   private void FindStartFaceAndEdge(Vector3 P0, Vector3 collNormal, out Edge startEdge, out Edge startEdgeOnStartFace, out Face startFace, out Vector3 n)
   {
      var found = false;

      n = Vector3.zero;
      startEdge = startEdgeOnStartFace = null;
      startFace = null;

      while (!found)
      {
         var edge = EdgePairs[Random.Range(0, EdgePairs.Count)].Edge1;
         var mid = (edge.Start.Point + edge.End.Point) / 2.0f;

         if (Vector3.Distance(P0, mid) > 0.01)
         {
            var toP0 = (P0 - mid).normalized;
            var edgeDir = (edge.Start.Point - edge.End.Point).normalized;

            if (Mathf.Abs(Vector3.Dot(toP0, edgeDir)) < 0.9f)
            {
               found = true;

               n = Vector3.Cross(collNormal, toP0).normalized;

               startFace = edge.OwnerFace;
               startEdgeOnStartFace = edge;
               startEdge = Face.FormSplitOnEdge(edge, mid);
            }
         }
      }
   }

   private void DoDetachEdge(Edge toDetach, Face startFace, out Edge ePointsToOpen1, out Edge ePointsToOpen2)
   {
      ePointsToOpen1 = toDetach.Next.Other;
      ePointsToOpen2 = toDetach.Other.Next.Other;

      startFace.DetachEdge(toDetach);
   }

   private void SplitFacesAndEdges(Vector3 P0, Vector3 collNormal, out Edge ePointsToOpen1, out Edge ePointsToOpen2, out Vector3 n)
   {
      Face startFace;
      Edge startEdge;
      Edge startEdgeStartFace;

      FindStartFaceAndEdge(P0, collNormal, out startEdge, out startEdgeStartFace, out startFace, out n);

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
      EdgePointIndicies.Clear();
      EdgePairs.Clear();
   }

   public void Split(Vector3 collPointWs, Vector3 collNormalWs, Shape shapeAbove, Shape shapeBelow)
   {
      var P0 = transform.InverseTransformPoint(collPointWs);
      var collNormalLocal = transform.InverseTransformDirection(collNormalWs);

      Edge pointsToOpen1;
      Edge pointsToOpen2;
      Vector3 n;

      SplitFacesAndEdges(P0, collNormalLocal, out pointsToOpen1, out pointsToOpen2, out n);

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
