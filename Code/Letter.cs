using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Letter : GridObjectComponent, IOnCollidedWithSnakeHead
{
   private WordGame m_Controller;
   private TextMesh m_Text;

   public char Char { get; private set; }

   protected override void OnAwake()
   {
      base.OnAwake();
      m_Controller = GetComponentInParent<WordGame>();
      m_Text = GetComponentInChildren<TextMesh>();
      GetComponentInChildren<MeshRenderer>().sortingLayerName = "Text";
   }

   public void OnCollidedWithSnakeHead(SnakeSegment head)
   {
      GridObj.OnRemovedFromGrid();
      m_Controller.OnLetterCollected(this, head.OwnerSnake);
   }

   public void Init(char letter, GridSquare square)
   {
      Char = letter;
      m_Text.text = char.ToUpper(letter).ToString();
      SlotIntoSquare(square, 0.2f);
   }
}
