using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class Graph {

    Dictionary<Vector2Int, LinkedList<Vector2Int>> neighbors;
    int currIdx;
    public int Count{get {return neighbors.Keys.Count;}}     

    public Graph (){
        currIdx=0;
        neighbors = new Dictionary<Vector2Int, LinkedList<Vector2Int>>();
    }

    public List<Vector2Int> getNodes(){
        return neighbors.Keys.ToList();
    }

    public void AddEdge(Vector2Int v, Vector2Int w) {            
        if(!neighbors[v].Contains(w)){
            neighbors[v].AddFirst(w);
            //Debug.Log("Edge added : " + v.x +" ,"+v.y + " <==> "+ w.x +" ,"+w.y  + " Size of neighborhood : " + neighbors[v].Count);
        }
        if(!neighbors[w].Contains(v)){
            neighbors[w].AddFirst(v);
            //Debug.Log("Edge added : " + w.x +" ,"+ w.y + " <==> "+ v.x +" ," + v.y + " Size of neighborhood : " + neighbors[w].Count );
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
            neighbors[neighbor].Remove(p);
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