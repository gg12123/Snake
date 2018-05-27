using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSquareSquareEnumerator : IMyEnumerator<GridSquare>
{
   private int m_CurrRight;
   private int m_CurrDown;

   private GridSquare m_UL;
   private GridSquare m_LR;

   public void Init(GridSquare uL, GridSquare lR)
   {
      m_UL = uL;
      m_LR = lR;
   }

   public GridSquare First()
   {
      return m_UL;
   }

   public GridSquare Next()
   {
      m_CurrRight++;

      if (m_UL.XIndex + m_CurrRight > m_LR.XIndex)
      {
         m_CurrRight = 0;
         m_CurrDown++;
      }

      if (m_UL.YIndex - m_CurrDown < m_LR.YIndex)
         return null;

      return m_UL.Next(Direction.Right, m_CurrRight).Next(Direction.Down, m_CurrDown);
   }

   public void Reset()
   {
      m_CurrRight = m_CurrDown = 0;
   }
}
