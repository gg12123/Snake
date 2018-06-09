﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Face : IEnumerable<Edge>
{
   private Shape m_Owner;
   private FaceMesh m_Mesh;
   private int m_NumPoints;
   private Vector3 m_Normal;

   private Edge m_FormsSplitWithNext1;
   private Edge m_FormsSplitWithNext2;

   private Edge m_Head;

   private List<Vector3> m_Points;

   public Face()
   {
      m_Points = new List<Vector3>();
   }

   public void Init(Edge head, int numPoints, Vector3 normal)
   {
      m_Head = head;
      m_NumPoints = numPoints;
      m_Normal = normal;
   }

   public void AddEdgeThatFormsSplitWithNext(Edge edge, List<Face> toBeSplit)
   {
      if (m_FormsSplitWithNext1 == null)
      {
         m_FormsSplitWithNext1 = edge;
      }
      else if (m_FormsSplitWithNext2 == null)
      {
         m_FormsSplitWithNext2 = edge;
      }
      else
      {
         Debug.LogError("Face been given more than two edges that form split");
      }
   }

   public void ClearOwnerShape()
   {
      m_Owner = null;
   }

   public void OnNewOwner(Shape owner)
   {
      if (m_Owner != owner)
      {
         owner.Faces.Add(this);
      }
      m_Owner = owner;
   }

   public void Split(Vector3 n, Vector3 P0, List<EdgePair> edgesBelow, List<EdgePair> edgesAbove,
                     out Edge edgeWithNullFaceAbove, out Edge edgeWithNullFaceBelow)
   {
      var Ea = m_FormsSplitWithNext1;
      var Eb = m_FormsSplitWithNext2;

      var EaNext = Ea.Next;
      var EbNext = Eb.Next;

      var startForThis = Eb.End;
      var endForThis = EaNext.Start;
      var startForNew = Ea.End;
      var endForNew = EbNext.Start;

      var newFace = new Face();

      var newPairForNewFace = new EdgePair(newFace, null);
      var newPairForThis = new EdgePair(this, null);

      // insert edges into linked lists
      Ea.InsertAfterAndBreak(newPairForNewFace.Edge1);
      EbNext.InsertBeforeAndBreak(newPairForNewFace.Edge1);

      EaNext.InsertBeforeAndBreak(newPairForThis.Edge1);
      Eb.InsertAfterAndBreak(newPairForThis.Edge1);

      // set start and end points
      newPairForThis.Edge1.Start = startForThis;
      newPairForThis.Edge1.End = endForThis;

      newPairForNewFace.Edge1.Start = startForNew;
      newPairForNewFace.Edge1.End = endForNew;

      // add new edges to edges lists
      var newFaceIsAbove = Utils.PointIsAbovePlane(n, P0, newPairForNewFace.Edge1.Next.End.Point);

      if (newFaceIsAbove)
      {
         edgesAbove.Add(newPairForNewFace);
         edgesBelow.Add(newPairForThis);

         edgeWithNullFaceAbove = newPairForNewFace.Edge2;
         edgeWithNullFaceBelow = newPairForThis.Edge2;
      }
      else
      {
         edgesAbove.Add(newPairForThis);
         edgesBelow.Add(newPairForNewFace);

         edgeWithNullFaceAbove = newPairForThis.Edge2;
         edgeWithNullFaceBelow = newPairForNewFace.Edge2;
      }

      // set face ref on new face's edges and count points
      int newFacePointCount = 0;
      foreach (var edge in newFace)
      {
         edge.OwnerFace = newFace;
         newFacePointCount++;
      }

      // init
      Init(newPairForThis.Edge1, m_NumPoints - newFacePointCount + 4, m_Normal);
      newFace.Init(newPairForNewFace.Edge1, newFacePointCount, m_Normal);

      m_FormsSplitWithNext1 = null;
      m_FormsSplitWithNext2 = null;
   }

   private Edge NextOnOpenHole(Edge e)
   {
      var prev = e.Prev;
      var otherPrevAgain = prev.OwnerPair.Other(prev).Prev;

      return otherPrevAgain.OwnerPair.Other(otherPrevAgain);
   }

   public void PutOntoOpenHole(Edge edgeOnHole, Vector3 normal)
   {
      var x = edgeOnHole;
      var xNext = NextOnOpenHole(x);
      var numPoints = 0;

      do
      {
         x.InsertAfter(xNext);
         x.OwnerFace = this;
         numPoints++;

         x = xNext;
         xNext = NextOnOpenHole(xNext);

      } while(x != edgeOnHole);

      Init(edgeOnHole, numPoints, normal);
   }

   public void AddMesh(FaceMeshPool pool)
   {
      if (m_Mesh != null && m_Mesh.NumPoints != m_NumPoints)
      {
         pool.ReturnMesh(m_Mesh);
         m_Mesh = null;
      }

      if (m_Mesh == null)
      {
         m_Mesh = pool.GetMesh(m_NumPoints);
         m_Mesh.transform.parent = m_Owner.transform;
      }

      foreach (var e in this)
         m_Points.Add(e.End.Point);

      m_Mesh.SetVerts(m_Points);
      m_Mesh.SetNormal(m_Normal);
   }

   public IEnumerator<Edge> GetEnumerator()
   {
      var e = m_Head;

      do
      {
         yield return e;
         e = e.Next;
      } while (e != m_Head);
   }

   IEnumerator IEnumerable.GetEnumerator()
   {
      var e = m_Head;

      do
      {
         yield return e;
         e = e.Next;
      } while (e != m_Head);
   }
}
