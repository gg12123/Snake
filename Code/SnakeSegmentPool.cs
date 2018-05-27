using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeSegmentPool : MonoBehaviour
{
   [SerializeField]
   private int m_StartNumSegments = 10;
   [SerializeField]
   private GameObject m_SegmentPrefab;

   private Stack<SnakeSegment> m_Segments;

   private void Awake()
   {
      m_Segments = new Stack<SnakeSegment>();
      GenerateSegments();
   }

   private void GenerateSegments()
   {
      for (int i = 0; i < m_StartNumSegments; i++)
      {
         ReturnSegment((Instantiate(m_SegmentPrefab) as GameObject).GetComponent<SnakeSegment>());
      }
   }

   public SnakeSegment GetSegment()
   {
      if (m_Segments.Count == 0)
         return (Instantiate(m_SegmentPrefab) as GameObject).GetComponent<SnakeSegment>();

      var seg = m_Segments.Pop();
      seg.gameObject.SetActive(true);

      return seg;
   }

   public void ReturnSegment(SnakeSegment segment)
   {
      segment.gameObject.SetActive(false);
      m_Segments.Push(segment);
   }
}
