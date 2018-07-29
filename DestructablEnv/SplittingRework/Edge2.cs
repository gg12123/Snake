using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge2
{
   public Point2 EdgeP1 { get; private set; }
   public Point2 EdgeP2 { get; private set; }

   public Edge2()
   {
   }

   public Edge2(Point2 p1, Point2 p2)
   {
      EdgeP1 = p1;
      EdgeP2 = p2;
   }

   public void AddPoint(Point2 p)
   {
      if (EdgeP1 == null)
      {
         EdgeP1 = p;
      }
      else if (EdgeP2 == null)
      {
         EdgeP2 = p;
      }
      else
      {
         Debug.LogError("Edge error");
      }
   }

   public void Cache(List<Vector3> edgePoints)
   {
      edgePoints.Add(EdgeP1.Point);
      edgePoints.Add(EdgeP2.Point);
   }

   private void SplitInHalf(Vector3 x, NewPointsGetter newPoints, Shape2 shapeAbove, Shape2 shapeBelow)
   {
      var a = new Point2(x);
      var b = new Point2(x);

      newPoints.AddPoints(EdgeP1, EdgeP2, a, b);

      shapeAbove.AddPoint(a);
      shapeBelow.AddPoint(b);

      if (EdgeP1.PlaneRelationship == PointPlaneRelationship.Above)
      {
         var newForBelow = new Edge2(EdgeP2, b);
         EdgeP2 = a;

         shapeAbove.Edges.Add(this);
         shapeBelow.Edges.Add(newForBelow);
      }
      else
      {
         var newForAbove = new Edge2(EdgeP2, a);
         EdgeP2 = b;

         shapeAbove.Edges.Add(newForAbove);
         shapeBelow.Edges.Add(this);
      }
   }

   private void SplitAtEnd(NewPointsGetter newPoints, Shape2 shapeAbove, Shape2 shapeBelow)
   {
      if (EdgeP1.PlaneRelationship == PointPlaneRelationship.Inside)
      {
         if (EdgeP2.PlaneRelationship == PointPlaneRelationship.Above)
         {
            EdgeP1 = newPoints.GetPointAbove(EdgeP1);
            shapeAbove.Edges.Add(this);
         }
         else
         {
            EdgeP1 = newPoints.GetPointBelow(EdgeP1);
            shapeBelow.Edges.Add(this);
         }
      }
      else if (EdgeP2.PlaneRelationship == PointPlaneRelationship.Inside)
      {
         if (EdgeP1.PlaneRelationship == PointPlaneRelationship.Above)
         {
            EdgeP2 = newPoints.GetPointAbove(EdgeP2);
            shapeAbove.Edges.Add(this);
         }
         else
         {
            EdgeP2 = newPoints.GetPointBelow(EdgeP2);
            shapeBelow.Edges.Add(this);
         }
      }
      else
      {
         throw new System.Exception();
      }
   }

   public void Split(Vector3 P0, Vector3 n, NewPointsGetter newPoints, Shape2 shapeAbove, Shape2 shapeBelow)
   {
      if (Point2.PointsBridgePlane(EdgeP1, EdgeP2))
      {
         Vector3 x;
         Utils.LinePlaneIntersect(n, P0, EdgeP1.Point, EdgeP2.Point, out x);

         SplitInHalf(x, newPoints, shapeAbove, shapeBelow);
      }
      else if (Point2.BothAbove(EdgeP1, EdgeP2))
      {
         shapeAbove.Edges.Add(this);
      }
      else if (Point2.BothBelow(EdgeP1, EdgeP2))
      {
         shapeBelow.Edges.Add(this);
      }
      else if (Point2.BothInside(EdgeP1, EdgeP2))
      {
         // do nothing - new edges for each shape will be created by the face split
      }
      else 
      {
         SplitAtEnd(newPoints, shapeAbove, shapeBelow);
      }
   }
}
