using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSplittingShape : MonoBehaviour
{

   private Vector3 RndVec { get { return new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)); } }

   private void Update()
   {
      if (Input.GetKeyDown(KeyCode.Space))
      {
         var shape = GetComponentInChildren<Shape>();
        // shape.Split(RndVec, RndVec.normalized);
      }
   }
}
