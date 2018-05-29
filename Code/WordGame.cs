using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WordGame : MonoBehaviour, IGame
{
   [SerializeField]
   private LetterPool m_LetterPool;

   private Grid m_Grid;
   private WordValidator m_Validator;
   private WordGenerator m_Generator;
   private Action m_OnFinished;

   // Use this for initialization
   void Awake ()
   {
      m_Grid = GetComponent<Grid>();
      m_LetterPool.Init(transform);

      m_Validator = new WordValidator();
      m_Generator = new WordGenerator();
   }

   public void OnLetterCollected(Letter letter, Snake collector)
   {
      m_LetterPool.ReturnObject(letter);

      if (m_Validator.OnNewChar(letter.Char))
      {
         if (m_Validator.IsFinished())
         {
            m_OnFinished();
         }
      }
      else
      {
         collector.Die();
      }
   }

   public void Play(Action onFinished)
   {
      m_OnFinished = onFinished;
      BeginWord();
   }

   private void PlaceLettersOnGrid(string word)
   {
      for (int i = 0; i < word.Length; i++)
      {
         var square = m_Grid.GetFreeSquare(UnityEngine.Random.Range(0, m_Grid.FreeSquareCount));
         var letter = m_LetterPool.GetObject();

         letter.Init(word[i], square);
      }
   }

   private void BeginWord()
   {
      string word = m_Generator.GetNewWord();
      m_Validator.Init(word);
      PlaceLettersOnGrid(word);
   }
}
