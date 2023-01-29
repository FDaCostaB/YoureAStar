using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOpenList<T>{
    public int size {get;}
    public void Enqueue(T elem, float p);
    public T Dequeue();
    public bool Contains(T elem);
    public void changePriority(T item, float p);
}
