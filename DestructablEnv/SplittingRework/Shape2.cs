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
      m_FinalFaceCreator.AddEdge(e);
   }

   private int RandomEdgePointIndex()
   {
      return Random.Range(0, CachedEdgePoints.Count / 2) * 2;
   }

   private Vector3 CalculateSplitPlaneNormal(Vector3 P0, Vector3 collNormal)
   {
      while (true)
      {
         var index = RandomEdgePointIndex();

         var p1 = CachedEdgePoints[index];
         var p2 = CachedEdgePoints[index + 1];

         var mid = (p1 + p2) / 2.0f;

         if (Vector3.Distance(P0, mid) > 0.01)
         {
            var toP0 = (P0 - mid).normalized;
            var edgeDir = (p1 - p2).normalized;

            if (Mathf.Abs(Vector3.Dot(toP0, edgeDir)) < 0.9f)
            {
               return Vector3.Cross(collNormal, toP0).normalized;
            }
         }
      }
   }

   private Vector3 CalculateCentre()
   {
      var c = Vector3.zero;

      for (int i = 0; i < Points.Count; i++)
         c += Points[i].Point;

      return c / Points.Count;
   }

   public void CentreAndCache()
   {
      var c = CalculateCentre();

      foreach (var p in Points)
         p.CentreAndCache(c, CachedPoints);

      foreach (var e in Edges)
         e.Cache(CachedEdgePoints);
   }

   public void InitFaces(Vector3 finalFaceNormal)
   {
      Faces.Add(m_FinalFaceCreator.Create(finalFaceNormal));

      foreach (var f in Faces)
         f.AddMeshAndCachePoints(m_MeshPool, transform);
   }

   public void Split(Vector3 collPointWs, Vector3 collNormalWs, Shape2 shapeAbove, Shape2 shapeBelow)
   {
      var P0 = transform.InverseTransformPoint(collPointWs);
      var collNormalLocal = transform.InverseTransformDirection(collNormalWs);

      var n = CalculateSplitPlaneNormal(P0, collNormalLocal);

      shapeAbove.Clear();
      shapeBelow.Clear();

      foreach (var p in Points)
         p.Split(P0, n, shapeAbove, shapeBelow, m_NewPointsGetter);

      foreach (var e in Edges)
         e.Split(P0, n, m_NewPointsGetter, shapeAbove, shapeBelow);

      foreach (var f in Faces)
         f.Split(m_NewPointsGetter, shapeAbove, shapeBelow);

      shapeAbove.CentreAndCache();
      shapeBelow.CentreAndCache();

      shapeAbove.InitFaces(-n);
      shapeBelow.InitFaces(n);
   }
}
