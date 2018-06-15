using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge
{
   public Face OwnerFace { get; set; }
   public EdgePair OwnerPair { get; private set; }

   public ShapePoint Start { get { return OwnerPair.GetStart(this); } set { OwnerPair.SetStart(this, value); } }
   public ShapePoint End { get { return OwnerPair.GetEnd(this); } set { OwnerPair.SetEnd(this, value); } }

   public Edge Next { get; private set; }
   public Edge Prev { get; private set; }

   public Edge Other { get { return OwnerPair.Other(this); } }

   public Edge(EdgePair pair, Face ownerFace)
   {
      OwnerPair = pair;
      OwnerFace = ownerFace;
   }

   public void InsertAfter(Edge toInsert)
   {
      var oldNext = Next;

      Next = toInsert;
      toInsert.Prev = this;

      if (oldNext != null)
      {
         toInsert.Next = oldNext;
         oldNext.Prev = toInsert;
      }
   }

   public void InsertBefore(Edge toInsert)
   {
      var oldPrev = Prev;

      Prev = toInsert;
      toInsert.Next = this;

      if (oldPrev != null)
      {
         toInsert.Prev = oldPrev;
         oldPrev.Next = toInsert;
      }
   }

   public void InsertAfterAndBreak(Edge toInsert)
   {
      Next = toInsert;
      toInsert.Prev = this;
   }

   public void InsertBeforeAndBreak(Edge toInsert)
   {
      Prev = toInsert;
      toInsert.Next = this;
   }
}
