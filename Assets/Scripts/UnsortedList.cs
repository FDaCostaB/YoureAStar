// C# code to implement priority-queue
// using array implementation of
// binary heap
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
class UnsortedList<T> : IOpenList<T> {
 
    List<Pair<T,float>> List = new List<Pair<T,float>>();
    public int size {get => List.Count;}
    public void Enqueue(T elem, float p){
        List.Add(new Pair<T,float>(elem,p));
    }

    public T Dequeue(){
        int idx = 0; int i =0;
        float minDH = List[0].ElemB;
        T min = List[0].Elem;
        foreach(Pair<T,float> p in List){
            if(minDH > p.ElemB){
                minDH = p.ElemB;
                min = p.Elem;
                idx = i;
            }
            i++;
        }
        List.RemoveAt(idx);
        return min;
    }
    public bool Exist(Predicate<T> pred){
        foreach(Pair<T,float> p in List)
            if( pred(p.Elem)) return true;
        return false;
    }

    public int Find(Predicate<T> pred){
        for(int i=0; i<List.Count; i++){
            Pair<T,float> p = List[i];
            if( pred(p.Elem)) return i;
        }
        return -1;
    }
    public void changePriority(T item, float p){
        int idx = Find(listItem => listItem.Equals(item));
        List[idx].ElemB = p;
    }

    public float FirstPrio()
    {
        int idx = 0; int i =0;
        float minDH = List[0].ElemB;
        T min = List[0].Elem;
        foreach(Pair<T,float> p in List){
            if(minDH > p.ElemB){
                minDH = p.ElemB;
                min = p.Elem;
                idx = i;
            }
            i++;
        }
        return minDH;
    }

    public bool Contains(T elem)
    {
        return Exist(curr => curr.Equals(elem));
    }

}