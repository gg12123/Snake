using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSplittingShape : MonoBehaviour
{
   private void Update()
   {
      if (Input.GetKeyDown(KeyCode.Space))
      {
         var shape = GetComponentInChildren<Shape>();
         shape.Split(Vector3.zero, Vector3.up);
      }
   }
}
