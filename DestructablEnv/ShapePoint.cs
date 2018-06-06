using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapePoint
{
   private bool m_BeenAdded;
   public Vector3 Point { get; set; }

   public ShapePoint(Vector3 point)
   {
      m_BeenAdded = false;
      Point = point;
   }

   public void Reset()
   {
      m_BeenAdded = false;
   }

   public void CentreAndAdd(List<Vector3> shapesPoints, Vector3 centre)
   {
      if (!m_BeenAdded)
      {
         Point -= centre;
         shapesPoints.Add(Point);
         m_BeenAdded = true;
      }
   }
}
