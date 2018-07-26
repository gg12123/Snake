using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Impulse
{
   public Vector3 WorldImpulse { get; private set; }
   public Vector3 LocalImpulse { get; private set; }
   public Vector3 WorldCollisionPoint { get; private set; }
   public Vector3 LocalCollisionPoint { get; private set; }
   public float Impact { get; private set; }

   public void Set(Vector3 worldImpulse, Vector3 localImpulse, Vector3 collPointWorld, Vector3 collPointLocal, float impact)
   {
      WorldImpulse = worldImpulse;
      LocalImpulse = localImpulse;
      WorldCollisionPoint = collPointWorld;
      LocalCollisionPoint = collPointLocal;
      Impact = impact;
   }
}

public class Collision
{
   private const float e = 0.8f;

   public Impulse Body1Impulse { get; private set; }
   public Impulse Body2Impulse { get; private set; }

   public Collision()
   {
      Body1Impulse = new Impulse();
      Body2Impulse = new Impulse();
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

      var signedImpact = Vector3.Dot(vr, collNormalWorld1To2);

      if (signedImpact < 0.0f)
      {
         var r1 = body1.transform.InverseTransformPoint(collPointWorld);
         var r2 = body2.transform.InverseTransformPoint(collPointWorld);

         var collisionNormalBody1Local = body1.transform.InverseTransformDirection(collNormalWorld1To2);
         var collisionNormalBody2Local = body2.transform.InverseTransformDirection(collNormalWorld1To2);

         var s1 = CalculateS(collisionNormalBody1Local, r1, body1);
         var s2 = CalculateS(collisionNormalBody2Local, r2, body2);

         var m1 = body1.Mass;
         var m2 = body2.Mass;

         var J = (-signedImpact * (e + 1.0f)) / (1.0f / m1 + 1.0f / m2 + s1 + s2);

         var impact = -signedImpact;

         Body1Impulse.Set(-J * collNormalWorld1To2, -J * collisionNormalBody1Local, collPointWorld, r1, impact);
         Body2Impulse.Set(J * collNormalWorld1To2, J * collisionNormalBody2Local, collPointWorld, r2, impact);

         body1.SetImpulse(Body1Impulse);
         body2.SetImpulse(Body2Impulse);
      }
   }
}
