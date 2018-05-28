using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridObjectComponent : RectObjectComponent
{
   public GridObject GridObj { get; private set; }

   protected override void OnAwake()
   {
      base.OnAwake();
      GridObj = GetComponent<GridObject>();
   }

   public void SlotIntoSquare(GridSquare square, float p)
   {
      GridObj.Init(square, square);

      Vector2 diagonal = square.Rect.LowerRight - square.Rect.UpperLeft;
      Rect.Init(square.Rect.UpperLeft + p * diagonal, square.Rect.LowerRight - p * diagonal);
   }
}
