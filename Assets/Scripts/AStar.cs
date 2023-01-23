using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;


public class AStar {
    public const int REACHABLE = 0x00000F;
    public const int SCANNED = 0x0000F0;
    public const int PATH = 0x000F00;
    public const int NODES = 0x00F000;
    public const int SELECTEDNODES = 0x0F0000;
    public const int EMPTY = 0xF00000;
    public const int ALL = 0xFFFFFF;
    
    Map map;
    SubGoalGraph graph;

    public AStar(Map m){
        map = m;
        mark(true);
        graph = new SubGoalGraph(map, this);
    }
    

    //TODO : MARK REACHABLE ONCE AND PLACE WALL ON NON REACHABLE FLOOR
    public void mark(bool doRefresh) {
        int characterX = map.CharacterX();
        int characterY = map.CharacterY();
        Vector2Int curr;
        List<Vector2Int> reachable = new List<Vector2Int>();

        reachable.Add(new Vector2Int(characterX, characterY));

        if(doRefresh){
            map.eraseMark();
            while (reachable.Count > 0) {
                curr = reachable[0];
                reachable.RemoveAt(0);
                map.AddNeighborhood(curr.x, curr.y, ~REACHABLE & ~NODES & ~SELECTEDNODES, reachable, true);
                map.setMark(REACHABLE, curr.x, curr.y);
            }
        }

        return;

    }

    public Move measurePath(int toX, int toY){
        int fromX = map.CharacterX();
        int fromY = map.CharacterY();
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        Move m = path(toX, toY);
        stopwatch.Stop();
        if(m != null){
            String method;
            if (map.parameters.useSubgoal){
                if(graph.isTL) method = "Subgoal";
                else method = "Subgoal-TL";
            } else method = "Grid";
            if (!map.parameters.debug)
                Data.CacheLine(map.name, graph.VertexCount, graph.EdgeCount, graph.buildTime,fromX, fromY, toX, toY,  stopwatch.ElapsedMilliseconds, m.scanned, m.openSetMaxSize, m.Steps().Count, method, map.parameters.listType, map.parameters.heuristic);
        }
        Data.flush();
        return m;
    }

    public Move path(int toX, int toY){
        if(map.parameters.debug) map.eraseMark();
        if(map.parameters.useSubgoal) return graph.path(new Vector2Int(toX, toY));
        else return pathGrid(toX, toY);
    }

    public Move pathGrid(int toX, int toY){
        IOpenList<Vector2Int> explore = map.parameters.newOpenList();
        List<Vector2Int> neighborhood =  new List<Vector2Int>();
        Vector2Int[,] pred = new Vector2Int[map.Width(),map.Height()];
        int fromX = map.CharacterX();
        int fromY = map.CharacterY();
        Vector2Int curr,min= new Vector2Int(-1,-1);
        float [,] dist;

        Move cp = new Move(map, map.currentChar());

        
        //If not reachable skip
        //TODO : FIND CLOSEST FREE GRID CELL
        if(!map.isFree(toX,toY) || map.mark(toX,toY) == EMPTY){
            UnityEngine.Debug.Log("No path exist" );
            return null;
        }

        dist = new float[map.Width(), map.Height()];
        for (int y = 0; y < map.Height(); y++) {
            for (int x = 0; x < map.Width(); x++) {
                dist[x,y] = 1e30f;
            }
        }

        dist[fromX,fromY]=0;
        min=new Vector2Int(fromX,fromY);
        explore.Enqueue(min,0);

        while(explore.size > 0){

            min = explore.Dequeue();
            cp.scanned++;
            map.setMark(SCANNED,min.x,min.y);

            //If destination reached
            if(min.x == toX && min.y == toY){
                curr = new Vector2Int(toX,toY);
                map.setMark(PATH,toX,toY);
                Step d;
                while( curr.x != fromX || curr.y != fromY){
                    d = new Step(pred[curr.x, curr.y].x, pred[curr.x, curr.y].y,curr.x,curr.y);
                    cp.Steps().Insert(0, d);
                    curr = new Vector2Int(pred[curr.x,curr.y].x, pred[curr.x,curr.y].y);
                    map.setMark(PATH,curr.x,curr.y);
                }
                return cp;
            }

            

            //For all neighborhood v update the distance
            neighborhood.Clear();
            map.AddNeighborhood(min.x,min.y,ALL,neighborhood);
            float diagDist = map.parameters.heuristic == Heuristics.Chebyshev ? 1f : map.parameters.heuristic == Heuristics.Manhattan ? 2f : 1.4f;
            float neighborhoodDist;
            foreach(Vector2Int next in neighborhood){
                if(min.x != next.x && min.y != next.y ) neighborhoodDist = diagDist;
                else neighborhoodDist = 1;
                if( dist[min.x,min.y] + neighborhoodDist < dist[next.x, next.y]){
                    dist[next.x, next.y] = dist[min.x,min.y] + neighborhoodDist;
                    pred[next.x, next.y] = min;
                    int idx = explore.Find(p=> p.x == next.x && p.y == next.y);
                    if(idx>=0){
                        explore.changePriority(idx, dist[min.x, min.y] + map.distHeuristic(next.x,next.y,toX,toY));
                    }else {
                        explore.Enqueue(next, dist[min.x, min.y] + map.distHeuristic(next.x,next.y,toX,toY));
                        cp.setOpenSetMaxSize(explore.size);
                    }
                }
            }
            
        }

        UnityEngine.Debug.Log("No path exist" );
        return null;
    }

