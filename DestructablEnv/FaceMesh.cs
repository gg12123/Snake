using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceMesh : MonoBehaviour
{
   private Mesh m_Mesh;

   public int NumPoints { get; private set; }

   public void Init(int numVerts)
   {
      NumPoints = numVerts;
      // setup triangles etc.
   }

   public void SetVerts(List<Vector3> verts)
   {
      if (verts.Count != NumPoints)
         Debug.LogError("wrong number of verts passed to face mesh");

      m_Mesh.SetVertices(verts);
   }
}
