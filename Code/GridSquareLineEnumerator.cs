using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMyEnumerator<T>
{
   T First();
   T Next();
   void Reset();
}

public class GridSquareLineEnumerator : IMyEnumerator<GridSquare>
{
   private GridSquare m_Start;
   private Direction m_Dir;
   private int m_Count;
   private int m_Current;

   public void Init(GridSquare start, Direction dir, int count)
   {
      m_Count = count;
      m_Dir = dir;
      m_Start = start;
      Reset();
   }

   public GridSquare First()
   {
      return m_Start;
   }

   public GridSquare Next()
   {
      m_Current++;

      if (m_Current < m_Count)
         return m_Start.Next(m_Dir, m_Current);

      return null;
   }

   public void Reset()
   {
      m_Current = 0;
   }
}
