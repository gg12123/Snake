using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerPointer
{
   public int Index { get; set; }

   public ContainerPointer(int index)
   {
      Index = index;
   }
}

public class MyContainer<T>
{
   private T[] m_Values;
   private ContainerPointer[] m_Pointers;

   public int Count { get; private set; }
   public T this[int i] { get { return m_Values[i]; } }

   public MyContainer(int size)
   {
      m_Values = new T[size];
      m_Pointers = new ContainerPointer[size];

      for (int i = 0; i < size; i++)
         m_Pointers[i] = new ContainerPointer(i);

      Count = 0;
   }

   public void Remove(ContainerPointer pointer)
   {
      int newFree = pointer.Index;
      int end = Count - 1;

      // Move end pointer to the free slot
      m_Pointers[newFree] = m_Pointers[end];
      m_Pointers[newFree].Index = newFree;

      // Move end value
      m_Values[newFree] = m_Values[end];

      // Put the old pointer at the end
      m_Pointers[end] = pointer;
      m_Pointers[end].Index = end;

      // Null the end value
      m_Values[end] = default(T);

      Count--;
   }

   public ContainerPointer Add(T value)
   {
      int index = Count;
      m_Values[index] = value;
      Count++;
      return m_Pointers[index];
   }

   public T GetValue(ContainerPointer p)
   {
      return m_Values[p.Index];
   }
}
