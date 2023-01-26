using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parameters : MonoBehaviour
{
    public bool debug;
    public bool allowDiagonal;
    public bool lockCam;
    public bool doBidirectionnal;
    public int heuristicMultiplier =1;
    public Heuristics heuristic;
    public OpenSetType listType;
    public bool useSubgoal;
    //public bool useTL;
    
    public IOpenList<Vector2Int> newOpenList(){
        switch(listType){
            case OpenSetType.UnsortedList:
                return new UnsortedList<Vector2Int>();
            case OpenSetType.PriorityQueue:
                return new PriorityQueue<Vector2Int>();
            default:
                return null;
        }
    }
    
}

public enum Heuristics{
    Manhattan,
    Euclidean,
    Chebyshev,
    Octile,
    Djikstra
}

public enum OpenSetType{
    UnsortedList,
    PriorityQueue
    //TODO HotQueue
}