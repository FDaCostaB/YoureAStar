using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parameters : MonoBehaviour
{
    public static Parameters instance;

    public bool debug;
    public bool lockCam = false ;
    public bool doBidirectionnal = false;
    public bool skipHreachable = false;
    public bool benchmark = false;
    public bool pause = false;
    public int heuristicMultiplier =1;
    public Heuristics heuristic;
    public OpenSetType listType;
    public bool useSubgoal;
    public bool useTL;
    public bool useHPA;
    public int clusterSize = 50;
    public int layerNb = 1;
    private void Awake()
    {
        instance = this;
    }

    public IOpenList<Vector2Int> newOpenList(){
        switch(listType){
            case OpenSetType.UnsortedList:
                return new UnsortedList<Vector2Int>();
            case OpenSetType.PriorityQueue:
                return new PriorityQueue<Vector2Int>();
            case OpenSetType.HotQueue:
                return new HOTBucketOpenSet<Vector2Int>();
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
    PriorityQueue,
    HotQueue
}