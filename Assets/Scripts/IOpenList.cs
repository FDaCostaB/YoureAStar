using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOpenList<T>{
    public int size {get;}
    public void Enqueue(T elem, float p);
    public T Dequeue();
    public bool Exist(Predicate<T> p);
    public int Find(Predicate<T> p);
    public void changePriority(int i, float p);
    public float getTopPriority();
}
