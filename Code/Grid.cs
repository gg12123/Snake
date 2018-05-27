using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
   [SerializeField]
   private GameObject m_SquarePrefab;
   [SerializeField]
   private float m_SquareSize = 1.5f;
   [SerializeField]
   private int m_NumSquaresAlong = 25;
   [SerializeField]
   private int m_NumSquaresUp = 15;

   private GridSquare[,] m_Squares;

   public int XCount { get { return m_NumSquaresAlong; } }
   public int YCount { get { return m_NumSquaresUp; } }

   private LinkedList<GridSquare> m_OccupiedSquares;
   private LinkedList<GridSquare> m_FreeSquares;

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
      var c = Camera.main;

      Vector3 bottomLeftSceen = c.ScreenToWorldPoint(Vector3.zero);
      Vector3 upperRightScreen = c.ScreenToWorldPoint(new Vector3(c.pixelWidth, c.pixelHeight, 0.0f));

      float screenWidth = upperRightScreen.x - bottomLeftSceen.x;
      float screenHeight = upperRightScreen.y - bottomLeftSceen.y;

      float width = m_SquareSize * m_NumSquaresAlong;
      float height = m_SquareSize * m_NumSquaresUp;

      Debug.Log("Width = " + width.ToString());
      Debug.Log("Screen width = " + screenWidth.ToString());
      Debug.Log("Height = " + height.ToString());
      Debug.Log("Screen height = " + screenHeight.ToString());

      if (width < screenWidth)
         Debug.LogError("Grid not big enough in X");

      if (height < screenHeight)
         Debug.LogError("Grid not big enough in Y");

      m_Squares = new GridSquare[m_NumSquaresAlong, m_NumSquaresUp];

      for (int i = 0; i < m_NumSquaresAlong; i++)
      {
         for (int j = 0; j < m_NumSquaresUp; j++)
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
         }
      }
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
}
