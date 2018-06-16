using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEdgeEnumerator
{
   void Init(Edge e);
   Edge First();
   Edge Next();
}

public class EdgeAboutPointEnumerator : IEdgeEnumerator
{
   public void Init(Edge edgeThatBridges)
   {

   }

   public Edge First()
   {

   }

   public Edge Next()
   {

   }
}

public class ShapePoint
{
   private bool m_BeenAdded;
   private EdgeAboutPointEnumerator m_Enumerator;

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

   private bool PointIsMoreOnPXSide(Vector3 P0, Vector3 n, Vector3 p, Vector3 pX, Vector3 pOther)
   {

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
