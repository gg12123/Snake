using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collision
{
   private const float e = 0.8f;

   private Vector3 m_CollisionNormalWorld;
   private Vector3 m_CollisionNormalBody1Local;
   private Vector3 m_CollisionNormalBody2Local;

   private Vector3 m_CollisionPointBody1Local;
   private Vector3 m_CollisionPointBody2Local;

   private float m_J;

   private MyRigidbody m_Body1;
   private MyRigidbody m_Body2;

   public Vector3 GetImpulseWorld(MyRigidbody body)
   {
      return body == m_Body2 ? m_J * m_CollisionNormalWorld : -m_J * m_CollisionNormalWorld;
   }

   public Vector3 GetImpulseLocal(MyRigidbody body)
   {
      return body == m_Body2 ? m_J * m_CollisionNormalBody2Local : -m_J * m_CollisionNormalBody1Local;
   }

   public Vector3 GetCollisionPointLocal(MyRigidbody body)
   {
      return body == m_Body2 ? m_CollisionPointBody2Local : m_CollisionPointBody1Local;
   }

   private float CalculateS(Vector3 n, Vector3 r, MyRigidbody body)
   {
      var x = (Vector3)(body.InertiaInverse * Vector3.Cross(r, n));
      return Vector3.Dot(n, Vector3.Cross(x, r));
   }

   public void Calculate(MyRigidbody body1, MyRigidbody body2, Vector3 collNormalWorld1To2, Vector3 collPointWorld)
   {
      var v1 = body1.VelocityWorldAtPoint(collPointWorld);
      var v2 = body2.VelocityWorldAtPoint(collPointWorld);

      var vr = v2 - v1;

      if (Vector3.Dot(vr, collNormalWorld1To2) < 0.0f)
      {
         var r1 = body1.transform.InverseTransformPoint(collPointWorld);
         var r2 = body2.transform.InverseTransformPoint(collPointWorld);

         m_CollisionNormalWorld = collNormalWorld1To2;
         m_CollisionNormalBody1Local = body1.transform.InverseTransformDirection(collNormalWorld1To2);
         m_CollisionNormalBody2Local = body2.transform.InverseTransformDirection(collNormalWorld1To2);

         var s1 = CalculateS(m_CollisionNormalBody1Local, r1, body1);
         var s2 = CalculateS(m_CollisionNormalBody2Local, r2, body2);

         var m1 = body1.Mass;
         var m2 = body2.Mass;

         m_J = (-Vector3.Dot(vr, m_CollisionNormalWorld) * (e + 1.0f)) / (1.0f / m1 + 1.0f / m2 + s1 + s2);

         m_Body1 = body1;
         m_Body2 = body2;
         m_CollisionPointBody1Local = r1;
         m_CollisionPointBody2Local = r2;

         m_Body1.SetCollision(this);
         m_Body2.SetCollision(this);
      }
   }
}
