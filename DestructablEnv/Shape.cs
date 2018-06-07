using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape : MonoBehaviour
{
   public List<Vector3> EdgePoints { get; private set; }
   public List<EdgePair> EdgePairs{ get; private set; }
   public List<Vector3> Points { get; private set; }
   public List<Face> Faces { get; private set; }

   private ShapePool m_ShapePool;
   private FaceMeshPool m_MeshPool;

   private Vector3 CalculateCentre(List<EdgePair> pairs)
   {
      Vector3 centre = Vector3.zero;

      for (int i = 0; i < pairs.Count; i++)
         centre += pairs[i].Midpoint();

      return centre / pairs.Count;
   }

   private void Divide(Shape shapeAbove, Shape shapeBelow, Vector3 P0, Vector3 n)
   {
      var facesToSplit = new List<Face>();

      for (int i = 0; i < EdgePairs.Count; i++)
      {
         EdgePairs[i].Clip(n, P0, shapeBelow.EdgePairs, shapeAbove.EdgePairs, facesToSplit);
      }

      Edge e1 = null;
      Edge e2 = null;

      for (int i = 0; i < facesToSplit.Count; i++)
      {
         facesToSplit[i].Split(n, P0, shapeBelow.EdgePairs, shapeAbove.EdgePairs, out e1, out e2);
      }

      var f1 = new Face();
      var f2 = new Face();

      f1.PutOntoOpenHole(e1);
      f2.PutOntoOpenHole(e2);
   }

   private void InitNewShape(Shape shape)
   {
      var centre = CalculateCentre(shape.EdgePairs);

      var pairs = shape.EdgePairs;
      for (int i = 0; i < pairs.Count; i++)
      {
         pairs[i].OnClippingFinished(shape.EdgePoints, shape.Points, centre, shape, shape.Faces);
      }

      shape.transform.position = transform.TransformPoint(centre);
      shape.transform.rotation = transform.rotation;

      var faces = shape.Faces;
      for (int i = 0; i < faces.Count; i++)
      {
         faces[i].AddMesh(m_MeshPool);
      }
   }

   public void Split(Vector3 P0, Vector3 n)
   {
      var shapeAbove = m_ShapePool.GetShape();
      var shapeBelow = m_ShapePool.GetShape();

      Divide(shapeAbove, shapeBelow, P0, n);

      InitNewShape(shapeAbove);
      InitNewShape(shapeBelow);

      m_ShapePool.Return(this);
   }
}
