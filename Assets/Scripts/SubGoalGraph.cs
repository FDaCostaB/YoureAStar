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
    public int VertexCountTL{get {return sgTLGraph.VertexCount;}}
    public int EdgeCount{get {return sgGraph.EdgeCount;}}
    public int EdgeCountTL{get {return sgTLGraph.EdgeCount;}}

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
                //TODO
                //REAL PROBLEM : MAP SIZE < 16k but 1M tiles scanned => 
                //eraseMark and Mark as scanned scanned tiles and add only h(min,goal) - neigbor dist(min,next) == h(next,goal) instead of h(min,goal) > h(next,goal)
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

    public bool isHreachable(Vector2Int start, Vector2Int goal){
        IOpenList<Vector2Int> explore = map.parameters.newOpenList();
        List<Vector2Int> neighborhood =  new List<Vector2Int>();
        Vector2Int min=start;
        float h = Map.Octile(start.x, start.y, goal.x, goal.y);

        explore.Enqueue(min,h);
        
        int MAX_ITER = 1000000;
        while(explore.size > 0 && MAX_ITER > 0){

            min = explore.Dequeue();

            //If destination reached
            if(min.x == goal.x && min.y == goal.y){
                return true;
            }

            neighborhood.Clear();

            foreach(Vector2Int next in map.AddNeighborhood(min.x,min.y,AStar.ALL,neighborhood)){
            //TODO
            //REAL PROBLEM : MAP SIZE < 16k but 1M tiles scanned => 
            //eraseMark and Mark as scanned scanned tiles and add only h(min,goal) - neigbor dist(min,next) == h(next,goal) instead of h(min,goal) > h(next,goal)
                if(!explore.Exist(p=> p.x == next.x && p.y == next.y) && Map.Octile(min.x,min.y, goal.x, goal.y) > Map.Octile(next.x,next.y, goal.x, goal.y) ){
                    explore.Enqueue(next, Map.Octile(next.x, next.y, goal.x, goal.y));
                }
            }
            MAX_ITER--;
        }
        if(MAX_ITER==0) UnityEngine.Debug.LogError("Problem in isHreachable\nStart : ("+ start.x +", "+ start.y+")" + "("+ goal.x +", "+ goal.y+")");
        return false;
    }

    public float costOtherPath(Vector2Int node, Vector2Int s, Vector2Int sprime){
        
        bool sExist = sgTLGraph.Contains(s); 
        bool sprimeExist = sgTLGraph.Contains(sprime); 
        if(!sExist) sgTLGraph.AddVertex(s);
        if(!sprimeExist) sgTLGraph.AddVertex(sprime);

        IOpenList<Vector2Int> explore = map.parameters.newOpenList();
        List<Vector2Int> neighborhood =  new List<Vector2Int>();
        List<Move> res =  new List<Move>();
        Vector2Int[,] pred = new Vector2Int[map.Width(),map.Height()];
        int toX = sprime.x;
        int toY = sprime.y;
        Vector2Int min = s;
        float [,] dist;

        if(node.x == s.x && node.y == s.y)
            throw new InvalidOperationException("Invalid state");

        dist = new float[map.Width(), map.Height()];
        for (int y = 0; y < map.Height(); y++) {
            for (int x = 0; x < map.Width(); x++) {
                dist[x,y] = 1e30f;
            }
        }

        dist[s.x,s.y] = 0;
        explore.Enqueue(min,0);

        while(explore.size > 0){

            min = explore.Dequeue();

            //If destination reached
            if(min.x == toX && min.y == toY){
                if(!sExist) sgTLGraph.RemoveVertex(s);
                if(!sprimeExist) sgTLGraph.RemoveVertex(sprime);
                return dist[toX, toY];
            }

            

            //For all neighborhood v update the distance
            neighborhood.Clear();
            AddNeighborhood(min.x, min.y, AStar.ALL, neighborhood, true);

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
                    }
                }
            }
            
        }

        if(!sExist) sgTLGraph.RemoveVertex(s);
        if(!sprimeExist) sgTLGraph.RemoveVertex(sprime);
        UnityEngine.Debug.Log("No path exist" );
        return float.MaxValue;
    }

    bool IsNecessaryToConnect(Vector2Int node, Vector2Int s, Vector2Int sprime){
        if(isHreachable(s, sprime))
            return false;
        if(costOtherPath(node,s,sprime) <= Map.Octile(node.x, node.y, s.x , s.y) + Map.Octile(node.x, node.y, sprime.x , sprime.y))
            return false;
        return true;
    }

    void PruneSubgoal(Vector2Int node){
        List<Vector2Int> nodeNeighbors = new List<Vector2Int>();
        foreach(Vector2Int s in sgTLGraph.neighborhood(node)){
            nodeNeighbors.Add(s);
        }
        sgTLGraph.RemoveVertex(node);

        foreach(Vector2Int s in nodeNeighbors){
            foreach(Vector2Int sprime in nodeNeighbors){
                if(s.x != sprime.x || s.y != sprime.y){
                    if(Map.Octile(s.x, s.y, sprime.x, sprime.y) == Map.Octile(node.x, node.y, s.x, s.y) + Map.Octile(node.x, node.y, sprime.x, sprime.y) ){
                        if(costOtherPath(node,s,sprime) > Map.Octile(node.x, node.y, s.x, s.y) + Map.Octile(node.x, node.y, sprime.x, sprime.y)){
                            if(!sgTLGraph.Contains(s))sgTLGraph.AddVertex(s);
                            if(!sgTLGraph.Contains(sprime))sgTLGraph.AddVertex(sprime);
                            sgTLGraph.AddEdge(s,sprime);
                        }
                    }
                }
            }
        }
        
    }

    public void computeTL(){
        int count = 0;
        isTL = true;
        bool necessary = false;

        UnityEngine.Debug.Log("# of nodes : "+ VertexCountTL);
        UnityEngine.Debug.Log("# of edges : "+ EdgeCountTL);

        foreach(Vector2Int node in sgTLGraph.getNodes()){
            if(!sgTLGraph.Contains(node)) continue;
            
            necessary = false;
            foreach(Vector2Int s in sgTLGraph.neighborhood(node)){
                foreach(Vector2Int sprime in sgTLGraph.neighborhood(node)){
                    if( (s.x != sprime.x || s.y != sprime.y) && IsNecessaryToConnect(node,s,sprime)){
                        necessary = true;
                        break;
                    }
                }
                if(necessary)break;
            }
            if(!necessary){
                PruneSubgoal(node);
                count++;
            }
        }

        UnityEngine.Debug.Log("# of pruned nodes : "+ count);
        UnityEngine.Debug.Log("# of nodes : "+ VertexCountTL);
        UnityEngine.Debug.Log("# of edges : "+ EdgeCountTL);
        foreach(Vector2Int p in sgTLGraph.getNodes()){
            map.setMark(AStar.SELECTEDNODES, p.x, p.y);
        }
        map.Notify();
             
    }

    public int CharacterX(){
        return map.CharacterX();
    }

    public int CharacterY(){
        return map.CharacterY();
    }

    internal void AddNeighborhood(int x, int y, int aLL, List<Vector2Int> neighborhood, bool onlyGlobal = false )
    {
        Vector2Int cell = new Vector2Int(x,y);
        if(!onlyGlobal && sgGraph.Contains(cell)){
            foreach(Vector2Int p in sgGraph.neighborhood(cell))
                    neighborhood.Add(p);
        }
        if(isTL){
            if(sgTLGraph.Contains(cell)){
                foreach(Vector2Int p in sgTLGraph.neighborhood(cell))
                    if(!neighborhood.Contains(p)) neighborhood.Add(p);
            }
        }
    }
}
