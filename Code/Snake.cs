using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISnakeController
{
   bool ChangeDirection(out Direction dir);
}

public class Snake : MonoBehaviour
{
   [SerializeField]
   private float m_Speed = 1.0f;
   [SerializeField]
   private float m_Width = 1.0f;

   private SnakeSegmentPool m_SegmentPool;
   private LinkedList<SnakeSegment> m_Segments;
   private ISnakeController m_Controller;

   public SnakeSegment Head { get { return m_Segments.First.Value; } }
   public float Width { get { return m_Width; } }

   // Use this for initialization
   void Awake ()
   {
      m_SegmentPool = GetComponent<SnakeSegmentPool>();
      m_Segments = new LinkedList<SnakeSegment>();
      m_Controller = GetComponent<ISnakeController>();

      enabled = false;
   }

   private void Start()
   {
      var firstSegment = m_SegmentPool.GetSegment();

      firstSegment.Rect.Init(Vector2.zero, m_Width * 2.0f * Rect.VectorDirection(Direction.Right) + m_Width * Rect.VectorDirection(Direction.Down));
      InitFirstSegmentGridObject(firstSegment);
      firstSegment.Init(Direction.Left);
      m_Segments.AddFirst(firstSegment);
   }

   private void InitFirstSegmentGridObject(SnakeSegment firstSegment)
   {
      int xMin = int.MaxValue;
      int xMax = -1;
      int yMin = int.MaxValue;
      int yMax = -1;

      Grid grid = GetComponentInParent<Grid>();

      for (int x = 0; x < grid.XCount; x++)
      {
         for (int y = 0; y < grid.YCount; y++)
         {
            if (grid[x, y].Rect.IsOverlappingWith(firstSegment.Rect))
            {
               if (x < xMin)
                  xMin = x;

               if (x > xMax)
                  xMax = x;

               if (y < yMin)
                  yMin = y;

               if (y > yMax)
                  yMax = y;
            }
         }
      }

      firstSegment.GridObj.Init(grid[xMin, yMax], grid[xMax, yMin]);
   }

   // Update is called once per frame
   void Update ()
   {
      Direction newDir;
      if (m_Controller.ChangeDirection(out newDir))
         ChangeDirection(newDir);

      float movement = Time.deltaTime * m_Speed;

      Head.Expand(movement);
      HandleShrinking(movement);
   }

   private Direction GetNextDirection()
   {
      return ((m_Segments.Count == 1) ? Head.Direction : m_Segments.Last.Previous.Value.Direction);
   }

   private void HandleShrinking(float movement)
   {
      float amountShrunk;
      while (!m_Segments.Last.Value.Shrink(GetNextDirection(), movement, out amountShrunk))
      {
         m_SegmentPool.ReturnSegment(m_Segments.Last.Value);
         m_Segments.RemoveLast();

         movement -= amountShrunk;
      }
   }

   private bool NewDirectionIsValid(Direction newDir, Direction currDir)
   {
      switch (currDir)
      {
         case Direction.Up:
         case Direction.Down:
            return (newDir == Direction.Left || newDir == Direction.Right);
         case Direction.Right:
         case Direction.Left:
            return (newDir == Direction.Up || newDir == Direction.Down);
         default:
            throw new System.Exception();
      }
   }

   private bool HeadIsLongEnoughForDirChange(SnakeSegment head)
   {
      return (head.Rect.Size(head.Direction) > m_Width);
   }

   private void ChangeDirection(Direction dir)
   {
      var currentHead = Head;

      if (!NewDirectionIsValid(dir, currentHead.Direction))
         return;

      if (!HeadIsLongEnoughForDirChange(currentHead))
         return;

      SnakeSegment newSegment = null;

      switch (currentHead.Direction)
      {
         case Direction.Up:
            newSegment = (dir == Direction.Left) ? HandleDirChangeUpToLeft(currentHead) : HandleDirChangeUpToRight(currentHead);
            break;

         case Direction.Down:
            newSegment = (dir == Direction.Left) ? HandleDirChangeDownToLeft(currentHead) : HandleDirChangeDownToRight(currentHead);
            break;

         case Direction.Left:
            newSegment = (dir == Direction.Up) ? HandleDirChangeLeftToUp(currentHead) : HandleDirChangeLeftToDown(currentHead);
            break;

         case Direction.Right:
            newSegment = (dir == Direction.Up) ? HandleDirChangeRightToUp(currentHead) : HandleDirChangeRightToDown(currentHead);
            break;
      }

      newSegment.Init(dir);
      m_Segments.AddFirst(newSegment);
   }

