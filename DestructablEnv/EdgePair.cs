using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgePair
{
   public Edge Edge1 { get; private set; }
   public Edge Edge2 { get; private set; }

   private ShapePoint m_StartFor1;
   private ShapePoint m_EndFor1;

   private Shape m_Owner;

   public EdgePair(Face edge1Face, Face edge2Face)
   {
      Edge1 = new Edge(this, edge1Face);
      Edge2 = new Edge(this, edge2Face);

      Edge1.InitOther(Edge2);
      Edge2.InitOther(Edge1);

      m_Owner = null;
   }

   public void RefreshPoints()
   {
      m_StartFor1 = Edge1.Start;
      m_EndFor1 = Edge1.End;
   }

   public Vector3 Midpoint()
   {
      return 0.5f * (m_StartFor1.Point + m_EndFor1.Point);
   }

   public void OnNewOwner(Shape newOwner)
   {
      if (newOwner != m_Owner)
      {
         m_StartFor1.Reset();
         m_EndFor1.Reset();

         newOwner.EdgePairs.Add(this);
         m_Owner = newOwner;
      }
   }

   public void OnSplittingFinished(Vector3 centre, Shape owner)
   {
      m_StartFor1.CentreAndAdd(owner.Points, centre);
      m_EndFor1.CentreAndAdd(owner.Points, centre);

      owner.EdgePointIndicies.Add(m_EndFor1.Index);
      owner.EdgePointIndicies.Add(m_StartFor1.Index);
   }
}
