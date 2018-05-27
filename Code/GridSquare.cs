using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSquare : RectGameObjComponent
{
   private Grid m_Grid;
   private List<GridObject> m_Objects;

   public LinkedListNode<GridSquare> Node { get; private set; }

   public int XIndex { get; private set; }
   public int YIndex { get; private set; }

   public int ObjectCount { get { return m_Objects.Count; } }

   private void Awake()
   {
      m_Grid = GetComponentInParent<Grid>();
   }

   public void Init(int xIndex, int yIndex, LinkedListNode<GridSquare> node)
   {
      XIndex = xIndex;
      YIndex = yIndex;

      Node = node;
   }

   public GridObject ObjectAt(int i)
   {
      return m_Objects[i];
   }

   public GridSquare Next(Direction dir)
   {
      return Next(dir, 1);
   }

   public GridSquare Next(Direction dir, int numAlong)
   {
      switch (dir)
      {
         case Direction.Up:
            return m_Grid[XIndex, YIndex + numAlong];
         case Direction.Down:
            return m_Grid[XIndex, YIndex - numAlong];
         case Direction.Right:
            return m_Grid[XIndex + numAlong, YIndex];
         case Direction.Left:
            return m_Grid[XIndex - numAlong, YIndex];
         default:
            throw new System.NotImplementedException();
      }
   }

   public void Add(GridObject obj)
   {
      if (m_Objects.Count == 0)
         m_Grid.OnSquareBecomesOccupied(this);

      m_Objects.Add(obj);
   }

   public void Remove(GridObject obj)
   {
      m_Objects.Remove(obj);

      if (m_Objects.Count == 0)
         m_Grid.OnSquareBecomesFree(this);
   }
}
