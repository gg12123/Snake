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

   private bool CalculateSplitPlaneNormal(Vector3 P0, Vector3 collNormal, out Vector3 n)
   {
      n = Vector3.zero;

      var numTries = 0;
      while (numTries < 10)
      {
         var p = Random.Range(0.0f, 0.5f) * CachedPoints[Random.Range(0, CachedPoints.Count)];
         var toP0 = (P0 - p).normalized;

         if (Mathf.Abs(Vector3.Dot(collNormal, toP0)) < 0.9f)
         {
            n = Vector3.Cross(toP0, collNormal).normalized;
            return true;
         }

         numTries++;
      }
      return false;
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

   public bool Split(Vector3 collPointWs, Vector3 collNormalWs, Shape2 shapeAbove, Shape2 shapeBelow)
   {
      var P0 = transform.InverseTransformPoint(collPointWs);
      var collNormalLocal = transform.InverseTransformDirection(collNormalWs);

      Vector3 n;
      if (CalculateSplitPlaneNormal(P0, collNormalLocal, out n))
      {
         shapeAbove.Clear();
         shapeBelow.Clear();

         foreach (var p in Points)
            p.Split(P0, n, shapeAbove, shapeBelow, m_NewPointsGetter);

         foreach (var e in Edges)
            e.Split(P0, n, m_NewPointsGetter, shapeAbove, shapeBelow);

         foreach (var f in Faces)
         {
            f.Split(m_NewPointsGetter, shapeAbove, shapeBelow);
            f.RetrunMesh(m_MeshPool);
         }

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
