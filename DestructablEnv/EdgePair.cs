using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgePair
{
   public Edge Edge1 { get; private set; }
   public Edge Edge2 { get; private set; }

   private ShapePoint m_StartFor1;
   private ShapePoint m_EndFor1;

   public EdgePair(Face edge1Face, Face edge2Face)
   {
      Edge1 = new Edge(this, edge1Face);
      Edge2 = new Edge(this, edge2Face);
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

   private void AddToCorrectList(Vector3 n, Vector3 P0, List<EdgePair> edgesBelow, List<EdgePair> edgesAbove)
   {
      // use the midpoint because it wont be a borderline case
      var pointOnThis = 0.5f * (m_StartFor1.Point + m_EndFor1.Point);

      if (Utils.PointIsAbovePlane(n, P0, pointOnThis))
      {
         edgesAbove.Add(this);
      }
      else
      {
         edgesBelow.Add(this);
      }
   }

   public Vector3 Midpoint()
   {
      return 0.5f * (m_StartFor1.Point + m_EndFor1.Point);
   }

   public void OnClippingFinished(List<Vector3> edgePoints, List<Vector3> points, Vector3 centre, Shape newOwner, List<Face> newOwnersFaces)
   {
      Edge1.OwnerFace.OnNewOwner(newOwner, newOwnersFaces);
      Edge2.OwnerFace.OnNewOwner(newOwner, newOwnersFaces);

      m_StartFor1.CentreAndAdd(points, centre);
      m_EndFor1.CentreAndAdd(points, centre);

      edgePoints.Add(m_EndFor1.Point);
      edgePoints.Add(m_StartFor1.Point);
   }

   private void OnClip()
   {
      m_StartFor1.Reset();
      m_EndFor1.Reset();

      Edge1.OwnerFace.ClearOwnerShape();
      Edge2.OwnerFace.ClearOwnerShape();
   }

   public void Clip(Vector3 n, Vector3 P0, List<EdgePair> edgesBelow, List<EdgePair> edgesAbove, List<Face> facesToBeSplit)
   {
      OnClip();

      Vector3 intPoint;

      if (Utils.LinePlaneIntersect(n, P0, m_StartFor1.Point, m_EndFor1.Point, out intPoint))
      {
         var newEdgePair = new EdgePair(Edge1.OwnerFace, Edge2.OwnerFace);

         Edge1.InsertBefore(newEdgePair.Edge1);
         Edge2.InsertAfter(newEdgePair.Edge2);

         var oldEdge1Start = Edge1.Start;

         // setting up edge1 will automatically setup edge2

         Edge1.Start = new ShapePoint(intPoint);

         newEdgePair.Edge1.Start = oldEdge1Start;
         newEdgePair.Edge1.End = new ShapePoint(intPoint); // make new point becasue the shapes will be disconnected

         Edge1.OwnerFace.AddEdgeThatFormsSplitWithNext(newEdgePair.Edge1, facesToBeSplit);
         Edge2.OwnerFace.AddEdgeThatFormsSplitWithNext(Edge2, facesToBeSplit);

         newEdgePair.AddToCorrectList(n, P0, edgesBelow, edgesAbove);
      }
      AddToCorrectList(n, P0, edgesBelow, edgesAbove);
   }

   public Edge Other(Edge e)
   {
      if (e == Edge1)
      {
         return Edge1;
      }
      else if (e == Edge2)
      {
         return Edge2;
      }
      else
      {
         throw new System.Exception("Cannot get to other on edge pair");
      }
   }
}
