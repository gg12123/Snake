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

   public Face2 Create(Vector3 finalFaceNormal)
   {
      var prev = m_Head.LinkedPoint1; // defines loop direction - choose this based on the required winding
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
