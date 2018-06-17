using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapePoint
{
   private bool m_BeenAdded;
   private IEdgeEnumerator m_Enumerator;

   public Vector3 Point { get; set; }

   public ShapePoint(Vector3 point)
   {
      m_BeenAdded = false;
      m_Enumerator = new EdgeAboutPointWhenSplittingEnumerator();
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

   private bool PointIsMoreOnPXSide(Vector3 P0, Vector3 n, Vector3 p, Vector3 pX, Vector3 pOther)
   {
      var pAmount = Vector3.Dot(n, p - P0);
      var pXAmount = Vector3.Dot(n, pX - P0);
      var pOtherAmount = Vector3.Dot(n, pOther - P0);

      return (Mathf.Abs(pAmount - pXAmount) < Mathf.Abs(pAmount - pOtherAmount));
   }

   public Edge Split(Vector3 P0, Vector3 n, Edge edgeThatBridgesWithNext)
   {
      var e0 = edgeThatBridgesWithNext;
      var e1 = edgeThatBridgesWithNext.Next;
      var startFace = e0.OwnerFace;

      m_Enumerator.Init(e0);

      Edge nextEdge = null; // this must be on the opposite side of the plane to the input edge and form the split with next
      Edge prev = m_Enumerator.First();

      for (Edge e = m_Enumerator.First(); e != null; e = m_Enumerator.Next())
      {
         if (PointIsMoreOnPXSide(P0, n, e.Start.Point, e0.Start.Point, e1.End.Point))
         {
            e.End = e0.End;
         }
         else
         {
            e.End = e1.Start;
         }

         if (e.End != prev.End && prev.OwnerFace != startFace)
         {
            nextEdge = prev;
         }

         prev = e;
      }

      return nextEdge;
   }
}
