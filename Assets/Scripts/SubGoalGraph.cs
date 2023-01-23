using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class SubGoalGraph {
    Map map;
    Graph sgGraph;
    Graph sgTLGraph;

    AStar astar;
    public bool isTL;
    public int buildTime {get;}
    public int VertexCount{get {return sgGraph.VertexCount;}}
    public int EdgeCount{get {return sgGraph.EdgeCount;}}

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

        sgGraph = new Graph();
        sgTLGraph = new Graph();
        foreach(Vector2Int p in vertices){
            map.setMark(AStar.NODES,p.x,p.y);
            sgGraph.AddVertex(p);
            sgTLGraph.AddVertex(p);
        }

        foreach(Vector2Int cell in sgGraph.getNodes()){
            foreach(Vector2Int cellPrime in GetDirectHReachable(cell)){
                sgGraph.AddEdge(cell,cellPrime);
                sgTLGraph.AddEdge(cell,cellPrime);
            }
        }

        stopwatch.Stop();
        buildTime = stopwatch.Elapsed.Milliseconds;
        UnityEngine.Debug.Log("# of nodes : "+ vertices.Count);
        UnityEngine.Debug.Log("# of edges : "+ sgGraph.EdgeCount);

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
        if(!sgGraph.Contains(cell)){
            sgGraph.AddVertex(cell);
            foreach(Vector2Int cellPrime in GetDirectHReachable(cell)){
                sgGraph.AddEdge(cell,cellPrime);
            }
        }
    }
    
    Move FindAbstractPath(Vector2Int goal){
        Vector2Int start = map.currentCharPos();
        ConnectToGraph(start);
        ConnectToGraph(goal);
        Move res = astar.pathGraph(goal.x, goal.y, map.currentChar() );
        sgGraph.RemoveVertex(start);
        sgGraph.RemoveVertex(goal);
        return res;
    }

    public Move measureFindAbstractPath(Vector2Int goal){
        Stopwatch timer = new Stopwatch();
        timer.Start();
        Move m = FindAbstractPath(goal);
        timer.Stop();
        if(m != null && !map.parameters.debug){
            Data.CacheLine(map.name, VertexCount, EdgeCount, buildTime, map.CharacterX(), map.CharacterY(), goal.x, goal.y, timer.ElapsedMilliseconds, m.scanned, m.openSetMaxSize, m.Steps().Count, "FindAbstractPath - " + (isTL? "SG-TL Graph": "SG Graph"), map.parameters.listType, map.parameters.heuristic);
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

    public bool isHreachable(int fromX, int fromY, int toX, int toY){
        IOpenList<Vector2Int> explore = map.parameters.newOpenList();
        List<Vector2Int> neighborhood =  new List<Vector2Int>();
        Vector2Int[,] pred = new Vector2Int[map.Width(),map.Height()];
        Vector2Int min=new Vector2Int(fromX,fromY);
        float h = Map.Octile(fromX, fromY, toX, toY);

        explore.Enqueue(min,h);
        
        while(explore.size > 0){

            min = explore.Dequeue();
            map.setMark(AStar.SCANNED,min.x,min.y);

            //If destination reached
            if(min.x == toX && min.y == toY){
                return true;
            }

            neighborhood.Clear();

            foreach(Vector2Int next in map.AddNeighborhood(min.x,min.y,AStar.ALL,neighborhood)){
                if(!explore.Exist(p=> p.x == next.x && p.y == next.y) && Map.Octile(min.x,min.y, toX,toY) > Map.Octile(next.x,next.y, toX,toY) ){
                    pred[next.x, next.y] = min;
                    explore.Enqueue(next, Map.Octile(next.x, next.y, toX, toY));
                }
            }
        }

        return false;
    }

    public float costOtherPath(Vector2Int node, Vector2Int s, Vector2Int sprime){

        sgTLGraph.AddVertex(s);
        sgTLGraph.AddVertex(sprime);

        IOpenList<Vector2Int> explore = map.parameters.newOpenList();
        List<Vector2Int> neighborhood =  new List<Vector2Int>();
        List<Move> res =  new List<Move>();
        Vector2Int[,] pred = new Vector2Int[map.Width(),map.Height()];
        int fromX = s.x;
        int fromY = s.y;
        int toX = sprime.x;
        int toY = sprime.y;
        Vector2Int min= new Vector2Int(-1,-1);
        float [,] dist;

        if(node.x == s.x && node.y == s.y)
            throw new InvalidOperationException("Invalid state");

        Move cp = new Move(map, -1);

        dist = new float[map.Width(), map.Height()];
        for (int y = 0; y < map.Height(); y++) {
            for (int x = 0; x < map.Width(); x++) {
                dist[x,y] = 1e30f;
            }
        }

        dist[fromX,fromY] = 0;
        min = new Vector2Int(fromX,fromY);
        explore.Enqueue(min,0);

        while(explore.size > 0){

            min = explore.Dequeue();

            //If destination reached
            if(min.x == toX && min.y == toY){
                sgTLGraph.RemoveVertex(s);
                sgTLGraph.RemoveVertex(sprime);
                return dist[toX, toY];
            }

            

            //For all neighborhood v update the distance
            neighborhood.Clear();
            AddNeighborhood(min.x, min.y, AStar.ALL, neighborhood);

            foreach(Vector2Int next in neighborhood){
                if(next.x == node.x && next.y == node.y) continue;
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

        sgTLGraph.RemoveVertex(s);
        sgTLGraph.RemoveVertex(sprime);
        UnityEngine.Debug.Log("No path exist" );
        return float.MaxValue;
    }

    bool IsNecessaryToConnect(Vector2Int node, Vector2Int s, Vector2Int sprime){
        if(isHreachable(s.x, s.y, sprime.x, sprime.y))
            return false;
        if(costOtherPath(node,s,sprime) <= Map.Octile(node.x, node.y, s.x , s.y) + Map.Octile(node.x, node.y, sprime.x , sprime.y))
            return false;
        return true;
    }

    void PruneSubgoal(Vector2Int node){
        foreach(Vector2Int s in sgTLGraph.neighborhood(node)){
            foreach(Vector2Int sprime in sgTLGraph.neighborhood(node)){
                if(Map.Octile(s.x, s.y, sprime.x, sprime.y) == Map.Octile(node.x, node.y, s.x, s.y) + Map.Octile(node.x, node.y, sprime.x, sprime.y) ){
                    if(costOtherPath(node,s,sprime) > Map.Octile(node.x, node.y, s.x, s.y) + Map.Octile(node.x, node.y, sprime.x, sprime.y))
                        sgTLGraph.AddEdge(s,sprime);
                }
            }
        }
        sgTLGraph.RemoveVertex(node);
    }

    public void computeTL(){
        isTL = true;
        bool necessary = false;
        foreach(Vector2Int node in sgTLGraph.getNodes()){
            necessary = false;
            foreach(Vector2Int s in sgTLGraph.neighborhood(node)){
                foreach(Vector2Int sprime in sgTLGraph.neighborhood(node)){
                    if(IsNecessaryToConnect(node,s,sprime)){
                        necessary = true;
                        break;
                    }
                }
                if(necessary)break;
            }
            if(!necessary)PruneSubgoal(node);
        }
             
    }

    public int CharacterX(){
        return map.CharacterX();
    }

    public int CharacterY(){
        return map.CharacterY();
    }

    internal void AddNeighborhood(int x, int y, int aLL, List<Vector2Int> neighborhood )
    {
        Vector2Int cell = new Vector2Int(x,y);
        if(sgGraph.Contains(cell)){
            foreach(Vector2Int p in sgGraph.neighborhood(cell))
                neighborhood.Add(p);
        }
        if(isTL){
            if(sgTLGraph.Contains(cell)){
                foreach(Vector2Int p in sgTLGraph.neighborhood(cell))
                    neighborhood.Add(p);
            }
        }
    }
}
