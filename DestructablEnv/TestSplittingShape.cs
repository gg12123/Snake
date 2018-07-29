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
         var shape = GetComponentInChildren<Shape2>();

         var pool = GetComponent<RigidBodyPool>();

         var above = pool.GetBody().GetComponent<Shape2>();
         var below = pool.GetBody().GetComponent<Shape2>();

         var f = shape.Faces[Random.Range(0, shape.Faces.Count)];

         var collNormal = shape.transform.TransformDirection(f.Normal);
         var collPoint = shape.transform.TransformPoint(f.RandomEdgePoint());

         shape.Split(collPoint, collNormal, above, below);

         pool.Return(shape.GetComponent<MyRigidbody>());
      }
   }
}
