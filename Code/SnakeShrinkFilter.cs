using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeShrinkFilter
{
   private float m_FilterAmount;

   public SnakeShrinkFilter()
   {
      m_FilterAmount = 0.0f;
   }

   public void Add(float amount)
   {
      m_FilterAmount += amount;
   }

   public float Filter(float input)
   {
      float output = Mathf.Max(input - m_FilterAmount, 0.0f);
      m_FilterAmount = Mathf.Max(m_FilterAmount - input, 0.0f);
      return output;
   }
}
