using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Matrix3
{
   private Vector3 m_Col1;
   private Vector3 m_Col2;
   private Vector3 m_Col3;

   public float this[int row, int col]
   {
      get
      {
         switch (col)
         {
            case 0:
               return m_Col1[row];

            case 1:
               return m_Col2[row];

            case 2:
               return m_Col3[row];
         }
         return 0.0f;
      }
      set
      {
         switch (col)
         {
            case 0:
               m_Col1[row] = value;
               break;

            case 1:
               m_Col2[row] = value;
               break;

            case 2:
               m_Col3[row] = value;
               break;
         }
      }
   }

   public static Vector3 operator *(Matrix3 lhs, Vector3 rhs)
   {
      Vector3 rtn = Vector3.zero;

      for (int i = 0; i < 3; i++)
      {
         for (int j = 0; j < 3; j++)
         {
            rtn[i] += (lhs[i, j] * rhs[j]);
         }
      }
      return rtn;
   }

   public static Matrix3 operator *(Matrix3 lhs, Matrix3 rhs)
   {
      Matrix3 rtn = new Matrix3();

      for (int row = 0; row < 3; row++)
      {
         for (int col = 0; col < 3; col++)
         {
            for (int i = 0; i < 3; i++)
            {
               rtn[row, col] += (lhs[row, i] * rhs[i, col]);
            }
         }
      }
      return rtn;
   }

   public Matrix3 Inverse()
   {
      var e11 = this[0, 0];
      var e12 = this[0, 1];
      var e13 = this[0, 2];

      var e21 = this[1, 0];
      var e22 = this[1, 1];
      var e23 = this[1, 2];

      var e31 = this[2, 0];
      var e32 = this[2, 1];
      var e33 = this[2, 2];

      var d = e11 * e22 * e33 -
      e11 * e32 * e23 +
      e21 * e32 * e13 -
      e21 * e12 * e33 +
      e31 * e12 * e23 -
      e31 * e22 * e13;

      Matrix3 inv = new Matrix3();

      inv[0, 0] = (e22 * e33 - e23 * e32) / d;
      inv[0, 1] = -(e12 * e33 - e13 * e32) / d;
      inv[0, 2] = (e12 * e23 - e13 * e22) / d;

      inv[1, 0] = -(e21 * e33 - e23 * e31) / d;
      inv[1, 1] = (e11 * e33 - e13 * e31) / d;
      inv[1, 2] = -(e11 * e23 - e13 * e21) / d;

      inv[2, 0] = (e21 * e32 - e22 * e31) / d;
      inv[2, 1] = -(e11 * e32 - e12 * e31) / d;
      inv[2, 2] = (e11 * e22 - e12 * e21) / d;

      return inv;
   }
}
