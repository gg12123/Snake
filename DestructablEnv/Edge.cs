using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge
{
   public Face OwnerFace { get; set; }
   public EdgePair OwnerPair { get; private set; }

   public ShapePoint Start { get; set; }
   public ShapePoint End { get; set; }

   public Edge Next { get; private set; }
   public Edge Prev { get; private set; }

   public Edge(EdgePair pair, Face ownerFace)
   {
      OwnerPair = pair;
      OwnerFace = ownerFace;
   }

   public void InsertAfter(Edge newEdge)
   {

   }

   public void InsertBefore(Edge newEdge)
   {

   }

   public void InsertAfterAndBreak(Edge newEdge)
   {

   }

   public void InsertBeforeAndBreak(Edge newEdge)
   {

   }
}
