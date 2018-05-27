using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RectGameObjComponent : MonoBehaviour
{
   public Rect Rect { get; private set; }

   private void Awake()
   {
      Rect = GetComponent<Rect>();
   }

   protected virtual void OnAwake() { }
}