   private SnakeSegment HandleDirChangeRightToDown(SnakeSegment currHead)
   {
      var newHead = m_SegmentPool.GetSegment();

      newHead.Rect.Init(currHead.Rect.LowerRight + m_Width * Rect.VectorDirection(Direction.Left), currHead.Rect.LowerRight);
      newHead.GridObj.Init(SquareOnRectOuter(currHead.GridObj.LowerRight, newHead.Rect, Direction.Left), currHead.GridObj.LowerRight);

      return newHead;
   }

   private SnakeSegment HandleDirChangeRightToUp(SnakeSegment currHead)
   {
      var newHead = m_SegmentPool.GetSegment();

      newHead.Rect.Init(currHead.Rect.UpperRight + m_Width * Rect.VectorDirection(Direction.Left), currHead.Rect.UpperRight);
      newHead.GridObj.Init(SquareOnRectOuter(currHead.GridObj.UpperRight, newHead.Rect, Direction.Left), currHead.GridObj.UpperRight);

      return newHead;
   }

   private SnakeSegment HandleDirChangeLeftToDown(SnakeSegment currHead)
   {
      var newHead = m_SegmentPool.GetSegment();

      newHead.Rect.Init(currHead.Rect.LowerLeft, currHead.Rect.LowerLeft + m_Width * Rect.VectorDirection(Direction.Right));
      newHead.GridObj.Init(currHead.GridObj.LowerLeft, SquareOnRectOuter(currHead.GridObj.LowerLeft, newHead.Rect, Direction.Right));

      return newHead;
   }

   private SnakeSegment HandleDirChangeLeftToUp(SnakeSegment currHead)
   {
      var newHead = m_SegmentPool.GetSegment();

      newHead.Rect.Init(currHead.Rect.UpperLeft, currHead.Rect.UpperLeft + m_Width * Rect.VectorDirection(Direction.Right));
      newHead.GridObj.Init(currHead.GridObj.UpperLeft, SquareOnRectOuter(currHead.GridObj.UpperLeft, newHead.Rect, Direction.Right));

      return newHead;
   }

   private SnakeSegment HandleDirChangeUpToLeft(SnakeSegment currHead)
   {
      var newHead = m_SegmentPool.GetSegment();

      newHead.Rect.Init(currHead.Rect.UpperLeft, currHead.Rect.UpperLeft + m_Width * Rect.VectorDirection(Direction.Down));
      newHead.GridObj.Init(currHead.GridObj.UpperLeft, SquareOnRectOuter(currHead.GridObj.UpperLeft, newHead.Rect, Direction.Down));

      return newHead;
   }

   private SnakeSegment HandleDirChangeUpToRight(SnakeSegment currHead)
   {
      var newHead = m_SegmentPool.GetSegment();

      newHead.Rect.Init(currHead.Rect.UpperRight, currHead.Rect.UpperRight + m_Width * Rect.VectorDirection(Direction.Down));
      newHead.GridObj.Init(currHead.GridObj.UpperRight, SquareOnRectOuter(currHead.GridObj.UpperRight, newHead.Rect, Direction.Down));

      return newHead;
   }

   private SnakeSegment HandleDirChangeDownToRight(SnakeSegment currHead)
   {
      var newHead = m_SegmentPool.GetSegment();

      newHead.Rect.Init(currHead.Rect.LowerRight + m_Width * Rect.VectorDirection(Direction.Up), currHead.Rect.LowerRight);
      newHead.GridObj.Init(SquareOnRectOuter(currHead.GridObj.LowerRight, newHead.Rect, Direction.Up), currHead.GridObj.LowerRight);

      return newHead;
   }

   private SnakeSegment HandleDirChangeDownToLeft(SnakeSegment currHead)
   {
      var newHead = m_SegmentPool.GetSegment();

      newHead.Rect.Init(currHead.Rect.LowerLeft + m_Width * Rect.VectorDirection(Direction.Up), currHead.Rect.LowerLeft);
      newHead.GridObj.Init(SquareOnRectOuter(currHead.GridObj.LowerLeft, newHead.Rect, Direction.Up), currHead.GridObj.LowerLeft);

      return newHead;
   }

   private GridSquare SquareOnRectOuter(GridSquare startSquare, Rect rect, Direction dir)
   {
      var next = startSquare.Next(dir);
      while (next.Rect.IsOverlappingWith(rect, dir))
      {
         startSquare = next;
         next = next.Next(dir);
      }
      return startSquare;
   }
}
