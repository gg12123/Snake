using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgePair
{
   public Edge Edge1 { get; private set; }
   public Edge Edge2 { get; private set; }

   private ShapePoint m_StartFor1;
   private ShapePoint m_EndFor1;

   private bool m_AddedToShape;

   public EdgePair(Face edge1Face, Face edge2Face)
   {
      Edge1 = new Edge(this, edge1Face);
      Edge2 = new Edge(this, edge2Face);
      m_AddedToShape = false;
   }

   public ShapePoint GetStart(Edge getter)
   {
      if (getter == Edge1)
      {
         return m_StartFor1;
      }
      else if (getter == Edge2)
      {
         return m_EndFor1;
      }
      else
      {
         Debug.LogError("Unkown edge trying to get on pair");
         return null;
      }
   }

   public ShapePoint GetEnd(Edge getter)
   {
      if (getter == Edge1)
      {
         return m_EndFor1;
      }
      else if (getter == Edge2)
      {
         return m_StartFor1;
      }
      else
      {
         Debug.LogError("Unkown edge trying to get on pair");
         return null;
      }
   }

   public void SetStart(Edge setter, ShapePoint val)
   {
      if (setter == Edge1)
      {
         m_StartFor1 = val;
      }
      else if (setter == Edge2)
      {
         m_EndFor1 = val;
      }
      else
      {
         Debug.LogError("Unkown edge trying to set on pair");
      }
   }

   public void SetEnd(Edge setter, ShapePoint val)
   {
      if (setter == Edge1)
      {
         m_EndFor1 = val;
      }
      else if (setter == Edge2)
      {
         m_StartFor1 = val;
      }
      else
      {
         Debug.LogError("Unkown edge trying to set on pair");
      }
   }

   public Vector3 Midpoint()
   {
      return 0.5f * (m_StartFor1.Point + m_EndFor1.Point);
   }

   public void OnNewOwner(Shape owner)
   {
      if (!m_AddedToShape)
      {
         m_StartFor1.Reset();
         m_EndFor1.Reset();

         owner.EdgePairs.Add(this);
         m_AddedToShape = true;
      }
   }

   public void OnSplittingFinished(Vector3 centre, Shape owner)
   {
      m_StartFor1.CentreAndAdd(owner.Points, centre);
      m_EndFor1.CentreAndAdd(owner.Points, centre);

      owner.EdgePoints.Add(m_EndFor1.Point);
      owner.EdgePoints.Add(m_StartFor1.Point);

      m_AddedToShape = false;
   }

   public Edge Other(Edge e)
   {
      if (e == Edge1)
      {
         return Edge2;
      }
      else if (e == Edge2)
      {
         return Edge1;
      }
      else
      {
         throw new System.Exception("Cannot get to other on edge pair");
      }
   }
}
