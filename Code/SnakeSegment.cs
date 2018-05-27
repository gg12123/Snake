using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeSegment : RectGameObjComponent
{
   public GridObject GridObj { get; private set; }
   public Direction Direction { get; private set; }

   protected override void OnAwake()
   {
      GridObj = GetComponent<GridObject>();
   }

   public void Init(Direction dir)
   {
      Direction = dir;
   }

   public void Expand(float amount)
   {
      Rect.Expand(Direction, amount);
      GridObj.OnExpanded(Direction);
   }

   public bool Shrink(float amount, out float amountShrunk)
   {
      var size = Rect.Size(Direction);

      if (size >= amount)
      {
         Rect.Shrink(Direction, amount);
         GridObj.OnShrunk(Direction);

         amountShrunk = amount;
         return true;
      }

      amountShrunk = size;
      GridObj.OnRemovedFromGrid();
      return false;
   }
}
