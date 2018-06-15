using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FaceMesh : MonoBehaviour
{
   private Mesh m_Mesh;
   private List<Vector3> m_Normals;

   public int NumPoints { get; private set; }

   public void Init(int numVerts)
   {
      NumPoints = numVerts;
      m_Normals = new List<Vector3>(numVerts);

      m_Mesh = new Mesh();
      m_Mesh.SetVertices((new Vector3[numVerts]).ToList());
      m_Mesh.SetTriangles(Triangles(), 0);

      GetComponent<MeshFilter>().mesh = m_Mesh;
   }

   public void SetVerts(List<Vector3> verts)
   {
      if (verts.Count != NumPoints)
         Debug.LogError("wrong number of verts passed to face mesh");

      m_Mesh.SetVertices(verts);
   }

   public void SetNormal(Vector3 normal)
   {
      for (int i = 0; i < NumPoints; i++)
         m_Normals.Add(normal);

      m_Mesh.SetNormals(m_Normals);
   }

   private List<int> Triangles()
   {
      var tris = new List<int>(NumPoints * 3);

      int p0 = 1;
      int p1 = 2;

      int numTris = NumPoints - 2;

      for (int i = 0; i < numTris; i++)
      {
         tris.Add(0);
         tris.Add(p0);
         tris.Add(p1);

         p0++;
         p1++;
      }

      return tris;
   }
}
