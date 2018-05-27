using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeSegment : RectGameObjComponent
{
   private Snake m_Owner;
   private bool m_ReachedSquare;

   public GridObject GridObj { get; private set; }
   public Direction Direction { get; private set; }

   protected override void OnAwake()
   {
      GridObj = GetComponent<GridObject>();
      m_Owner = GetComponentInParent<Snake>();
   }

   public void Init(Direction dir)
   {
      Direction = dir;
      m_ReachedSquare = false;
   }

   public void Expand(float amount)
   {
      Rect.Expand(Direction, amount);
      GridObj.OnExpanded(Direction);
   }

   public bool Shrink(Direction nextDirection, float amount, out float amountShrunk)
   {
      if (m_ReachedSquare)
      {
         return ShrinkAfterReachingSqaure(nextDirection, amount, out amountShrunk);
      }
      else
      {
         ShrinkBeforeReachingSquare(nextDirection, amount);
         amountShrunk = amount;
         return true;
      }
   }

   public bool ShrinkAfterReachingSqaure(Direction nextDirection, float amount, out float amountShrunk)
   {
      var size = Rect.Size(nextDirection);

      if (size >= amount)
      {
         DoShrink(nextDirection, amount);
         amountShrunk = amount;
         return true;
      }
      else
      {
         amountShrunk = size;
         GridObj.OnRemovedFromGrid();
         return false;
      }
   }

   private void ShrinkBeforeReachingSquare(Direction nextDirection, float amount)
   {
      var size = Rect.Size(Direction);

      if (size - amount > m_Owner.Width)
      {
         DoShrink(Direction, amount);
      }
      else
      {
         float toReachSquare = size - m_Owner.Width;
         float amountLeft = amount - toReachSquare;

         DoShrink(Direction, toReachSquare);
         DoShrink(nextDirection, amountLeft);

         m_ReachedSquare = true;
      }
   }

   private void DoShrink(Direction dir, float amount)
   {
      Rect.Shrink(dir, amount);
      GridObj.OnShrunk(dir);
   }
}
