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
    public const int EMPTY = 0xF00000;
    public const int ALL = 0xFFFFFF;
    Map map;

    public AStar(Map m){
        map = m;
    }
    
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
                AddNeighborhood(curr.x, curr.y, ~REACHABLE, reachable, true);
                map.setMark(REACHABLE, curr.x, curr.y);
            }
        }

        return;

    }

    List<Vector2Int> AddNeighborhood(int x, int y, int color, List<Vector2Int> res, bool doMark = false){
        byte neighborhood = 0x0;
        for(int i=0; i<8;i++){
            Directions d = (Directions)(1<<i);
            neighborhood |=  (byte) ((map.isFree(x,y,d) && ((map.mark(x,y,d) & color) != 0) )  ? 0x1 << i : 0x0);
        }
        for(int i=0; i<8;i++){
            Directions d = (Directions)(1<<i);
            if( (neighborhood | MaskFree.maskFree[i]) == 0xFF){
                if(map.parameters.allowDiagonal || d == Directions.NORTH || d == Directions.EAST || d==Directions.SOUTH || d==Directions.WEST ){
                    res.Add(new Vector2Int(Map.moveX(x,d), Map.moveY(y,d)));
                    if(doMark) map.setMark(REACHABLE, Map.moveX(x,d), Map.moveY(y,d));
                }
            }
        }
        return res;
    }

    public Move measurePath(int toX, int toY, int charNb){
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        Move m = path(toX,toY, charNb);
        stopwatch.Stop();
        TimeSpan stopwatchElapsed = stopwatch.Elapsed;
        if(m != null){
            UnityEngine.Debug.Log("Time taken : "+ stopwatchElapsed.TotalMilliseconds);
            UnityEngine.Debug.Log("Path Length : "+ m.Steps().Count);
            UnityEngine.Debug.Log("Tile scanned : " + m.scanned);
        }
        return m;
    }

    public Move path(int toX, int toY, int charNb){
        IOpenList<Vector2Int> explore = map.parameters.newOpenList();
        List<Vector2Int> neighborhood =  new List<Vector2Int>();
        List<Move> res =  new List<Move>();
        Vector2Int[,] pred = new Vector2Int[map.Width(),map.Height()];
        int fromX = map.CharacterX();
        int fromY = map.CharacterY();
        Vector2Int curr,min= new Vector2Int(-1,-1);
        float [,] dist;

        //Mark reachable grid cell
        //mark(true);
        Move cp = new Move(map, charNb);

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
            AddNeighborhood(min.x,min.y,ALL,neighborhood);
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
                        explore.changePriority(idx, distHeuristic(dist,next.x,next.y,toX,toY));
                    }else {
                        explore.Enqueue(next, distHeuristic(dist,next.x,next.y,toX,toY));
                    }
                }
            }
            
        }

        UnityEngine.Debug.Log("No path exist" );
        return null;
    }

    float distHeuristic(float [,]dist, int pt1x, int pt1y, int pt2x, int pt2y){
        //dist is g i.e. the cost from the start to pt1 the rest is the heuristic h
        int dx = Mathf.Abs(pt2x - pt1x);
        int dy = Mathf.Abs(pt2y - pt1y);
        float d2 = map.parameters.heuristic == Heuristics.Chebyshev ? 1f : map.parameters.heuristic == Heuristics.Manhattan ? 2f : 1.4f;

        switch(map.parameters.heuristic){
            case Heuristics.Manhattan:
                return dx + dy + dist[pt1x,pt1y]; 
            case Heuristics.Euclidean:
                return Mathf.Sqrt(dx+dy);
            case Heuristics.Chebyshev:
            case Heuristics.Octile:
                return (dx + dy) + (d2 - 2) * (dx > dy? dy : dx);
            case Heuristics.Djikstra:
                return dist[pt1x,pt1y]+d2;
            default:
                return 1;
        }

    }

}
