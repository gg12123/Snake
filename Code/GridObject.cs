using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridObject : RectGameObjComponent
{
   private GridSquare m_UL;
   private GridSquare m_LR;

   private GridSquareLineEnumerator m_Enumerator;
   private GridSquareSquareEnumerator m_SquareEnumerator;

   public GridSquare UpperLeft { get { return m_UL; } }
   public GridSquare LowerRight { get { return m_LR; } }
   public GridSquare UpperRight { get { return m_UL.Next(Direction.Right, HorizontalCount() - 1); } }
   public GridSquare LowerLeft { get { return m_LR.Next(Direction.Left, HorizontalCount() - 1); } }

   protected override void OnAwake()
   {
      m_Enumerator = new GridSquareLineEnumerator();
      m_SquareEnumerator = new GridSquareSquareEnumerator();
   }

   public void Init(GridSquare uL, GridSquare lR)
   {
      m_UL = uL;
      m_LR = lR;

      var squares = GetAll();

      for (var s = squares.First(); s != null; s = squares.Next())
         s.Add(this);
   }

   private int HorizontalCount()
   {
      return (Mathf.Abs(m_UL.XIndex - m_LR.XIndex) + 1);
   }

   private int VerticalCount()
   {
      return (Mathf.Abs(m_UL.YIndex - m_LR.YIndex) + 1);
   }

   public IMyEnumerator<GridSquare> GetAll()
   {
      m_SquareEnumerator.Init(m_UL, m_LR);
      return m_SquareEnumerator;
   }

   public IMyEnumerator<GridSquare> GetFront(Direction dir)
   {
      switch (dir)
      {
         case Direction.Up:
            m_Enumerator.Init(m_UL, Direction.Right, HorizontalCount());
            break;
         case Direction.Down:
            m_Enumerator.Init(m_LR, Direction.Left, HorizontalCount());
            break;
         case Direction.Left:
            m_Enumerator.Init(m_UL, Direction.Down, VerticalCount());
            break;
         case Direction.Right:
            m_Enumerator.Init(m_LR, Direction.Up, VerticalCount());
            break;
      }
      return m_Enumerator;
   }

   public IMyEnumerator<GridSquare> GetBack(Direction dir)
   {
      switch (dir)
      {
         case Direction.Up:
            m_Enumerator.Init(m_LR, Direction.Left, HorizontalCount());
            break;
         case Direction.Down:
            m_Enumerator.Init(m_UL, Direction.Right, HorizontalCount());
            break;
         case Direction.Left:
            m_Enumerator.Init(m_LR, Direction.Up, VerticalCount());
            break;
         case Direction.Right:
            m_Enumerator.Init(m_UL, Direction.Down, VerticalCount());
            break;
      }
      return m_Enumerator;
   }

   private void OnExpandedToNewSquares(Direction dir)
   {
      switch (dir)
      {
         case Direction.Up:
         case Direction.Left:
            m_UL = m_UL.Next(dir);
            break;
         case Direction.Down:
         case Direction.Right:
            m_LR = m_LR.Next(dir);
            break;
      }
   }

   private void OnShrunkAwayFromSquares(Direction dir)
   {
      switch (dir)
      {
         case Direction.Up:
         case Direction.Left:
            m_LR = m_LR.Next(dir);
            break;
         case Direction.Down:
         case Direction.Right:
            m_UL = m_UL.Next(dir);
            break;
      }
   }

   private bool OverlappedWithAnyOfNext(IMyEnumerator<GridSquare> squares, Direction dir)
   {
      for (var square = squares.First(); square != null; square = squares.Next())
      {
         if (square.Next(dir).Rect.IsOverlappingWith(Rect))
         {
            return true;
         }
      }
      return false;
   }

   private bool OverlappedWithAny(IMyEnumerator<GridSquare> squares)
   {
      for (var square = squares.First(); square != null; square = squares.Next())
      {
         if (square.Rect.IsOverlappingWith(Rect))
         {
            return true;
         }
      }
      return false;
   }

   public void OnExpanded(Direction dir)
   {
      var front = GetFront(dir);

      if (OverlappedWithAnyOfNext(front, dir))
      {
         front.Reset();

         for (var square = front.First(); square != null; square = front.Next())
         {
            square.Next(dir).Add(this);
         }

         OnExpandedToNewSquares(dir);
      }
   }

   public void OnShrunk(Direction dir)
   {
      var back = GetBack(dir);

      if (!OverlappedWithAny(back))
      {
         back.Reset();
         for (var square = back.First(); square != null; square = back.Next())
         {
            square.Remove(this);
         }

         OnShrunkAwayFromSquares(dir);
      }
   }

   public void OnRemovedFromGrid()
   {
      var squares = GetAll();

      for (var square = squares.First(); square != null; square = squares.Next())
         square.Remove(this);

      m_UL = m_LR = null;
   }

   public void OnCollidedWithSnakeHead(SnakeSegment head)
   {

   }
}
