using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// https://www.simplilearn.com/ Graph Implementation
class Graph {

    private int vertNb;
    LinkedList<int>[] neighbors;
    Dictionary<Vector2Int, int> verticesIdx;       

    public Graph (int vNb){
        verticesIdx = new Dictionary<Vector2Int, int>();
        neighbors = new LinkedList<int>[vNb];
        for (int i = 0; i < vertNb; i++) {
            neighbors[i] = new LinkedList<int>();
        }
        vertNb = vNb; 
    }

    public void Add_Edge(Vector2Int v, Vector2Int w) {            
        neighbors[verticesIdx[v]].AddLast(verticesIdx[w]);
        neighbors[verticesIdx[w]].AddLast(verticesIdx[v]);

    }

    public void Remove_Edge(Vector2Int v, Vector2Int w) {            
        neighbors[verticesIdx[v]].Remove(verticesIdx[w]);
        neighbors[verticesIdx[w]].Remove(verticesIdx[v]);

    }

    public int Add_Vertex(Vector2Int p) {            
        // Add head and capacity if capacity copy array and double capacity            
        throw new NotImplementedException();
    }

    public void Remove_Vertex() {
        // Remove the value from all neighbors list + Remove in vertices Idx           
        throw new NotImplementedException();
    }

}