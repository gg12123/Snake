using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PoolForShapes : Pool<Shape>
{
}

public class ShapePool : MonoBehaviour
{
   [SerializeField]
   private PoolForShapes m_Pool;

   public Shape GetShape()
   {
      m_Pool.Init(transform);
      return m_Pool.GetObject();
   }

   public void Return(Shape shape)
   {
      m_Pool.Init(transform);
      m_Pool.ReturnObject(shape);
   }
}
