using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalFaceCreator
{
   private Point2 m_Head;

   public void AddEdge(Edge2 e)
   {
      e.EdgeP1.AddLink(e.EdgeP2);
      e.EdgeP2.AddLink(e.EdgeP1);

      m_Head = e.EdgeP1;
   }

   private Point2 CalculatePrev(Vector3 finalFaceNormal)
   {
      var P0 = m_Head.LinkedPoint1.Point;
      var P1 = m_Head.Point;
      var P2 = m_Head.LinkedPoint2.Point;

      var e01 = (P1 - P0).normalized;
      var e12 = (P2 - P1).normalized;

      var c = Vector3.Cross(e01, e12);

      return Vector3.Dot(c, finalFaceNormal) > 0.0f ? m_Head.LinkedPoint1 : m_Head.LinkedPoint2;
   }

   public Face2 Create(Vector3 finalFaceNormal)
   {
      var prev = CalculatePrev(finalFaceNormal);
      var curr = m_Head;

      var points = new List<Point2>();

      do
      {
         points.Add(curr);

         var next = curr.NextLinkedPoint(prev);
         prev = curr;
         curr = next;
      } while (curr != m_Head);

      return new Face2(points, finalFaceNormal);
   }
}
