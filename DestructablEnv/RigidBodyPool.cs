using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PoolForBodies : Pool<MyRigidbody>
{
}

public class RigidBodyPool : MonoBehaviour
{
   [SerializeField]
   private PoolForBodies m_Pool;

   public MyRigidbody GetBody()
   {
      m_Pool.Init(transform);
      return m_Pool.GetObject();
   }

   public void Return(MyRigidbody body)
   {
      m_Pool.Init(transform);
      m_Pool.ReturnObject(body);
   }
}
