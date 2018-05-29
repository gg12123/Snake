using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSquare : RectObjectComponent
{
   [SerializeField]
   private Color m_FreeColour;
   [SerializeField]
   private Color m_OccupiedColour;

   private Grid m_Grid;
   private SpriteRenderer m_Renderer;
   private List<GridObject> m_Objects;

   public ContainerPointer Node { get; private set; }

   public int XIndex { get; private set; }
   public int YIndex { get; private set; }

   public int ObjectCount { get { return m_Objects.Count; } }

   protected override void OnAwake()
   {
      m_Objects = new List<GridObject>();
      m_Grid = GetComponentInParent<Grid>();
      m_Renderer = GetComponent<SpriteRenderer>();
   }

   public void Init(int xIndex, int yIndex, ContainerPointer node)
   {
      XIndex = xIndex;
      YIndex = yIndex;

      Node = node;
      m_Renderer.color = m_FreeColour;
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
         OnBecomeOccupied();

      m_Objects.Add(obj);
   }

   public void Remove(GridObject obj)
   {
      m_Objects.Remove(obj);

      if (m_Objects.Count == 0)
         OnBecomeFree();
   }

   private void OnBecomeOccupied()
   {
      Node = m_Grid.OnSquareBecomesOccupied(this);
      m_Renderer.color = m_OccupiedColour;
   }

   private void OnBecomeFree()
   {
      Node = m_Grid.OnSquareBecomesFree(this);
      m_Renderer.color = m_FreeColour;
   }
}
