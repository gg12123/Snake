﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyRigidbody : MonoBehaviour
{
   [SerializeField]
   private float m_Drag = 0.5f;
   [SerializeField]
   private float m_AngularDrag = 0.5f;
   [SerializeField]
   private float m_Mass = 1.0f;

   private const float g = 9.8f;

   private Matrix3 m_Inertia;
   private Matrix3 m_InertiaInv;

   public Shape2 Shape { get; private set; }

   public Vector3 VelocityWorld { get; private set; }
   public Vector3 VelocityLocal { get { return transform.InverseTransformDirection(VelocityWorld); } }
   public Vector3 AngularVelocityWorld { get { return transform.TransformDirection(AngularVelocityLocal); } }
   public Vector3 AngularVelocityLocal { get; private set; }
   public float Mass { get { return m_Mass; } }
   public Matrix3 Inertia { get { return m_Inertia; } }
   public Matrix3 InertiaInverse { get { return m_InertiaInv; } }
   public float Drag { get { return m_Drag; } set { m_Drag = value; } }

   private Impulse m_Impulse;

   private PhysicsManager m_Physics;

   private void Awake()
   {
      Shape = GetComponent<Shape2>();
      m_Physics = GetComponentInParent<PhysicsManager>();
   }

   public void Init(MyRigidbody body)
   {
      VelocityWorld = body.VelocityWorld;
      AngularVelocityLocal = body.AngularVelocityLocal;
      Init();
   }

   public void Init()
   {
      // use the bounding box for mass and inertia

      var Ixx = 0.0f;
      var Iyy = 0.0f;
      var Izz = 0.0f;

      var Ixy = 0.0f;
      var Ixz = 0.0f;
      var Iyz = 0.0f;

      for (int i = 0; i < Shape.Points.Count; i++)
      {
         var P = Shape.Points[i].Point;

         Ixx += (P.y * P.y + P.z * P.z);
         Iyy += (P.x * P.x + P.z * P.z);
         Izz += (P.x * P.x + P.y * P.y);

         Ixy += (P.x * P.y);
         Ixz += (P.x * P.z);
         Iyz += (P.y * P.z);
      }

      m_Inertia = new Matrix3();

      m_Inertia[0, 0] = Ixx;
      m_Inertia[0, 1] = -Ixy;
      m_Inertia[0, 3] = -Ixz;

      m_Inertia[1, 0] = -Ixy;
      m_Inertia[1, 1] = Iyy;
      m_Inertia[1, 2] = -Iyz;

      m_Inertia[2, 0] = -Ixz;
      m_Inertia[2, 1] = -Iyz;
      m_Inertia[2, 2] = Izz;

      m_InertiaInv = m_Inertia.Inverse();
   }

   public void SetImpulse(Impulse i)
   {
      m_Impulse = i;
   }

   private void CalculateForces(out Vector3 forcesWorld, out Vector3 momentsLocal)
   {
      forcesWorld = Vector3.zero;
      forcesWorld -= Mass * g * Vector3.up;
      forcesWorld -= m_Drag * VelocityWorld;

      momentsLocal = Vector3.zero;
      momentsLocal -= m_AngularDrag * AngularVelocityLocal;
   }

   private Quaternion AngularVelAsQuat()
   {
      return new Quaternion(AngularVelocityLocal.x, AngularVelocityLocal.y, AngularVelocityLocal.z, 0.0f);
   }

   private Quaternion QMul(Quaternion q, float m)
   {
      return new Quaternion(q.x * m, q.y * m, q.z * m, q.w * m);
   }

   private Quaternion QAdd(Quaternion q1, Quaternion q2)
   {
      return new Quaternion(q1.x + q2.x, q1.y + q2.y, q1.z + q2.z, q1.w + q2.w);
   }

   private void Integrate(Vector3 forcesWorld, Vector3 momentsLocal)
   {
      VelocityWorld += (forcesWorld / Mass) * Time.deltaTime;

      AngularVelocityLocal += (InertiaInverse * momentsLocal) * Time.deltaTime;
   }

   private void ApplyImpulse()
   {
      VelocityWorld += m_Impulse.WorldImpulse / Mass;

      var r = transform.InverseTransformPoint(m_Impulse.WorldCollisionPoint);
      var J = m_Impulse.LocalImpulse;
      AngularVelocityLocal += (InertiaInverse * Vector3.Cross(r, J));

      UpdateTransform();

      if (m_Impulse.Impact > Mass)
         m_Physics.DoSplit(this, m_Impulse);
   }

   private void UpdateTransform()
   {
      transform.position += VelocityWorld * Time.deltaTime;

      var q = transform.rotation;
      transform.rotation = QAdd(q, QMul(q * AngularVelAsQuat(), 0.5f * Time.deltaTime));
   }

   private void DoNormalMotion()
   {
      Vector3 f, m;
      CalculateForces(out f, out m);
      Integrate(f, m);
      UpdateTransform();
   }

   public void UpdateSimulation()
   {
      if (m_Impulse == null)
      {
         DoNormalMotion();
      }
      else
      {
         ApplyImpulse();
         m_Impulse = null;
      }
   }

   public Vector3 VelocityWorldAtPoint(Vector3 pointWorld)
   {
      return VelocityWorld + Vector3.Cross(AngularVelocityWorld, pointWorld - transform.position);
   }
}
