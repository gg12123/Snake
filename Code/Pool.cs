using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Pool<T> where T : MonoBehaviour
{
   [SerializeField]
   private GameObject m_Prefab;
   [SerializeField]
   private int m_StartNumObjects = 10;

   private Transform m_Parent;
   private Stack<T> m_Objects;

   private bool m_Initialised = false;

   public void Init(Transform parent)
   {
      if (!m_Initialised)
      {
         m_Parent = parent;
         m_Objects = new Stack<T>();
         GenerateObjects();

         m_Initialised = true;
      }
   }

   private void GenerateObjects()
   {
      for (int i = 0; i < m_StartNumObjects; i++)
      {
         ReturnObject((GameObject.Instantiate(m_Prefab, m_Parent) as GameObject).GetComponent<T>());
      }
   }

   public T GetObject()
   {
      if (m_Objects.Count == 0)
         return (GameObject.Instantiate(m_Prefab) as GameObject).GetComponent<T>();

      var obj = m_Objects.Pop();
      obj.gameObject.SetActive(true);

      return obj;
   }

   public void ReturnObject(T obj)
   {
      obj.gameObject.SetActive(false);
      obj.transform.SetParent(m_Parent, false);
      m_Objects.Push(obj);
   }
}

// Needed to make serialization work.

[System.Serializable]
public class SnakeSegmentPool : Pool<SnakeSegment>
{
}

[System.Serializable]
public class LetterPool : Pool<Letter>
{
}
