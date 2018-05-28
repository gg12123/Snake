using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetector : MonoBehaviour, IOnSnakeStartsMoving
{
   private Snake m_Snake;

   // Use this for initialization
   void Awake ()
   {
      m_Snake = GetComponent<Snake>();
      enabled = false;
   }
   
   // Update is called once per frame
   void Update ()
   {
      var head = m_Snake.Head;

      var testSquares = head.GridObj.GetFront(head.Direction);

      for (var square = testSquares.First(); square != null; square = testSquares.Next())
      {
         for (int i = 0; i < square.ObjectCount; i++)
         {
            var obj = square.ObjectAt(i);
            if ((obj != head.GridObj) && obj.Rect.IsOverlappingWith(head.Rect, 0.01f * m_Snake.Width))
            {
               obj.OnCollidedWithSnakeHead(head);
            }
         }
      }
   }

   public void OnSnakeStartsMoving()
   {
      enabled = true;
   }
}
