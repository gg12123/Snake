using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Letter : GridObjectComponent, IOnCollidedWithSnakeHead
{
   private WordController m_Controller;

   protected override void OnAwake()
   {
      base.OnAwake();
      m_Controller = GetComponentInParent<WordController>();
   }

   public void OnCollidedWithSnakeHead(SnakeSegment head)
   {
      GridObj.OnRemovedFromGrid();
      m_Controller.OnLetterCollected(this);
   }
}
