using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape2 : MonoBehaviour
{
   public List<Face2> Faces { get; private set; }
   public List<Point2> Points { get; private set; }
   public List<Edge2> Edges { get; private set; }

   public List<Vector3> CachedPoints { get; private set; }
   public List<Vector3> CachedEdgePoints { get; private set; }

   private FinalFaceCreator m_FinalFaceCreator = new FinalFaceCreator();

   private FaceMeshPool m_MeshPool;
   private NewPointsGetter m_NewPointsGetter;

   private int m_CurrId = 0;

   private void Awake()
   {
      Faces = new List<Face2>();
      Points = new List<Point2>();
      Edges = new List<Edge2>();
      CachedPoints = new List<Vector3>();
      CachedEdgePoints = new List<Vector3>();

      m_MeshPool = GetComponentInParent<FaceMeshPool>();
      m_NewPointsGetter = GetComponentInParent<NewPointsGetter>();
   }

   public void Clear()
   {
      Faces.Clear();
      Points.Clear();
      Edges.Clear();

      CachedPoints.Clear();
      CachedEdgePoints.Clear();

      m_CurrId = 0;
   }

   public void FindCollisions(Shape2 other, List<Vector3> collPoints, List<Vector3> collNormals)
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

   private bool IsEdgeCollidedWithOthersFace(Shape2 other, ref Vector3 collPoint, ref Vector3 collNormal)
   {
      var faces = other.Faces;

      var M = other.transform.worldToLocalMatrix * transform.localToWorldMatrix;

      for (int i = 0; i < CachedEdgePoints.Count; i += 2)
      {
         Vector3 P0 = M * CachedEdgePoints[i];
         Vector3 P1 = M * CachedEdgePoints[i + 1];

         for (int j = 0; j < faces.Count; j++)
         {
            var face = faces[j];

            var P0comp = Vector3.Dot(face.Normal, P0 - face.Point0);
            var P1comp = Vector3.Dot(face.Normal, P1 - face.Point0);

            if (P0comp > 0.0f && P1comp > 0.0f)
            {
               break;
            }
            else if (P0comp < 0.0f && P1comp < 0.0f)
            {
               continue;
            }

            if (face.IsCollidedWithEdge(P0, P1, ref collPoint, ref collNormal))
            {
               collPoint = other.transform.TransformPoint(collPoint);
               collNormal = other.transform.TransformDirection(collNormal);
               return true;
            }
         }
      }
      return false;
   }

   private void FindPointCollisionsWithOthersFaces(Shape2 other, List<Vector3> collPoints, List<Vector3> collNormals)
   {
      var M = other.transform.worldToLocalMatrix * transform.localToWorldMatrix;

      for (int i = 0; i < CachedPoints.Count; i++)
         FindSinglePointFaceCollisionWithOthersFace(other, M * CachedPoints[i], collPoints, collNormals);
   }

   private void FindSinglePointFaceCollisionWithOthersFace(Shape2 other, Vector3 P, List<Vector3> collPoints, List<Vector3> collNormals)
   {
      var smallestAmount = Mathf.Infinity;
      Face2 closestFace = null;

      var faces = other.Faces;

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

      collPoints.Add(other.transform.TransformPoint(P));
      collNormals.Add(other.transform.TransformDirection(closestFace.Normal));
   }

   public void AddPoint(Point2 p)
   {
      p.Id = m_CurrId;
      m_CurrId++;
      Points.Add(p);
   }

   public void AddNewEdgeFromFaceSplit(Edge2 e)
   {
      Edges.Add(e);
      m_FinalFaceCreator.AddEdge(e);
   }

   private Vector3 CalculateSplitPlaneNormal(Vector3 P0, Vector3 collNormal)
   {
      var p = Random.Range(0.0f, 0.5f) * CachedPoints[Random.Range(0, CachedPoints.Count)];
      var toP0 = (P0 - p).normalized;
      return Vector3.ProjectOnPlane(new Vector3(-toP0.y, -toP0.z, toP0.x), toP0);
   }

   private Vector3 CalculateCentre()
   {
      var c = Vector3.zero;

      for (int i = 0; i < Points.Count; i++)
         c += Points[i].Point;

      return c / Points.Count;
   }

   public Vector3 CentreAndCache()
   {
      var c = CalculateCentre();

      foreach (var p in Points)
         p.CentreAndCache(c, CachedPoints);

      foreach (var e in Edges)
         e.Cache(CachedEdgePoints);

      return c;
   }

   public void InitFaces(Vector3 finalFaceNormal)
   {
      Faces.Add(m_FinalFaceCreator.Create(finalFaceNormal));

      foreach (var f in Faces)
         f.AddMeshAndCachePoints(m_MeshPool, transform);
   }

   private bool SplitPoints(Vector3 P0, Vector3 n, Shape2 shapeAbove, Shape2 shapeBelow)
   {
      var numInside = 0;

      foreach (var p in Points)
         p.Split(P0, n, shapeAbove, shapeBelow, m_NewPointsGetter, ref numInside);

      if (numInside >= 3)
      {
         Debug.Log(numInside.ToString() + " points inside plane");

         foreach (var f in Faces)
         {
            if (f.CountNumInside() >= 3)
            {
               Debug.LogWarning("Failed to split shape!");
               // will need to do something to stop the newly created points from leaking here.
               // could you existing inside points to get the new ones out of NewPointsGetter, then delete them.
               return false;
            }
         }
      }
      return true;
   }

   public bool Split(Vector3 collPointWs, Vector3 collNormalWs, Shape2 shapeAbove, Shape2 shapeBelow)
   {
      var P0 = transform.InverseTransformPoint(collPointWs);
      var collNormalLocal = transform.InverseTransformDirection(collNormalWs);

      var n = CalculateSplitPlaneNormal(P0, collNormalLocal);

      shapeAbove.Clear();
      shapeBelow.Clear();

      if (SplitPoints(P0, n, shapeAbove, shapeBelow))
      {
         foreach (var e in Edges)
            e.Split(P0, n, m_NewPointsGetter, shapeAbove, shapeBelow);

         foreach (var f in Faces)
            f.Split(m_NewPointsGetter, shapeAbove, shapeBelow);

         InitNewShape(shapeAbove, -n);
         InitNewShape(shapeBelow, n);

         return true;
      }
      return false;
   }

   private void InitNewShape(Shape2 shape, Vector3 finalFaceNormal)
   {
      var c = shape.CentreAndCache();

      shape.InitFaces(finalFaceNormal);

      shape.transform.position = transform.TransformPoint(c);
      shape.transform.rotation = transform.rotation;
   }
}
