using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordController : MonoBehaviour, IOnSnakeStartsMoving
{
   [SerializeField]
   private GameObject m_LetterPrefab;

   private Grid m_Grid;

   // Use this for initialization
   void Awake ()
   {
      m_Grid = GetComponent<Grid>();
   }

   public void OnLetterCollected(Letter letter)
   {
      letter.SlotIntoSquare(m_Grid.GetFreeSquare(Random.Range(0, m_Grid.FreeSquareCount)), 0.2f);
   }

   public void OnSnakeStartsMoving()
   {
      var letter = (Instantiate(m_LetterPrefab, transform) as GameObject).GetComponent<Letter>();
      letter.SlotIntoSquare(m_Grid.GetFreeSquare(Random.Range(0, m_Grid.FreeSquareCount)), 0.2f);
   }
}
