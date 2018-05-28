using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RectObjectComponent : MonoBehaviour
{
   public Rect Rect { get; private set; }

   private void Awake()
   {
      Rect = GetComponent<Rect>();
      OnAwake();
   }

   protected virtual void OnAwake() { }
}
