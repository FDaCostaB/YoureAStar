using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class Graph {

    Dictionary<Vector2Int, LinkedList<Vector2Int>> neighbors;
    int currIdx;
    public int VertexCount{get {return neighbors.Count;}}     
    public int EdgeCount{get {return neighbors.Aggregate(0, (acc,next) => acc +next.Value.Count, res => res/2);}}     

    public Graph (){
        currIdx=0;
        neighbors = new Dictionary<Vector2Int, LinkedList<Vector2Int>>();
    }

    public List<Vector2Int> getNodes(){
        return neighbors.Keys.ToList();
    }

    public void AddEdge(Vector2Int v, Vector2Int w) {            
        if(v.x != w.x || v.y != w.y){
            if(!neighbors[v].Contains(w)) neighbors[v].AddFirst(w);
            if(!neighbors[w].Contains(v)) neighbors[w].AddFirst(v);
        }

    }

    public void RemoveEdge(Vector2Int v, Vector2Int w) {
        if(neighbors[v].Contains(w)) neighbors[v].Remove(w);
        if(neighbors[w].Contains(v)) neighbors[w].Remove(v);          
    }

    public int AddVertex(Vector2Int p) {       
        if(!neighbors.Keys.Contains(p)) neighbors[p] = new LinkedList<Vector2Int>();
        return currIdx++;
    }

    public void RemoveVertex(Vector2Int p) {
        foreach(Vector2Int neighbor in neighbors[p]){
            if(neighbor.x != p.x || neighbor.y != p.y) neighbors[neighbor].Remove(p);
        }
        neighbors[p].Clear();
        neighbors.Remove(p);
    }

    public bool Contains(Vector2Int cell){
        return neighbors.Keys.Contains(cell);
    }

    public LinkedList<Vector2Int> neighborhood(Vector2Int cell){
        return neighbors[cell];
    }

}