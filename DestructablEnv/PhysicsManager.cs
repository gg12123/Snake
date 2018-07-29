using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PhysicsManager : MonoBehaviour
{
   private List<MyRigidbody> m_Bodies;

   private List<Vector3> m_CollPoints = new List<Vector3>();
   private List<Vector3> m_CollNormals = new List<Vector3>();

   private List<MyRigidbody> m_ToRemove = new List<MyRigidbody>();
   private List<MyRigidbody> m_ToAdd = new List<MyRigidbody>();

   private RigidBodyPool m_Pool;

   // Use this for initialization
   void Start ()
   {
      m_Pool = GetComponent<RigidBodyPool>();
      m_Bodies = GetComponentsInChildren<MyRigidbody>().ToList();

      foreach (var b in m_Bodies)
         b.Init();
   }

   private void CalculateFinalCollisionParams(Vector3 refNormal, out Vector3 normal, out Vector3 point)
   {
      point = Vector3.zero;
      normal = Vector3.zero;

      var bestComp = Mathf.NegativeInfinity;

      for (int i = 0; i < m_CollNormals.Count; i++)
      {
         point += m_CollPoints[i];

         var n = m_CollNormals[i];
         var comp = Mathf.Abs(Vector3.Dot(refNormal, n));

         if (comp > bestComp)
         {
            normal = n;
            bestComp = comp;
         }
      }

      point /= m_CollNormals.Count;

      if (Vector3.Dot(normal, refNormal) < 0.0f)
         normal *= -1.0f;
   }

   private void DetectCollisions()
   {
      var collPoint = Vector3.zero;
      var collNormal = Vector3.zero;

      for (int i = 0; i < m_Bodies.Count; i++)
      {
         for (int j = i + 1; j < m_Bodies.Count; j++)
         {
            var b1 = m_Bodies[i];
            var b2 = m_Bodies[j];

            m_CollNormals.Clear();
            m_CollPoints.Clear();

           // b1.Shape.FindCollisions(b2.Shape, m_CollPoints, m_CollNormals);

            if (m_CollNormals.Count > 0)
            {
               var collision = new Collision();
               var toB2 = (b2.transform.position - b1.transform.position).normalized;

               CalculateFinalCollisionParams(toB2, out collNormal, out collPoint);
               collision.Calculate(b1, b2, collNormal, collPoint);
            }
         }
      }
   }

   public void DoSplit(MyRigidbody toSplit, Impulse impulse)
   {
      Debug.Log("Splitting");

      var above = m_Pool.GetBody();
      var below = m_Pool.GetBody();

      toSplit.Shape.Split(impulse.WorldCollisionPoint, impulse.WorldImpulse.normalized, above.Shape, below.Shape);

      above.Init(toSplit);
      below.Init(toSplit);

      above.SetImpulse(impulse);
      below.SetImpulse(impulse);

      m_ToRemove.Add(toSplit);
      m_ToAdd.Add(above);
      m_ToAdd.Add(below);

      m_Pool.Return(toSplit);
   }

   private void UpdateBodies()
   {
      for (int i = 0; i < m_Bodies.Count; i++)
         m_Bodies[i].UpdateSimulation();
   }

   private void AddAndRemoveBodies()
   {
      foreach (var b in m_ToRemove)
         m_Bodies.Remove(b);

      foreach (var b in m_ToAdd)
         m_Bodies.Add(b);

      m_ToRemove.Clear();
      m_ToAdd.Clear();
   }
   
   // Update is called once per frame
   void FixedUpdate ()
   {
      DetectCollisions();
      UpdateBodies();
      AddAndRemoveBodies();
   }
}
