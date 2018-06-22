using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEdgeEnumerator
{
   void Init(Edge e);
   Edge First();
   Edge Next();
}

public class EdgeLoopEnumerator : IEdgeEnumerator
{
   private Edge m_First;
   private Edge m_Curr;

   public Edge First()
   {
      return m_First;
   }

   public void Init(Edge e)
   {
      m_First = e;
      m_Curr = e;
   }

   public Edge Next()
   {
      m_Curr = m_Curr.Prev;
      return (m_Curr == m_First ? null : m_Curr);
   }
}
