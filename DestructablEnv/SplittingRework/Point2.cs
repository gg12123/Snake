using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PointPlaneRelationship
{
   Above,
   Below,
   Inside
}

public class Point2
{
   public Vector3 Point { get; private set; }
   public PointPlaneRelationship PlaneRelationship { get; private set; }

   public Point2 LinkedPoint1 { get; private set; }
   public Point2 LinkedPoint2 { get; private set; }

   public Point2(Vector3 point)
   {
      Point = point;
   }

   public Point2 NextLinkedPoint(Point2 prev)
   {
      if (LinkedPoint1 == prev)
      {
         return LinkedPoint2;
      }
      else if (LinkedPoint2 == prev)
      {
         return LinkedPoint1;
      }

      Debug.LogError("point link error");
      return null;
   }

   public void AddLink(Point2 linkedPoint)
   {
      if (LinkedPoint1 == null)
      {
         LinkedPoint1 = linkedPoint;
         return;
      }
      else if (LinkedPoint2 == null)
      {
         LinkedPoint2 = linkedPoint;
         return;
      }

      Debug.LogError("point link error");
      return;
   }

   public void CentreAndCache(Vector3 centre, List<Vector3> points)
   {
      Point -= centre;
      points.Add(Point);
   }

   public void Split(Vector3 P0, Vector3 n, Shape2 shapeAbove, Shape2 shapeBelow, NewPointsGetter newPoints)
   {
      m_LinkedPoint1 = null;
      m_LinkedPoint2 = null;

      var comp = Vector3.Dot(Point - P0, n);

      if (Mathf.Abs(comp) <= Utils.PointInPlaneTol)
      {
         var newAbove = new Point2(Point);
         var newBelow = new Point2(Point);

         newPoints.AddPoints(this, newAbove, newBelow);

         shapeAbove.Points.Add(newAbove);
         shapeBelow.Points.Add(newBelow);

         PlaneRelationship = PointPlaneRelationship.Inside;
      }
      else if (comp > 0.0f)
      {
         shapeAbove.Points.Add(this);
      }
      else
      {
         shapeBelow.Points.Add(this);
      }
   }

   public static bool PointsBridgePlane(Point2 p1, Point2 p2)
   {

   }

   public static bool BothAbove(Point2 p1, Point2 p2)
   {

   }

   public static bool BothBelow(Point2 p1, Point2 p2)
   {

   }

   public static bool BothInside(Point2 p1, Point2 p2)
   {

   }
}
