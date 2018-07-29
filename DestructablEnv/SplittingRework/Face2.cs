using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Face2
{
   private List<Point2> m_Points;
   private List<Vector3> m_CachedPoints = new List<Vector3>();

   private Vector3 m_Normal;
   private FaceMesh m_Mesh;

   public Vector3 Normal { get { return m_Normal; } }

   public Face2(List<Point2> points, Vector3 normal)
   {
      m_Points = points;
      m_Normal = normal;
   }

   public Vector3 RandomEdgePoint()
   {
      var i = Random.Range(0, m_Points.Count);
      var n = (i + 1) % m_Points.Count;

      return m_Points[i].Point + Random.Range(0.0f, 1.0f) * (m_Points[n].Point - m_Points[i].Point);
   }

   public void Split(NewPointsGetter newPoints, Shape2 shapeAbove, Shape2 shapeBelow)
   {
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

   private void ProcessNewFace(Shape2 shape, List<Point2> points, Edge2 newEdge)
   {
      if (DefinesNewFace(points))
      {
         shape.Faces.Add(new Face2(points, m_Normal));

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

   public void RetrunMesh(FaceMeshPool pool)
   {
      pool.ReturnMesh(m_Mesh);
   }

   public void AddMeshAndCachePoints(FaceMeshPool pool, Transform owner)
   {
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
