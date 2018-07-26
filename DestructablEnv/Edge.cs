using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge
{
   private ShapePoint m_Start;
   private ShapePoint m_End;

   public Face OwnerFace { get; set; }
   public EdgePair OwnerPair { get; private set; }

   public Edge Next { get; private set; }
   public Edge Prev { get; private set; }

   public Edge Other { get; private set; }

   public ShapePoint Start
   {
      get
      {
         return m_Start;
      }
      set
      {
         m_Start = value;
         Other.OnOthersStartSet(value);
         OwnerPair.RefreshPoints();
      }
   }

   public ShapePoint End
   {
      get
      {
         return m_End;
      }
      set
      {
         m_End = value;
         Other.OnOthersEndSet(value);
         OwnerPair.RefreshPoints();
      }
   }

   public Edge(EdgePair pair, Face ownerFace)
   {
      OwnerPair = pair;
      OwnerFace = ownerFace;
   }

   public void OnOthersStartSet(ShapePoint othersStart)
   {
      m_End = othersStart;
   }

   public void OnOthersEndSet(ShapePoint othersEnd)
   {
      m_Start = othersEnd;
   }

   public void InitOther(Edge other)
   {
      Other = other;
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

   public void Clear()
   {
      Next = null;
      Prev = null;
      OwnerFace = null;
   }
}
