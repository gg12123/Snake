using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateInitialShape2 : MonoBehaviour
{
   [System.Serializable]
   public class ShapeParams
   {
      public Vector3 Position;
      public Vector3 Scale;
      public float Drag;
   }

   [SerializeField]
   private ShapeParams[] m_Params;

   // Use this for initialization
   void Awake ()
   {
      for (int i = 0; i < m_Params.Length; i++)
         Create(i);
   }

   private void Create(int i)
   {
      var shape = GetComponent<RigidBodyPool>().GetBody().GetComponent<Shape2>();

      shape.Clear();

      var s = m_Params[i].Scale;

      var P0 = new Point2(new Vector3(s.x, s.y, s.z));
      var P1 = new Point2(new Vector3(-s.x, s.y, s.z));
      var P2 = new Point2(new Vector3(-s.x, s.y, -s.z));
      var P3 = new Point2(new Vector3(s.x, s.y, -s.z));

      var P4 = new Point2(new Vector3(s.x, -s.y, s.z));
      var P5 = new Point2(new Vector3(-s.x, -s.y, s.z));
      var P6 = new Point2(new Vector3(-s.x, -s.y, -s.z));
      var P7 = new Point2(new Vector3(s.x, -s.y, -s.z));

      // points

      shape.AddPoint(P0);
      shape.AddPoint(P1);
      shape.AddPoint(P2);
      shape.AddPoint(P3);
      shape.AddPoint(P4);
      shape.AddPoint(P5);
      shape.AddPoint(P6);
      shape.AddPoint(P7);

      // edges

      shape.Edges.Add(new Edge2(P1, P0));
      shape.Edges.Add(new Edge2(P0, P3));
      shape.Edges.Add(new Edge2(P3, P2));
      shape.Edges.Add(new Edge2(P2, P1));

      shape.Edges.Add(new Edge2(P0, P4));
      shape.Edges.Add(new Edge2(P3, P7));
      shape.Edges.Add(new Edge2(P2, P6));
      shape.Edges.Add(new Edge2(P1, P5));

      shape.Edges.Add(new Edge2(P4, P7));
      shape.Edges.Add(new Edge2(P7, P6));
      shape.Edges.Add(new Edge2(P6, P5));
      shape.Edges.Add(new Edge2(P5, P4));

      // faces

      shape.Faces.Add(MakeFace(P1, P0, P3, P2));
      shape.Faces.Add(MakeFace(P0, P4, P7, P3));
      shape.Faces.Add(MakeFace(P3, P7, P6, P2));
      shape.Faces.Add(MakeFace(P2, P6, P5, P1));
      shape.Faces.Add(MakeFace(P1, P5, P4, P0));
      shape.Faces.Add(MakeFace(P4, P5, P6, P7));

      // init

      var pool = GetComponent<FaceMeshPool>();

      shape.CentreAndCache();

      foreach (var f in shape.Faces)
         f.AddMeshAndCachePoints(pool, shape.transform);
   }

   private Vector3 Normal(Point2 p1, Point2 p2, Point2 p3)
   {
      var n = Vector3.Cross(p3.Point - p2.Point, p1.Point - p2.Point).normalized;

      if (Vector3.Dot(p1.Point, n) < 0.0f)
         n *= -1.0f;

      return n;
   }

   private Face2 MakeFace(Point2 p1, Point2 p2, Point2 p3, Point2 p4)
   {
      return new Face2(new List<Point2>() { p1, p2, p3, p4 }, Normal(p1, p2, p3));
   }
}
