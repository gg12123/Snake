using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGame
{
   void Play(System.Action onFinished);
}

public class GameController : MonoBehaviour, IOnSnakeStartsMoving
{
   [SerializeField]
   private float m_TimeBetweenGames = 1.0f;

   private IGame[] m_Games;

   private void Awake()
   {
      m_Games = GetComponents<IGame>();
   }

   public void OnSnakeStartsMoving()
   {
      RandomGame().Play(OnActiveGameFinished);
   }

   private void OnActiveGameFinished()
   {
      StartCoroutine(WaitAndThenPlayNextGame());
   }

   private IGame RandomGame()
   {
      return m_Games[Random.Range(0, m_Games.Length)];
   }

   private IEnumerator WaitAndThenPlayNextGame()
   {
      var wait = new WaitForSeconds(m_TimeBetweenGames);
      yield return wait;
      RandomGame().Play(OnActiveGameFinished);
   }
}
