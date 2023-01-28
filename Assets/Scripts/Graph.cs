using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class Graph {

    Dictionary<Vector2Int, LinkedList<Vector2Int>> neighbors;
    HashSet<Vector2Int> vertex;
    int currIdx;
    public int VertexCount{get {return neighbors.Count;}}     
    public int EdgeCount{get {return neighbors.Aggregate(0, (acc,next) => acc +next.Value.Count, res => res/2);}}     
    public int RawEdgeCount{get {return neighbors.Aggregate(0, (acc,next) => acc +next.Value.Count);}}     

    public Graph (){
        currIdx=0;
        neighbors = new Dictionary<Vector2Int, LinkedList<Vector2Int>>();
        vertex = new HashSet<Vector2Int>();
    }

    public List<Vector2Int> getNodes(){
        return vertex.ToList();
    }

    public void AddEdge(Vector2Int v, Vector2Int w) {
        if(!neighbors.ContainsKey(w)) neighbors[w] = new LinkedList<Vector2Int>();
        if (!neighbors.ContainsKey(v)) neighbors[v] = new LinkedList<Vector2Int>();
        if (v.x != w.x || v.y != w.y){
            if (!neighbors[v].Contains(w)) neighbors[v].AddFirst(w);
            if (!neighbors[w].Contains(v)) neighbors[w].AddFirst(v);
        }

    }

    public void RemoveEdge(Vector2Int v, Vector2Int w) {
        if (neighbors.ContainsKey(v) && neighbors[v].Contains(w)) neighbors[v].Remove(w);
        if(neighbors.ContainsKey(w) && neighbors[w].Contains(v)) neighbors[w].Remove(v);          
    }

    public int AddVertex(Vector2Int p) {       
        if(!neighbors.Keys.Contains(p)) neighbors[p] = new LinkedList<Vector2Int>();
        vertex.Add(p);
        return currIdx++;
    }

    public void RemoveVertex(Vector2Int p) {
        vertex.Remove(p);
    }

    public bool Contains(Vector2Int cell){
        return vertex.Contains(cell);
    }

    public LinkedList<Vector2Int> neighborhood(Vector2Int cell){
        if (!vertex.Contains(cell)) throw new ArgumentException("cell "+cell+" not in graph");
        return neighbors[cell];
    }

    public LinkedList<Vector2Int> linkedNodes(Vector2Int cell)
    {
        return neighbors[cell];
    }
}