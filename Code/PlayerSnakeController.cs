using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSnakeController : MonoBehaviour, ISnakeController
{
   private void Update()
   {
      if (Input.GetKeyDown(KeyCode.Space))
      {
         GetComponent<Snake>().enabled = true;
         enabled = false;
      }
   }

   public bool ChangeDirection(out Direction dir)
   {
      if (Input.GetKeyDown(KeyCode.UpArrow))
      {
         dir = Direction.Up;
         return true;
      }

      if (Input.GetKeyDown(KeyCode.DownArrow))
      {
         dir = Direction.Down;
         return true;
      }

      if (Input.GetKeyDown(KeyCode.RightArrow))
      {
         dir = Direction.Right;
         return true;
      }

      if (Input.GetKeyDown(KeyCode.LeftArrow))
      {
         dir = Direction.Left;
         return true;
      }

      dir = Direction.Up;
      return false;
   }
}
