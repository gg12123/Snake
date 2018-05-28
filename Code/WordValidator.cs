using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordValidator
{
   private string m_Word;
   private int m_Current;

   public void Init(string word)
   {
      m_Current = 0;
      m_Word = word;
   }

   public bool OnNewChar(char c)
   {
      m_Current++;
      return (m_Word[m_Current - 1] == c);
   }

   public bool IsFinished()
   {
      return (m_Current >= m_Word.Length);
   }
}
