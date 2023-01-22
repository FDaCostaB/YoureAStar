using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class SubGoalGraph {
    Map map;
    Graph graph;

    AStar astar;
    public bool isTL;
    public int buildTime {get;}
    public int VertexCount{get {return graph.VertexCount;}}
    public int EdgeCount{get {return graph.EdgeCount;}}

    Directions[] dirs = {Directions.NORTHWEST,Directions.NORTH,Directions.NORTHEAST,Directions.WEST,Directions.EAST,Directions.SOUTHWEST,Directions.SOUTH,Directions.SOUTHEAST};
    Directions[] diag = {Directions.NORTHWEST, Directions.NORTHEAST, Directions.SOUTHWEST, Directions.SOUTHEAST};
    
    public SubGoalGraph(Map m, AStar pathFinder){
        isTL=false;
        map=m;
        astar=pathFinder;
        List<Vector2Int> vertices = new List<Vector2Int>();

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        //Compute nodes
        for(int x =0; x < map.Width(); x++){
            for(int y=0; y < map.Height(); y++){
                if(map.isFree(x,y)){

                    if( !map.isFree(Map.moveX(x,Directions.EAST), Map.moveY(y,Directions.NORTH)) ){
                        if( map.isFree(x,y,Directions.EAST) && map.isFree(x,y,Directions.NORTH) ){
                            if(map.isIn(x,y)) vertices.Add(new Vector2Int(x,y));
                        }
                    }

                    if( !map.isFree(Map.moveX(x,Directions.EAST), Map.moveY(y,Directions.SOUTH)) ){
                        if( map.isFree(x,y,Directions.EAST) && map.isFree(x,y,Directions.SOUTH) ){
                            if(map.isIn(x,y)) vertices.Add(new Vector2Int(x,y));
                        }
                    }

                    if( !map.isFree(Map.moveX(x,Directions.WEST), Map.moveY(y,Directions.SOUTH)) ){
                        if( map.isFree(x,y,Directions.WEST) && map.isFree(x,y,Directions.SOUTH) ){
                            if(map.isIn(x,y)) vertices.Add(new Vector2Int(x,y));
                        }
                    }

                    if( !map.isFree(Map.moveX(x,Directions.WEST), Map.moveY(y,Directions.NORTH)) ){
                        if( map.isFree(x,y,Directions.WEST) && map.isFree(x,y,Directions.NORTH) ){
                            if(map.isIn(x,y)) vertices.Add(new Vector2Int(x,y));
                        }
                    }
                }
            }
        }

        graph = new Graph();
        // Debug.Log("Nodes nb : " + vertices.Count);
        foreach(Vector2Int p in vertices){
            map.setMark(AStar.NODES,p.x,p.y);
            graph.AddVertex(p);
        }

        foreach(Vector2Int cell in graph.getNodes()){
            foreach(Vector2Int cellPrime in GetDirectHReachable(cell)){
                graph.AddEdge(cell,cellPrime);
            }
        }

        stopwatch.Stop();
        buildTime = stopwatch.Elapsed.Milliseconds;
        UnityEngine.Debug.Log("# of nodes : "+ vertices.Count);
        UnityEngine.Debug.Log("# of edges : "+ graph.EdgeCount);

    }

    public List<Vector2Int> GetDirectHReachable(Vector2Int cell){
        List<Vector2Int> hReachable = new List<Vector2Int>();

        //if(map.parameters.debug) map.eraseMark();

        foreach(Directions d in dirs){
            if(map.isSubgoal(Map.move(cell, d, Clearance(cell,d)))){
                hReachable.Add(Map.move(cell, d, Clearance(cell,d)));
                //if(map.parameters.debug)map.setMark(AStar.SELECTEDNODES,Map.move(cell, d, Clearance(cell,d)).x, Map.move(cell, d, Clearance(cell,d)).y);
            }
        }
        foreach(Directions d in diag){
            foreach(Directions c in cardinalAssociated(d)){
                int max = Clearance(cell,c);
                int diag = Clearance(cell,d);
                if(map.isSubgoal(Map.move(cell, c, max)))
                    max--;
                if(map.isSubgoal(Map.move(cell, d, diag)))
                    diag--;    
                for(int i =1; i<=diag; i++){
                    int j = Clearance(Map.move(cell, d,i),c);
                    //if(map.parameters.debug) for(int k = 0; k<j && k<max ;k++)
                    //    map.setMark(AStar.SCANNED,Map.move(Map.move(cell.x, cell.y,d,i),c,k).x, Map.move(Map.move(cell.x, cell.y,d,i),c,k).y);
                    if(j<=max && map.isSubgoal(Map.move(Map.move(cell.x, cell.y,d,i),c,j))){
                        hReachable.Add(Map.move(Map.move(cell.x, cell.y,d,i),c,j));
                        //if(map.parameters.debug) 
                        //    map.setMark(AStar.SELECTEDNODES,Map.move(Map.move(cell.x, cell.y,d,i),c,j).x,Map.move(Map.move(cell.x, cell.y,d,i),c,j).y);
                        j--;
                    }
                    if(j<max) max = j;
                }
            }
        }
        //Debug.Log("# hReachable nodes : "+hReachable.Count);
        return hReachable;
    }
    
    public Directions[] cardinalAssociated(Directions d){
        if(d == Directions.NORTH || d == Directions.WEST || d == Directions.EAST || d == Directions.SOUTH) throw new InvalidOperationException();
        Directions[] res = new Directions[2];
        switch(d){
            case Directions.NORTHWEST:
                res[0] = Directions.NORTH;
                res[1] = Directions.WEST;
                break;
            case Directions.SOUTHWEST:
                res[0] = Directions.SOUTH;
                res[1] = Directions.WEST;
                break;
            case Directions.NORTHEAST:
                res[0] = Directions.NORTH;
                res[1] = Directions.EAST;
                break;
            case Directions.SOUTHEAST:
                res[0] = Directions.SOUTH;
                res[1] = Directions.EAST;
                break;
        }
        return res;
    }
    public int Clearance(Vector2Int cell, Directions d){
        int i =0;
        while(true){
            if(!map.isFree(Map.move(cell,d,i+1))) return i;
            i++;
            if(map.isSubgoal(Map.move(cell,d,i))) return i;
        }
    }

    public Move path(Vector2Int goal){
        Move cp = TryDirectPath(goal);
        if(cp.Steps().Count > 0){
            return cp;
        }
        int fromX = CharacterX();
        int fromY = CharacterY();

        Move globalPath = measureFindAbstractPath(goal);
        globalPath.scanned += cp.scanned;

        if(globalPath.Steps().Count == 0) return null;

        //TODO : Parallelize this for
        for(int i =0; i < globalPath.Steps().Count; i++){
            FindHreachablePath(cp, globalPath.Steps()[i].fromX, globalPath.Steps()[i].fromY, globalPath.Steps()[i].toX, globalPath.Steps()[i].toY);
        }
        return cp;
    }

    public Move TryDirectPath(Vector2Int goal){
        Move res = new Move(map, map.currentChar());
        Vector2Int from = map.currentCharPos();
        int N = diagonalDistance(map.currentCharPos(), goal);
        for (int step = 0; step <= N; step++) {
            
            float t = N == 0 ? 0.0f : (float) step / N;
            Vector2Int point = roundPoint(lerpPoint(map.currentCharPos(), goal, t));
            if(map.isFree(point)) {
                if(map.parameters.debug) map.setMark(AStar.SCANNED, point.x, point.y);
                if(from.x != point.x && from.y != point.y){
                    if(!map.isFree(point.x,from.y) || !map.isFree(from.x,point.y)){
                        res.Steps().Clear();
                        return res;
                    }
                }
                res.deplace(from.x, from.y, point.x, point.y);
                res.scanned++;
                from = point;
            } else {
                res.Steps().Clear();
                return res;
            }
        }
        return res;
    }

    int diagonalDistance(Vector2Int p0, Vector2Int p1) {
        int dx = p1.x - p0.x, dy = p1.y - p0.y;
        return Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));
    }

    Vector2Int roundPoint(Vector2 p) {
        return new Vector2Int((int) Mathf.Round(p.x), (int) Mathf.Round(p.y));
    }

    Vector2 lerpPoint(Vector2Int p0, Vector2Int p1, float t) {
        return new Vector2(Mathf.Lerp(p0.x, p1.x, t), Mathf.Lerp(p0.y, p1.y, t));
    }

    void ConnectToGraph(Vector2Int cell){
        if(!graph.Contains(cell)){
            graph.AddVertex(cell);
            foreach(Vector2Int cellPrime in GetDirectHReachable(cell)){
                graph.AddEdge(cell,cellPrime);
            }
        }
    }
    
    Move FindAbstractPath(Vector2Int goal){
        Vector2Int start = map.currentCharPos();
        ConnectToGraph(start);
        ConnectToGraph(goal);
        Move res = astar.pathGraph(goal.x, goal.y, map.currentChar() );
        graph.RemoveVertex(start);
        graph.RemoveVertex(goal);
        return res;
    }

    public Move measureFindAbstractPath(Vector2Int goal){
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        Move m = FindAbstractPath(goal);
        stopwatch.Stop();
        if(m != null && !map.parameters.debug){
            Data.WriteLine(map.name, VertexCount, EdgeCount, buildTime, map.CharacterX(), map.CharacterY(), goal.x, goal.y, stopwatch.Elapsed.Milliseconds, m.scanned, m.openSetMaxSize, m.Steps().Count, "FindAbstractPath - " + (isTL? "SG-TL Graph": "SG Graph"), map.parameters.listType, map.parameters.heuristic);
        }
        return m;
    }

    public Move FindHreachablePath(Move cp, int fromX, int fromY, int toX, int toY){
        IOpenList<Vector2Int> explore = map.parameters.newOpenList();
        List<Vector2Int> neighborhood =  new List<Vector2Int>();
        Vector2Int[,] pred = new Vector2Int[map.Width(),map.Height()];
        Vector2Int curr;

        float h = Map.Octile(fromX, fromY, toX, toY);
        Vector2Int min=new Vector2Int(fromX,fromY);
        explore.Enqueue(min,h);
        int insertIdx = cp.Steps().Count;
        
        while(explore.size > 0){

            min = explore.Dequeue();
            map.setMark(AStar.SCANNED,min.x,min.y);
            cp.scanned++;

            //If destination reached
            if(min.x == toX && min.y == toY){
                curr = new Vector2Int(toX,toY);
                if(map.parameters.debug)map.setMark(AStar.PATH,curr.x,curr.y);
                Step d;
                while(curr.x != fromX || curr.y != fromY){
                    d = new Step(pred[curr.x, curr.y].x, pred[curr.x, curr.y].y,curr.x,curr.y);
                    cp.Steps().Insert(insertIdx, d);
                    curr = new Vector2Int(pred[curr.x,curr.y].x, pred[curr.x,curr.y].y);
                    if(map.parameters.debug)map.setMark(AStar.PATH,curr.x,curr.y);
                }
                return cp;
            }

            

            //For all neighborhood v update the distance
            neighborhood.Clear();
            map.AddNeighborhood(min.x,min.y,AStar.ALL,neighborhood);

            foreach(Vector2Int next in neighborhood){
                if(!explore.Exist(p=> p.x == next.x && p.y == next.y) && Map.Octile(min.x,min.y, toX,toY) > Map.Octile(next.x,next.y, toX,toY) ){
                    pred[next.x, next.y] = min;
                    explore.Enqueue(next, Map.Octile(next.x, next.y, toX, toY));
                    cp.setOpenSetMaxSize(explore.size);
                }
            }
        }

        UnityEngine.Debug.Log("Not hReachable" );
        return null;
    }

    //TODO
    public void computeTL(){
        throw new NotImplementedException();
    }

    public int CharacterX(){
        return map.CharacterX();
    }

    public int CharacterY(){
        return map.CharacterY();
    }

    internal void AddNeighborhood(int x, int y, int aLL, List<Vector2Int> neighborhood)
    {
        Vector2Int cell = new Vector2Int(x,y);
        if(graph.Contains(cell)){
            foreach(Vector2Int p in graph.neighborhood(cell))
                neighborhood.Add(p);
        }
    }
}
