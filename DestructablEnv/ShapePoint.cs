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

   private Edge GetNextAboutPoint(Edge e)
   {
      return e.Next.Other;
   }

   public Edge Split(Vector3 P0, Vector3 n, Edge edgeThatBridgesWithNext)
   {
      var e0 = edgeThatBridgesWithNext;
      var e1 = edgeThatBridgesWithNext.Next;

      Edge nextEdge = null; // this must be on the opposite side of the plane to the input edge and form the split with next

      bool onE1Side = true;

      var curr = GetNextAboutPoint(GetNextAboutPoint(e0));
      var prev = GetNextAboutPoint(e0);

      var end = prev;

      while (curr != end)
      {
         var currA = Vector3.Dot(n, curr.Start.Point - P0);
         var prevA = Vector3.Dot(n, prev.Start.Point - P0);

         if (onE1Side && (currA * prevA <= 0.0f))
         {
            nextEdge = prev;
            onE1Side = false;
         }

         curr.End = onE1Side ? e1.Start : e0.End;

         prev = curr;
         curr = GetNextAboutPoint(curr);
      }

      if (onE1Side)
         Debug.LogError("error when splitting point");

      return nextEdge;
   }
}
