using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceMeshPool : MonoBehaviour
{
   [SerializeField]
   private GameObject m_Prefab;
   [SerializeField]
   private int m_AmountPerVertNumber = 20;
   [SerializeField]
   private int m_MaxNumVerts = 20;

   private Stack<FaceMesh>[] m_Meshes;

   private void Awake()
   {
      m_Meshes = new Stack<FaceMesh>[m_MaxNumVerts + 1];

      for (int i = 2; i <= m_MaxNumVerts; i++)
      {
         m_Meshes[i] = new Stack<FaceMesh>(m_AmountPerVertNumber);

         for (int j = 0; j < m_AmountPerVertNumber; j++)
            m_Meshes[i].Push(CreateNew(i));
      }
   }

   private FaceMesh CreateNew(int numVers)
   {
      var m = (Instantiate(m_Prefab, transform) as GameObject).GetComponent<FaceMesh>();
      m.Init(numVers);
      return m;
   }

   public FaceMesh GetMesh(int numPoints)
   {
      var meshes = m_Meshes[numPoints];
      var m = meshes.Count > 0 ? meshes.Pop() : CreateNew(numPoints);

      m.gameObject.SetActive(true);
      return m;
   }

   public void ReturnMesh(FaceMesh mesh)
   {
      mesh.gameObject.SetActive(false);
      m_Meshes[mesh.NumPoints].Push(mesh);
   }
}
