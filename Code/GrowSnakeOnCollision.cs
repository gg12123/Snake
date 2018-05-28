using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrowSnakeOnCollision : MonoBehaviour, IOnCollidedWithSnakeHead
{
   [SerializeField]
   private float m_GrowAmount = 0.5f;

   public void OnCollidedWithSnakeHead(SnakeSegment head)
   {
      head.OwnerSnake.Grow(m_GrowAmount);
   }
}
