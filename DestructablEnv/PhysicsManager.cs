using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsManager : MonoBehaviour
{
   private MyRigidbody[] m_Bodies;

   // Use this for initialization
   void Start ()
   {
      m_Bodies = GetComponentsInChildren<MyRigidbody>();

      foreach (var b in m_Bodies)
         b.Init();
   }

   private void DetectCollisions()
   {
      Vector3 collPoint;
      for (int i = 0; i < m_Bodies.Length; i++)
      {
         for (int j = i + 1; j < m_Bodies.Length; j++)
         {
            var b1 = m_Bodies[i];
            var b2 = m_Bodies[j];

            if (b1.Shape.IsCollidedWithOther(b2.Shape, out collPoint))
            {
               var collision = new Collision();
               collision.Calculate(b1, b2, (b2.transform.position - b1.transform.position).normalized, collPoint);
            }
         }
      }
   }

   private void UpdateBodies()
   {
      for (int i = 0; i < m_Bodies.Length; i++)
         m_Bodies[i].UpdateSimulation();
   }
   
   // Update is called once per frame
   void FixedUpdate ()
   {
      DetectCollisions();
      UpdateBodies();
   }
}
