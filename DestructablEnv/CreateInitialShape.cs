using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateInitialShape : MonoBehaviour
{
   [System.Serializable]
   public class ShapeParams
   {
      public Vector3 Position;
      public float Drag;
   }

   [SerializeField]
   private ShapeParams[] m_Positions;

   private Shape m_Shape;

   // Use this for initialization
   void Awake ()
   {
      for (int i = 0; i < m_Positions.Length; i++)
         CreateShape(i);
   }

   private void CreateShape(int i)
   {
      m_Shape = GetComponent<ShapePool>().GetShape();

      var P0 = new ShapePoint(new Vector3(1.0f, 1.0f, 1.0f));
      var P1 = new ShapePoint(new Vector3(-1.0f, 1.0f, 1.0f));
      var P2 = new ShapePoint(new Vector3(-1.0f, 1.0f, -1.0f));
      var P3 = new ShapePoint(new Vector3(1.0f, 1.0f, -1.0f));

      var P4 = new ShapePoint(new Vector3(1.0f, -1.0f, 1.0f));
      var P5 = new ShapePoint(new Vector3(-1.0f, -1.0f, 1.0f));
      var P6 = new ShapePoint(new Vector3(-1.0f, -1.0f, -1.0f));
      var P7 = new ShapePoint(new Vector3(1.0f, -1.0f, -1.0f));

      var P0P4pair = new EdgePair(null, null);
      var P1P5pair = new EdgePair(null, null);
      var P2P6pair = new EdgePair(null, null);
      var P3P7pair = new EdgePair(null, null);

      ConstructFace(P0P4pair, P1P5pair, P0, P4, P5, P1, Vector3.forward, true);
      ConstructFace(P1P5pair, P2P6pair, P1, P5, P6, P2, -Vector3.right, false);
      ConstructFace(P2P6pair, P3P7pair, P2, P6, P7, P3, -Vector3.forward, true);
      ConstructFace(P3P7pair, P0P4pair, P3, P7, P4, P0, Vector3.right, false);

      var top = new Face();
      top.PutOntoOpenHole(P0P4pair.Edge1.Prev.Other, Vector3.up);
      top.OnNewOwner(m_Shape);

      var bottom = new Face();
      bottom.PutOntoOpenHole(P0P4pair.Edge1.Next.Other, -Vector3.up);
      bottom.OnNewOwner(m_Shape);

      P0P4pair.OnNewOwner(m_Shape);
      P1P5pair.OnNewOwner(m_Shape);
      P2P6pair.OnNewOwner(m_Shape);
      P3P7pair.OnNewOwner(m_Shape);

      m_Shape.Points.Add(P0.Point);
      m_Shape.Points.Add(P1.Point);
      m_Shape.Points.Add(P2.Point);
      m_Shape.Points.Add(P3.Point);
      m_Shape.Points.Add(P4.Point);
      m_Shape.Points.Add(P5.Point);
      m_Shape.Points.Add(P6.Point);
      m_Shape.Points.Add(P7.Point);

      P0.Index = 0;
      P1.Index = 1;
      P2.Index = 2;
      P3.Index = 3;
      P4.Index = 4;
      P5.Index = 5;
      P6.Index = 6;
      P7.Index = 7;

      foreach (var pair in m_Shape.EdgePairs)
      {
         m_Shape.EdgePoints.Add(pair.Edge1.Start.Index);
         m_Shape.EdgePoints.Add(pair.Edge1.End.Index);
      }

      m_Shape.EnsureWorldPointsListIsBigEnough();

      var meshPool = GetComponent<FaceMeshPool>();
      foreach (var face in m_Shape.Faces)
         face.AddMesh(meshPool);

      m_Shape.transform.position = m_Positions[i].Position;
      m_Shape.transform.rotation = Quaternion.identity;
      m_Shape.UpdateWorldPoints();
      m_Shape.GetComponent<MyRigidbody>().Drag = m_Positions[i].Drag;
   }

   private void ConstructFace(EdgePair P0P1pair, EdgePair P2P3Pair,
                              ShapePoint P0, ShapePoint P1, ShapePoint P2, ShapePoint P3,
                              Vector3 n,
                              bool setEdge1)
   {
      var face = new Face();

      var P1P2Pair = new EdgePair(null, null);
      var P3P0Pair = new EdgePair(null, null);

      // setup the linked lists
      RequiredEdge(P0P1pair, setEdge1).InsertAfter(RequiredEdge(P1P2Pair, setEdge1));
      RequiredEdge(P1P2Pair, setEdge1).InsertAfter(RequiredEdge(P2P3Pair, setEdge1));
      RequiredEdge(P2P3Pair, setEdge1).InsertAfter(RequiredEdge(P3P0Pair, setEdge1));
      RequiredEdge(P3P0Pair, setEdge1).InsertAfter(RequiredEdge(P0P1pair, setEdge1));

      // set the start and end points
      RequiredEdge(P0P1pair, setEdge1).Start = P0;
      RequiredEdge(P0P1pair, setEdge1).End = P1;

      RequiredEdge(P1P2Pair, setEdge1).Start = P1;
      RequiredEdge(P1P2Pair, setEdge1).End = P2;

      RequiredEdge(P2P3Pair, setEdge1).Start = P2;
      RequiredEdge(P2P3Pair, setEdge1).End = P3;

      RequiredEdge(P3P0Pair, setEdge1).Start = P3;
      RequiredEdge(P3P0Pair, setEdge1).End = P0;

      // set the face
      RequiredEdge(P0P1pair, setEdge1).OwnerFace = face;
      RequiredEdge(P1P2Pair, setEdge1).OwnerFace = face;
      RequiredEdge(P2P3Pair, setEdge1).OwnerFace = face;
      RequiredEdge(P3P0Pair, setEdge1).OwnerFace = face;

      // init the face
      face.Init(RequiredEdge(P0P1pair, setEdge1), 4, n);
      face.OnNewOwner(m_Shape);

      // set stuff on the shape
      P1P2Pair.OnNewOwner(m_Shape);
      P3P0Pair.OnNewOwner(m_Shape);
   }

   private Edge RequiredEdge(EdgePair pair, bool set1)
   {
      return set1 ? pair.Edge1 : pair.Edge2;
   }
}
