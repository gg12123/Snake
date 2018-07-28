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

   public Point2(Vector3 point)
   {
      Point = point;
   }

   public void UpdateRelationship(Vector3 P0, Vector3 n)
   {

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
