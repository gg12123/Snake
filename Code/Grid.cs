﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
   [SerializeField]
   private GameObject m_SquarePrefab;
   [SerializeField]
   private GameObject m_WallPrefab;
   [SerializeField]
   private float m_SquareSize = 1.0f;
   [SerializeField]
   private int m_XCount = 25;
   [SerializeField]
   private int m_YCount = 15;

   private GridSquare[,] m_Squares;

   private LinkedList<GridSquare> m_OccupiedSquares;
   private LinkedList<GridSquare> m_FreeSquares;

   public int XCount { get { return m_XCount; } }
   public int YCount { get { return m_YCount; } }

   public int FreeSquareCount { get { return m_FreeSquares.Count; } }

   public GridSquare this[int xIndex, int yIndex]
   {
      get
      {
         return m_Squares[xIndex, yIndex];
      }
   }

   // Use this for initialization
   void Awake ()
   {
      m_OccupiedSquares = new LinkedList<GridSquare>();
      m_FreeSquares = new LinkedList<GridSquare>();
      GenerateSquares();
   }

   private void GenerateSquares()
   {
      float width = m_SquareSize * XCount;
      float height = m_SquareSize * YCount;

      m_Squares = new GridSquare[XCount, YCount];

      for (int i = 0; i < XCount; i++)
      {
         for (int j = 0; j < YCount; j++)
         {
            Vector2 uL = new Vector2(i * m_SquareSize - (width / 2.0f),
                                     (j + 1) * m_SquareSize - (height / 2.0f));

            Vector2 lR = new Vector2((i + 1) * m_SquareSize - (width / 2.0f),
                                     j * m_SquareSize - (height / 2.0f));

            var square = (Instantiate(m_SquarePrefab, transform) as GameObject).GetComponent<GridSquare>();

            m_FreeSquares.AddFirst(square);
            square.Init(i, j, m_FreeSquares.First);
            square.Rect.Init(uL, lR);
            m_Squares[i, j] = square;

            if (IsPermiterSquare(square))
               PlaceWall(square);
         }
      }

      var c = Camera.main;
      c.aspect = (width - 4.0f * m_SquareSize) / (height - 4.0f * m_SquareSize);
      c.orthographicSize = 0.5f * height - 2.0f * m_SquareSize;
   }

   private bool IsPermiterSquare(GridSquare square)
   {
      return (square.XIndex <= 1 || square.XIndex >= XCount - 2 || square.YIndex <= 1 || square.YIndex >= YCount - 2);
   }

   private void PlaceWall(GridSquare square)
   {
      var wall = (Instantiate(m_WallPrefab, transform) as GameObject).GetComponent<Wall>();
      wall.SlotIntoSquare(square, 0.0f);
   }

   public void OnSquareBecomesFree(GridSquare square)
   {
      m_OccupiedSquares.Remove(square.Node);
      m_FreeSquares.AddFirst(square.Node);
   }

   public void OnSquareBecomesOccupied(GridSquare square)
   {
      m_FreeSquares.Remove(square.Node);
      m_OccupiedSquares.AddFirst(square.Node);
   }

   public GridSquare GetFreeSquare(int index)
   {
      int i = 0;
      var square = m_FreeSquares.First;
      while (i < index)
      {
         square = square.Next;
         i++;
      }
      return square.Value;
   }
}
