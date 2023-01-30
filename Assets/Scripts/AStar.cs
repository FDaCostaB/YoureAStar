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
    SubGoalGraph sgGraph;
    HPAGraph hpaGraph;

    public AStar(Map m){
        map = m;
        mark(true);
        sgGraph = new SubGoalGraph(map, this, m.Width() * m.Height() < 500000);
        hpaGraph = new HPAGraph(map);
    }

    public void UpdateNodes()
    {
        map.eraseNodes();
        if (Parameters.instance.useSubgoal)
            sgGraph.markNodes();
        else if(Parameters.instance.useHPA)
            hpaGraph.markNodes();

    }

    public void RebuildHPA()
    {
        hpaGraph = new HPAGraph(map);
        UpdateNodes();
        map.Notify();
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
                map.AddNeighborhood(curr.x, curr.y, ~REACHABLE & ~NODES & ~SELECTEDNODES, reachable, true);
                map.setMark(REACHABLE, curr.x, curr.y);
            }
        }

        return;

    }

    public Move measurePath(int toX, int toY){
        int fromX = map.CharacterX();
        int fromY = map.CharacterY();
        CursorController.instance.SetLoading();
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        Move m = path(toX, toY);
        stopwatch.Stop();
        m.time = stopwatch.ElapsedMilliseconds;
        CursorController.instance.SetNormal();
        if (m != null){
            String method;
            if (Parameters.instance.useSubgoal)
            {
                if (sgGraph.isTL) method = "Subgoal";
                else method = "Subgoal-TL";
            }
            else if (Parameters.instance.useHPA) method = "HPA";
            else method = "Grid";
            if (!Parameters.instance.debug)
                Data.CacheLine(map.name, sgGraph.VertexCount, sgGraph.EdgeCount, sgGraph.buildTime,fromX, fromY, toX, toY,  stopwatch.ElapsedMilliseconds, m.scanned, m.openSetMaxSize, m.Steps().Count, method, Parameters.instance.listType, Parameters.instance.heuristic, Parameters.instance.heuristicMultiplier);
        }
        Data.flush();
        return m;
    }

    public Move path(int toX, int toY){
        int newToX = toX;
        int newToY = toY;
        //If not reachable FindClosestFree(toX,toY)
        if (!map.isFree(toX, toY) || map.mark(toX, toY) == AStar.EMPTY)
        {
            Vector2Int newDest = map.closestFree(toX, toY);
            newToX = newDest.x;
            newToY = newDest.y;
        }
        if (Parameters.instance.debug) map.eraseMark();
        if (Parameters.instance.useSubgoal) return sgGraph.path(new Vector2Int(newToX, newToY));
        else if (Parameters.instance.useHPA) return hpaGraph.GetPath(new GridTile(map.CharacterX(), map.CharacterY()), new GridTile(newToX, newToY)) ;
        else return pathGrid(newToX, newToY);
    }

    public Move pathGrid(int toX, int toY){
        IOpenList<Vector2Int> explore = Parameters.instance.newOpenList();
        List<Vector2Int> neighborhood =  new List<Vector2Int>();
        Vector2Int[,] pred = new Vector2Int[map.Width(),map.Height()];
        int fromX = map.CharacterX();
        int fromY = map.CharacterY();
        Vector2Int curr,min= new Vector2Int(-1,-1);
        float [,] dist;

        Move cp = new Move(map, map.currentChar());

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
            float diagDist = Parameters.instance.heuristic == Heuristics.Chebyshev ? 1f : Parameters.instance.heuristic == Heuristics.Manhattan ? 2f : 1.4f;
            float neighborhoodDist;
            foreach(Vector2Int next in neighborhood){
                if(min.x != next.x && min.y != next.y ) neighborhoodDist = diagDist;
                else neighborhoodDist = 1;
                if( dist[min.x,min.y] + neighborhoodDist < dist[next.x, next.y]){
                    dist[next.x, next.y] = dist[min.x,min.y] + neighborhoodDist;
                    pred[next.x, next.y] = min;
                    if(explore.Contains(next))
                    {
                        explore.changePriority(next, dist[min.x, min.y] + map.distHeuristic(next.x,next.y,toX,toY));
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
        IOpenList<Vector2Int> explore = Parameters.instance.newOpenList();
        List<Vector2Int> neighborhood =  new List<Vector2Int>();
        List<Move> res =  new List<Move>();
        Vector2Int[,] pred = new Vector2Int[map.Width(),map.Height()];
        int fromX = sgGraph.CharacterX();
        int fromY = sgGraph.CharacterY();
        Vector2Int curr,min= new Vector2Int(-1,-1);
        float [,] dist;

        //Mark reachable grid cell
        //mark(true);
        Move cp = new Move(map, agentNb);

        //If not reachable FindClosestFree(toX,toY)
        if(!map.isFree(toX,toY) && map.mark(toX, toY) != AStar.EMPTY)
        {
            Vector2Int newDest = map.closestFree(toX, toY);
            toX=newDest.x;
            toY=newDest.y;
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
                    d = new Step(pred[curr.x, curr.y].x, pred[curr.x, curr.y].y,curr.x,curr.y, Map.Octile(pred[curr.x, curr.y].x, pred[curr.x, curr.y].y,curr.x,curr.y));
                    cp.Steps().Insert(0, d);
                    curr = new Vector2Int(pred[curr.x,curr.y].x, pred[curr.x,curr.y].y);
                    map.setMark(PATH,curr.x,curr.y);
                }
                return cp;
            }

            

            //For all neighborhood v update the distance
            neighborhood.Clear();
            
            sgGraph.AddNeighborhood(min.x,min.y,neighborhood, Parameters.instance.useTL);

            foreach(Vector2Int next in neighborhood){
                if( dist[min.x,min.y] + map.distHeuristic(min.x, min.y, next.x,next.y) < dist[next.x, next.y]){
                    dist[next.x, next.y] = dist[min.x,min.y] + map.distHeuristic(min.x, min.y, next.x,next.y);
                    pred[next.x, next.y] = min;
                    if(explore.Contains(next)){
                        explore.changePriority(next, dist[next.x, next.y] + map.distHeuristic(next.x,next.y,toX,toY));
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
        if(Parameters.instance.useTL) sgGraph.markGlobal();
        map.Notify();
    }

    public void debug(int x, int y){
        if (!map.isIn(x, y)) return;
        map.eraseMark();
        if (map.isSubgoal(x, y))
        {
            map.setMark(AStar.SELECTEDNODES, x, y);
            LinkedList<Vector2Int> toDisplay = null;
            if (Parameters.instance.useTL)
                toDisplay = sgGraph.linkedNodesTL(x, y);
            else if (Parameters.instance.useSubgoal)
                toDisplay = sgGraph.Neighborhood(x, y);
            else if(Parameters.instance.useHPA)
                toDisplay = hpaGraph.Neighborhood(x, y);
            if (toDisplay == null) return;
            foreach (Vector2Int p in toDisplay)
            {
                map.setMark(AStar.SELECTEDNODES,p.x,p.y);
            }
        } else {
            if (Parameters.instance.useTL)
                sgGraph.markGlobal();
        }
        map.Notify();
	}

}