    public Move pathGraph(int toX, int toY, int agentNb){
        IOpenList<Vector2Int> explore = map.parameters.newOpenList();
        List<Vector2Int> neighborhood =  new List<Vector2Int>();
        List<Move> res =  new List<Move>();
        Vector2Int[,] pred = new Vector2Int[map.Width(),map.Height()];
        int fromX = graph.CharacterX();
        int fromY = graph.CharacterY();
        Vector2Int curr,min= new Vector2Int(-1,-1);
        float [,] dist;

        //Mark reachable grid cell
        //mark(true);
        Move cp = new Move(map, agentNb);

        //If not reachable skip
        //TODO : FIND CLOSEST FREE GRID CELL
        if(!map.isFree(toX,toY)){
            UnityEngine.Debug.Log("No path exist" );
            return null;
        }


        dist = new float[map.Width(), map.Height()];
        for (int y = 0; y < map.Height(); y++) {
            for (int x = 0; x < map.Width(); x++) {
                dist[x,y] = 1e30f;
            }
        }

        dist[fromX,fromY]=0;
        min=new Vector2Int(fromX,fromY);
        explore.Enqueue(min,0);

        while(explore.size > 0){

            min = explore.Dequeue();
            cp.scanned++;
            map.setMark(SCANNED,min.x,min.y);

            //If destination reached
            if(min.x == toX && min.y == toY){
                curr = new Vector2Int(toX,toY);
                map.setMark(PATH,toX,toY);
                Step d;
                while( curr.x != fromX || curr.y != fromY){
                    d = new Step(pred[curr.x, curr.y].x, pred[curr.x, curr.y].y,curr.x,curr.y);
                    cp.Steps().Insert(0, d);
                    curr = new Vector2Int(pred[curr.x,curr.y].x, pred[curr.x,curr.y].y);
                    map.setMark(PATH,curr.x,curr.y);
                }
                return cp;
            }

            

            //For all neighborhood v update the distance
            neighborhood.Clear();
            
            graph.AddNeighborhood(min.x,min.y,ALL,neighborhood);

            foreach(Vector2Int next in neighborhood){
                if( dist[min.x,min.y] + map.distHeuristic(min.x, min.y, next.x,next.y) < dist[next.x, next.y]){
                    dist[next.x, next.y] = dist[min.x,min.y] + map.distHeuristic(min.x, min.y, next.x,next.y);
                    pred[next.x, next.y] = min;
                    int idx = explore.Find(p=> p.x == next.x && p.y == next.y);
                    if(idx>=0){
                        explore.changePriority(idx, dist[next.x, next.y] + map.distHeuristic(next.x,next.y,toX,toY));
                    }else {
                        explore.Enqueue(next, dist[next.x, next.y] + map.distHeuristic(next.x,next.y,toX,toY));
                        cp.setOpenSetMaxSize(explore.size);
                    }
                }
            }
            
        }

        UnityEngine.Debug.Log("No path exist" );
        return null;
    }

    public void debug(){
        UnityEngine.Debug.Log("Debug function called !");
	}

    public void debug(int x, int y){
        map.eraseMark();
        //graph.TryDirectPath(new Vector2Int(x,y));
        //graph.FindHreachablePath(new Move(map, map.currentChar()),map.currentCharPos().x, map.currentCharPos().y, x,y);
        map.Notify();
        //throw new NotImplementedException();
	}

}
