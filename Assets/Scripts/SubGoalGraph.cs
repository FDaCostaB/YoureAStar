using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubGoalGraph {
    Map map;
    Graph graph;
    public SubGoalGraph(Map m){
        map=m;
        List<Vector2Int> vertices = new List<Vector2Int>();

        //Compute nodes
        for(int x =0; x < map.Width(); x++){
            for(int y=0; y < map.Height(); y++){
                if(map.isFree(x,y)){

                    if( map.isWall(Map.moveX(x,Directions.EAST), Map.moveY(y,Directions.NORTH)) ){
                        if( map.isFree(x,y,Directions.EAST) && map.isFree(x,y,Directions.NORTH) ){
                            if(map.isIn(x,y)) vertices.Add(new Vector2Int(x,y));
                        }
                    }

                    if( map.isWall(Map.moveX(x,Directions.EAST), Map.moveY(y,Directions.SOUTH)) ){
                        if( map.isFree(x,y,Directions.EAST) && map.isFree(x,y,Directions.SOUTH) ){
                            if(map.isIn(x,y)) vertices.Add(new Vector2Int(x,y));
                        }
                    }

                    if( map.isWall(Map.moveX(x,Directions.WEST), Map.moveY(y,Directions.SOUTH)) ){
                        if( map.isFree(x,y,Directions.WEST) && map.isFree(x,y,Directions.SOUTH) ){
                            if(map.isIn(x,y)) vertices.Add(new Vector2Int(x,y));
                        }
                    }

                    if( map.isWall(Map.moveX(x,Directions.WEST), Map.moveY(y,Directions.NORTH)) ){
                        if( map.isFree(x,y,Directions.WEST) && map.isFree(x,y,Directions.NORTH) ){
                            if(map.isIn(x,y)) vertices.Add(new Vector2Int(x,y));
                        }
                    }
                }
            }
        }

        Debug.Log("Nodes nb : " + vertices.Count);
        foreach(Vector2Int p in vertices){
            map.setMark(AStar.NODES,p.x,p.y);
        }
        map.Notify();

        //graph = new Graph(vertices.Count);
        //TODO 2 : CONNECT VERTICES TOGETHER

    }

    // TODO 1 : GetDirectHReachable(cell s) + Clearance => Algo 3


}
