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

public class EdgeAboutPointWhenSplittingEnumerator : IEdgeEnumerator
{
   private Edge m_First;
   private Edge m_Curr;
   private Edge m_Second;
   private bool m_ReachedSecond;

   public void Init(Edge edgeThatBridges)
   {
      m_First = edgeThatBridges;
      m_Curr = m_First;
      m_Second = GetNext(m_First);
      m_ReachedSecond = false;
   }

   public Edge First()
   {
      return m_First;
   }

   public Edge Next()
   {
      m_Curr = GetNext(m_Curr);

      var toRet = m_Curr;

      if (m_Curr == m_Second)
      {
         if (!m_ReachedSecond)
         {
            m_ReachedSecond = true;
         }
         else
         {
            toRet = null;
         }
      }
      return toRet;
   }

   private Edge GetNext(Edge e)
   {
      return e.Next.Other;
   }
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
      m_Curr = m_Curr.Next;
      return (m_Curr == m_First ? null : m_Curr);
   }
}
