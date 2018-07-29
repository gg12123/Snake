using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
   public const float PointInPlaneTol = 0.00001f;

   public static bool LinePlaneIntersect(Vector3 planeNormal, Vector3 planeP0, Vector3 lineP0, Vector3 lineP1, out Vector3 intPoint)
   {
      intPoint = Vector3.zero;

      var l = (lineP1 - lineP0).normalized;

      var num = Vector3.Dot(planeP0 - lineP0, planeNormal);
      var denom = Vector3.Dot(l, planeNormal);

      if (denom == 0.0f)
         return false;

      var u = num / denom;
      if (u >= 0.0f && (u <= Vector3.Magnitude(lineP0 - lineP1)))
      {
         intPoint = lineP0 + u * l;
         return true;
      }

      return false;
   }

   public static bool PointIsInPlane(Vector3 planeNormal, Vector3 planeP0, Vector3 point)
   {
      return (Mathf.Abs(Vector3.Dot(planeNormal, point - planeP0)) <= PointInPlaneTol);
   }
}
