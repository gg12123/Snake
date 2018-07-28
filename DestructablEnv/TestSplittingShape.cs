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

         var pool = GetComponent<RigidBodyPool>();

         var above = pool.GetBody().GetComponent<Shape>();
         var below = pool.GetBody().GetComponent<Shape>();

         var e = shape.EdgePairs[Random.Range(0, shape.EdgePairs.Count)].Edge1;

         var collPoint = e.Start.Point;
         var collNormal = e.OwnerFace.Normal;

         collPoint = shape.transform.TransformPoint(collPoint);

         shape.Split(collPoint, collNormal, above, below);

         pool.Return(shape.GetComponent<MyRigidbody>());
      }
   }
}
