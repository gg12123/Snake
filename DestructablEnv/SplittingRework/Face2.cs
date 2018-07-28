using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Face2
{
   private List<Point2> m_Points;
   private Vector3 m_Normal;

   public Face2(List<Point2> points, Vector3 normal)
   {

   }

   private bool PointsBridgePlane(Point2 p1, Point2 p2)
   {

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
            newBelowEdge.AddPoint(a);
         }

         if (PointsBridgePlane(p1, p2))
         {
            var a = newPoints.GetPointAbove(p1, p2);
            var b = newPoints.GetPointBelow(p1, p2);

            abovePoints.Add(a);
            belowPoints.Add(b);

            newAboveEdge.AddPoint(a);
            newBelowEdge.AddPoint(a);
         }
      }

      if (DefinesNewFace(abovePoints))
      {
         shapeAbove.Faces.Add(new Face2(abovePoints, m_Normal));
         shapeAbove.AddNewEdgeFromFaceSplit(newAboveEdge);
      }

      if (DefinesNewFace(belowPoints))
      {
         shapeAbove.Faces.Add(new Face2(belowPoints, m_Normal));
         shapeAbove.AddNewEdgeFromFaceSplit(newBelowEdge);
      }
   }

   private bool DefinesNewFace(List<Point2> points)
   {
      return (points.Count > 2);
   }
}
