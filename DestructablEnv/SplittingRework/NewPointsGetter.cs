using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewPointsGetter : MonoBehaviour
{
   private class PointPair
   {
      public Point2 Above { get; private set; }
      public Point2 Below { get; private set; }

      public void Set(Point2 a, Point2 b)
      {
         Above = a;
         Below = b;
      }
   }

   private PointPair[,] m_PointsAlongEdges;
   private PointPair[] m_PointsOnPoints;

   private const int MaxNumPoints = 50;

   private void Awake()
   {
      m_PointsAlongEdges = new PointPair[MaxNumPoints, MaxNumPoints];
      m_PointsOnPoints = new PointPair[MaxNumPoints];

      for (int i = 0; i < MaxNumPoints; i++)
      {
         for (int j = 0; j < MaxNumPoints; j++)
            m_PointsAlongEdges[i, j] = new PointPair();

         m_PointsOnPoints[i] = new PointPair();
      }
   }

   private PointPair Get(Point2 existing1, Point2 existing2)
   {
      if (existing1.PlaneRelationship == PointPlaneRelationship.Above)
      {
         return m_PointsAlongEdges[existing1.Id, existing2.Id];
      }
      return m_PointsAlongEdges[existing2.Id, existing1.Id];
   }

   public void AddPoints(Point2 existing1, Point2 existing2, Point2 newAbove, Point2 newBelow)
   {
      Get(existing1, existing2).Set(newAbove, newBelow);
   }

   public void AddPoints(Point2 inside, Point2 newAbove, Point2 newBelow)
   {
      m_PointsOnPoints[inside.Id].Set(newAbove, newBelow);
   }

   public Point2 GetPointAbove(Point2 existing1, Point2 existing2)
   {
      return Get(existing1, existing2).Above;
   }

   public Point2 GetPointBelow(Point2 existing1, Point2 existing2)
   {
      return Get(existing1, existing2).Below;
   }

   public Point2 GetPointAbove(Point2 inside)
   {
      return m_PointsOnPoints[inside.Id].Above;
   }

   public Point2 GetPointBelow(Point2 inside)
   {
      return m_PointsOnPoints[inside.Id].Below;
   }
}
