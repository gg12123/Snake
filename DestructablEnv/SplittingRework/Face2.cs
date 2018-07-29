using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Face2
{
   private List<Point2> m_Points;
   private List<Vector3> m_CachedPoints = new List<Vector3>();

   private Vector3 m_Normal;
   private FaceMesh m_Mesh;

   private bool m_InUse = true;

   public Vector3 Normal { get { return m_Normal; } }
   public Vector3 Point0 { get { return m_CachedPoints[0]; } }

   public Face2(List<Point2> points, Vector3 normal)
   {
      m_Points = points;
      m_Normal = normal;
   }

   public int CountNumInside()
   {
      var n = 0;
      for (int i = 0; i < m_Points.Count; i++)
      {
         if (m_Points[i].PlaneRelationship == PointPlaneRelationship.Inside)
            n++;
      }
      return n;
   }

   public bool IsAbovePoint(Vector3 P, out float amountAbove)
   {
      var P0 = m_CachedPoints[0];
      amountAbove = Vector3.Dot(P0 - P, m_Normal);
      return (amountAbove > 0.0f);
   }

   public bool IsCollidedWithEdge(Vector3 P0, Vector3 P1, ref Vector3 collPoint, ref Vector3 collNormal)
   {
      var closestEdge = -1;
      var closestComp = Mathf.NegativeInfinity;

      if (Utils.LinePlaneIntersect(Normal, Point0, P0, P1, out collPoint))
      {
         for (int i = 0; i < m_CachedPoints.Count; i++)
         {
            var next = (i + 1) % m_CachedPoints.Count;

            var Pi = m_CachedPoints[i];
            var Pnext = m_CachedPoints[next];

            var n = Vector3.Cross(Pnext - Pi, m_Normal);
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

         var Pc = m_CachedPoints[closestEdge];
         var PcNext = m_CachedPoints[(closestEdge + 1) % m_CachedPoints.Count];

         collNormal = Vector3.Cross(P0 - P1, Pc - PcNext).normalized;
         return true;
      }
      return false;
   }

   public Vector3 RandomEdgePoint()
   {
      var i = Random.Range(0, m_Points.Count);
      var n = (i + 1) % m_Points.Count;

      return m_Points[i].Point + Random.Range(0.0f, 1.0f) * (m_Points[n].Point - m_Points[i].Point);
   }

   public void Split(NewPointsGetter newPoints, Shape2 shapeAbove, Shape2 shapeBelow)
   {
      m_InUse = false;

      var abovePoints = new List<Point2>();
      var belowPoints = new List<Point2>();

      var newAboveEdge = new Edge2();
      var newBelowEdge = new Edge2();

      for (int i = 0; i < m_Points.Count; i++)
      {
         var next = (i + 1) % m_Points.Count;

         var p1 = m_Points[i];
         var p2 = m_Points[next];

         if (p1.PlaneRelationship == PointPlaneRelationship.Above)
         {
            abovePoints.Add(p1);
         }
         else if (p1.PlaneRelationship == PointPlaneRelationship.Below)
         {
            belowPoints.Add(p1);
         }
         else
         {
            var a = newPoints.GetPointAbove(p1);
            var b = newPoints.GetPointBelow(p1);

            abovePoints.Add(a);
            belowPoints.Add(b);

            newAboveEdge.AddPoint(a);
            newBelowEdge.AddPoint(b);
         }

         if (Point2.PointsBridgePlane(p1, p2))
         {
            var a = newPoints.GetPointAbove(p1, p2);
            var b = newPoints.GetPointBelow(p1, p2);

            abovePoints.Add(a);
            belowPoints.Add(b);

            newAboveEdge.AddPoint(a);
            newBelowEdge.AddPoint(b);
         }
      }

      ProcessNewFace(shapeAbove, abovePoints, newAboveEdge);
      ProcessNewFace(shapeBelow, belowPoints, newBelowEdge);
   }

   private Face2 GetNewFace(List<Point2> points, Vector3 normal)
   {
      if (!m_InUse)
      {
         m_InUse = true;
         m_Points = points; // return old list to some pool
         m_Normal = normal;
         return this;
      }
      return new Face2(points, normal);
   }

   private void ProcessNewFace(Shape2 shape, List<Point2> points, Edge2 newEdge)
   {
      if (DefinesNewFace(points))
      {
         shape.Faces.Add(GetNewFace(points, m_Normal));

         if (IsNewlyFormedEdge(newEdge))
            shape.AddNewEdgeFromFaceSplit(newEdge);
      }
   }

   private bool IsNewlyFormedEdge(Edge2 e)
   {
      return (e.EdgeP1 != null);
   }

   private bool DefinesNewFace(List<Point2> points)
   {
      return (points.Count > 2);
   }

   public void AddMeshAndCachePoints(FaceMeshPool pool, Transform owner)
   {
      if (m_Mesh != null && m_Mesh.NumPoints != m_Points.Count)
      {
         pool.ReturnMesh(m_Mesh);
         m_Mesh = null;
      }

      if (m_Mesh == null)
         m_Mesh = pool.GetMesh(m_Points.Count);

      m_Mesh.transform.SetParent(owner, false);
      m_Mesh.transform.localPosition = Vector3.zero;
      m_Mesh.transform.localRotation = Quaternion.identity;

      m_CachedPoints.Clear();

      for (int i = 0; i < m_Points.Count; i++)
         m_CachedPoints.Add(m_Points[i].Point);

      m_Mesh.SetVerts(m_CachedPoints);
      m_Mesh.SetNormal(m_Normal);
   }
}
