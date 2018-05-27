using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rect : MonoBehaviour
{
   private Vector2 m_UL;
   private Vector2 m_LR;

   public Vector2 UpperRight { get { return (m_UL + Size(Direction.Right) * VectorDirection(Direction.Right)); } }
   public Vector2 LowerLeft { get { return (m_LR + Size(Direction.Right) * VectorDirection(Direction.Left)); } }
   public Vector2 UpperLeft { get { return m_UL; } }
   public Vector2 LowerRight { get { return m_LR; } }

   public void Init(Vector2 uL, Vector2 lR)
   {
      m_LR = lR;
      m_UL = uL;
      OnNewRect();
   }

   private void OnNewRect()
   {
      transform.position = (m_LR + m_UL) / 2.0f;
      transform.localScale = new Vector3(Size(Direction.Right), Size(Direction.Up), 1.0f);
   }

   public void Expand(Direction dir, float amount)
   {
      switch (dir)
      {
         case Direction.Up:
            m_UL = new Vector2(m_UL.x, m_UL.y + amount);
            break;
         case Direction.Down:
            m_LR = new Vector2(m_LR.x, m_LR.y - amount);
            break;
         case Direction.Right:
            m_LR = new Vector2(m_LR.x + amount, m_LR.y);
            break;
         case Direction.Left:
            m_UL = new Vector2(m_UL.x - amount, m_UL.y);
            break;
      }

      OnNewRect();
   }

   public void Shrink(Direction dir, float amount)
   {
      switch (dir)
      {
         case Direction.Up:
            m_LR = new Vector2(m_LR.x, m_LR.y + amount);
            break;
         case Direction.Down:
            m_UL = new Vector2(m_UL.x, m_UL.y - amount);
            break;
         case Direction.Right:
            m_UL = new Vector2(m_UL.x + amount, m_UL.y);
            break;
         case Direction.Left:
            m_LR = new Vector2(m_LR.x - amount, m_LR.y);
            break;
      }

      OnNewRect();
   }

   public bool IsOverlappingWith(Rect other, float tol)
   {
      return OverlappingInX(other, tol) && OverlappingInY(other, tol);
   }

   public bool IsOverlappingWith(Rect other)
   {
      return IsOverlappingWith(other, 0.0f);
   }

   public bool IsOverlappingWith(Rect other, Direction testDirection)
   {
      switch (testDirection)
      {
         case Direction.Up:
         case Direction.Down:
            return OverlappingInY(other, 0.0f);
         case Direction.Right:
         case Direction.Left:
            return OverlappingInX(other, 0.0f);
         default:
            throw new System.Exception();
      }
   }

   public float Size(Direction dir)
   {
      switch (dir)
      {
         case Direction.Up:
         case Direction.Down:
            return m_UL.y - m_LR.y;
         case Direction.Right:
         case Direction.Left:
            return m_LR.x - m_UL.x;
         default:
            throw new System.Exception();
      }
   }

   public static Vector2 VectorDirection(Direction dir)
   {
      switch (dir)
      {
         case Direction.Up:
            return Vector2.up;
         case Direction.Down:
            return -Vector2.up;
         case Direction.Right:
            return Vector2.right;
         case Direction.Left:
            return -Vector2.right;
         default:
            throw new System.Exception();
      }
   }

   private bool OverlappingInX(Rect other, float tol)
   {
      return ValuesOverlap(tol, m_LR.x, m_UL.x, other.LowerRight.x, other.UpperLeft.x);
   }

   private bool OverlappingInY(Rect other, float tol)
   {
      return ValuesOverlap(tol, m_LR.y, m_UL.y, other.LowerRight.y, other.UpperLeft.y);
   }

   private bool ValuesOverlap(float tol, float a0, float b0, float a1, float b1)
   {
      float max0 = Mathf.Max(a0, b0);
      float min0 = Mathf.Min(a0, b0);

      float max1 = Mathf.Max(a1, b1);
      float min1 = Mathf.Min(a1, b1);

      return (max0 - min1 >= tol) && (max1 - min0 >= tol);
   }
}
