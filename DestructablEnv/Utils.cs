using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
   public static bool PointIsAbovePlane(Vector3 planeNormal, Vector3 planeP0, Vector3 point)
   {
      return (Vector3.Dot(planeNormal, point - planeP0) > 0.0f);
   }

   public static bool LinePlaneIntersect(Vector3 planeNormal, Vector3 planeP0, Vector3 lineP0, Vector3 lineP1, out Vector3 intPoint)
   {
      intPoint = Vector3.zero;
      return false;
   }

   public static bool PointIsInPlane(Vector3 planeNormal, Vector3 planeP0, Vector3 point)
   {
   // i think it will be best to use some small tol
      return true;
   }
}
