using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : GridObjectComponent, IOnCollidedWithSnakeHead
{
   public void OnCollidedWithSnakeHead(SnakeSegment head)
   {
      head.OwnerSnake.Die();
   }
}
