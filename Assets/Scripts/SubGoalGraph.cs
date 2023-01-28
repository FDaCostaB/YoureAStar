using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class SubGoalGraph {
    Map map;
    Graph sgGraph;
    Graph sgTLGraph;
    HashSet<Vector2Int> tempGlobalGoal;

    AStar astar;
    public bool isTL;
    public int buildTime {get;}
    public int VertexCount{get {return sgGraph.VertexCount;}}
    public int VertexCountTL{get {return sgTLGraph.VertexCount;}}
    public int EdgeCount{get {return sgGraph.EdgeCount;}}
    public int EdgeCountTL{get {return sgTLGraph.EdgeCount; }}

    Directions[] dirs = {Directions.NORTHWEST,Directions.NORTH,Directions.NORTHEAST,Directions.WEST,Directions.EAST,Directions.SOUTHWEST,Directions.SOUTH,Directions.SOUTHEAST};
    Directions[] diag = {Directions.NORTHWEST, Directions.NORTHEAST, Directions.SOUTHWEST, Directions.SOUTHEAST};
    
    public SubGoalGraph(Map m, AStar pathFinder){
        isTL=false;
        tempGlobalGoal = new HashSet<Vector2Int>();
        map =m;
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
        UnityEngine.Debug.Log("# of nodes : "+ VertexCount);
        UnityEngine.Debug.Log("# of edges : "+ EdgeCount);

    }

    public List<Vector2Int> GetDirectHReachable(Vector2Int cell, bool doDebug = false){

       List<Vector2Int> hReachable = new List<Vector2Int>();

        if(doDebug) map.eraseMark();

        foreach(Directions d in dirs){
            if(map.isSubgoal(Map.move(cell, d, Clearance(cell,d)))){
                hReachable.Add(Map.move(cell, d, Clearance(cell,d)));
                if(doDebug) map.setMark(AStar.SELECTEDNODES,Map.move(cell, d, Clearance(cell,d)).x, Map.move(cell, d, Clearance(cell,d)).y);
            }
        }
        foreach(Directions d in diag){
            foreach(Directions c in Map.cardinalAssociated(d)){
                int max = Clearance(cell,c);
                int diag = Clearance(cell,d);
                if(map.isSubgoal(Map.move(cell, c, max)))
                    max--;
                if(map.isSubgoal(Map.move(cell, d, diag)))
                    diag--;    
                for(int i =1; i<=diag; i++){
                    int j = Clearance(Map.move(cell, d,i),c);
                    if(doDebug) for(int k = 1; k<=j && k<max ;k++)
                        map.setMark(AStar.SCANNED,Map.move(Map.move(cell.x, cell.y,d,i),c,k).x, Map.move(Map.move(cell.x, cell.y,d,i),c,k).y);
                    if(j<=max && map.isSubgoal(Map.move(Map.move(cell.x, cell.y,d,i),c,j))){
                        hReachable.Add(Map.move(Map.move(cell.x, cell.y,d,i),c,j));
                        if(doDebug) 
                            map.setMark(AStar.SELECTEDNODES,Map.move(Map.move(cell.x, cell.y,d,i),c,j).x,Map.move(Map.move(cell.x, cell.y,d,i),c,j).y);
                        j--;
                    }
                    if(j<max) max = j;
                }
            }
        }
        return hReachable;
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
        //TODO : JPS FindHreachablePath on SGTL
        if (isTL || !map.parameters.skipHreachable)
        {
            for (int i = 0; i < globalPath.Steps().Count; i++)
                FindHreachablePath(cp, globalPath.Steps()[i].fromX, globalPath.Steps()[i].fromY, globalPath.Steps()[i].toX, globalPath.Steps()[i].toY);
            return cp;
        }
        return globalPath;
    }

    public Move TryDirectPath(Vector2Int goal){
        Move res = new Move(map, map.currentChar());
        Vector2Int from = map.currentCharPos();
        int N = diagonalDistance(map.currentCharPos(), goal);
        for (int step = 0; step <= N; step++) {
            
            float t = N == 0 ? 0.0f : (float) step / N;
            Vector2Int point = roundPoint(lerpPoint(map.currentCharPos(), goal, t));
            if(map.isFree(point)) {
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
            tempGlobalGoal.Add(cell);
            foreach(Vector2Int cellPrime in GetDirectHReachable(cell)){
                sgGraph.AddEdge(cell,cellPrime);
            }
        }
    }

    void ConnectToGraphTL(Vector2Int cell)
    {
        if (!sgTLGraph.Contains(cell))
        {
            sgTLGraph.AddVertex(cell);
            tempGlobalGoal.Add(cell);
        }
        sgTLGraph.AddVertex(cell);
        if (!sgGraph.Contains(cell))
        {
            foreach (Vector2Int cellPrime in GetDirectHReachable(cell))
            {
                sgTLGraph.AddEdge(cell, cellPrime);
                if (!sgTLGraph.Contains(cellPrime))
                {
                    sgTLGraph.AddVertex(cellPrime);
                    tempGlobalGoal.Add(cellPrime);
                }
                    
            }
        }
    }

    Move FindAbstractPath(Vector2Int goal){
        Vector2Int start = map.currentCharPos();
        if (isTL)
        {
            ConnectToGraphTL(start);
            ConnectToGraphTL(goal);
        } else
        {
            ConnectToGraph(start);
            ConnectToGraph(goal);
        }
        Move res = astar.pathGraph(goal.x, goal.y, map.currentChar() );
        foreach(Vector2Int temp in tempGlobalGoal)
        {
            if (isTL) sgTLGraph.RemoveVertex(temp);
            else sgGraph.RemoveVertex(temp);
        }
        tempGlobalGoal.Clear();
        return res;
    }

    public Move measureFindAbstractPath(Vector2Int goal){
        Stopwatch timer = new Stopwatch();
        timer.Start();
        Move m = FindAbstractPath(goal);
        timer.Stop();
        if(m != null && !map.parameters.debug){
            Data.CacheLine(map.name, VertexCount, EdgeCount, buildTime, map.CharacterX(), map.CharacterY(), goal.x, goal.y, timer.ElapsedMilliseconds, m.scanned, m.openSetMaxSize, m.Steps().Count, "FindAbstractPath - " + (isTL? "SG-TL Graph": "SG Graph"), map.parameters.listType, map.parameters.heuristic, map.parameters.heuristicMultiplier);
        }
        return m;
    }

    public Move FindHreachablePath(Move cp, int fromX, int fromY, int toX, int toY)
    {
        IOpenList<Vector2Int> explore = map.parameters.newOpenList();
        List<Vector2Int> neighborhood = new List<Vector2Int>();
        Vector2Int[,] pred = new Vector2Int[map.Width(), map.Height()];
        Vector2Int curr, min = new Vector2Int(fromX, fromY);
        Dictionary<Vector2Int, float> dist = new Dictionary<Vector2Int, float>();

        float h = Map.Octile(fromX, fromY, toX, toY);
        explore.Enqueue(min, h);
        dist.Add(min, 0);
        int insertIdx = cp.Steps().Count;

        while (explore.size > 0)
        {

            min = explore.Dequeue();
            cp.scanned++;

            //If destination reached
            if (min.x == toX && min.y == toY)
            {
                curr = new Vector2Int(toX, toY);
                map.setMark(AStar.PATH, toX, toY);
                Step d;
                while (curr.x != fromX || curr.y != fromY)
                {
                    d = new Step(pred[curr.x, curr.y].x, pred[curr.x, curr.y].y, curr.x, curr.y);
                    cp.Steps().Insert(insertIdx, d);
                    curr = new Vector2Int(pred[curr.x, curr.y].x, pred[curr.x, curr.y].y);
                    map.setMark(AStar.PATH, curr.x, curr.y);
                }
                return cp;
            }

            neighborhood.Clear();
            map.AddNeighborhood(min.x, min.y, AStar.ALL, neighborhood);
            float neighborhoodDist;
            foreach (Vector2Int next in neighborhood)
            {
                if (min.x != next.x && min.y != next.y) neighborhoodDist = 1.4f;
                else neighborhoodDist = 1;

                if ( (!dist.ContainsKey(next) || dist[min] + neighborhoodDist < dist[next] +0.01f) && dist[min] + neighborhoodDist < h +0.01f && Map.Octile(min.x, min.y, next.x, next.y) + Map.Octile(next.x, next.y, toX, toY) <= Map.Octile(min.x, min.y, toX, toY) + 0.01f)
                {
                    if (!dist.ContainsKey(next)) dist.Add(next, dist[min] + neighborhoodDist);
                    else dist[next] = dist[min] + neighborhoodDist;
                    pred[next.x, next.y] = min;

                    int idx = explore.Find(p => p.x == next.x && p.y == next.y);
                    if (idx >= 0)
                    {
                        explore.changePriority(idx, dist[min] + Map.Octile(next.x, next.y, toX, toY));
                    }
                    else
                    {
                        explore.Enqueue(next, dist[min] + Map.Octile(next.x, next.y, toX, toY));
                        cp.setOpenSetMaxSize(explore.size);
                    }
                }
            }

        }

        UnityEngine.Debug.Log("Not hReachable");
        return null;
    }

    public bool isHreachable(Vector2Int start, Vector2Int goal){
        IOpenList<Vector2Int> explore = map.parameters.newOpenList();
        HashSet<Vector2Int> closed = new HashSet<Vector2Int>();
        List<Vector2Int> neighborhood =  new List<Vector2Int>();
        Vector2Int min=start;
        float h = Map.Octile(start.x, start.y, goal.x, goal.y);

        explore.Enqueue(min,h);

        while (explore.size > 0){

            float currh = explore.getTopPriority();
            min = explore.Dequeue();
            closed.Add(min);

            //If destination reached
            if(min.x == goal.x && min.y == goal.y){
                return true;
            }

            neighborhood.Clear();
            float neighborhoodDist;
            //TODO :Parallelize for
            foreach (Vector2Int next in map.AddNeighborhood(min.x,min.y,AStar.ALL,neighborhood)){
                if (min.x != next.x && min.y != next.y) neighborhoodDist = 1.4f;
                else neighborhoodDist = 1;
                if (!closed.Contains(next) && currh-neighborhoodDist + 0.01f >= 0 && Map.Octile(min.x,min.y,next.x,next.y) + Map.Octile(next.x, next.y, goal.x, goal.y) <= Map.Octile(min.x, min.y, goal.x, goal.y) + 0.01f)
                {
                    int idx = explore.Find(p => p.x == next.x && p.y == next.y);
                    if (idx >= 0)
                    {
                        explore.changePriority(idx, currh - neighborhoodDist);
                    }
                    else
                    {
                        explore.Enqueue(next, currh - neighborhoodDist);
                    }
                }
            }

        }
        return false;
    }

    public float costOtherPath(Vector2Int node, Vector2Int s, Vector2Int sprime){

        IOpenList<Vector2Int> explore = map.parameters.newOpenList();
        List<Vector2Int> neighborhood =  new List<Vector2Int>();
        List<Move> res =  new List<Move>();
        Vector2Int[,] pred = new Vector2Int[map.Width(),map.Height()];
        int toX = sprime.x;
        int toY = sprime.y;
        Vector2Int min = s;
        float [,] dist;

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
                return dist[toX, toY];
            }

            

            //For all neighborhood v update the distance
            neighborhood.Clear();
            AddNeighborhood(min.x, min.y, neighborhood, true);
            //TODO :Parallelize for
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
        AddNeighborhood(node.x, node.y, nodeNeighbors);
        sgTLGraph.RemoveVertex(node);

        foreach(Pair<Vector2Int, Vector2Int> p in PairList<Vector2Int>.uniquePair(nodeNeighbors)){
            Vector2Int s = p.Elem;
            Vector2Int sprime = p.ElemB;
            if (s.x != sprime.x || s.y != sprime.y){
                float heurDiff = Mathf.Abs(Map.Octile(s.x, s.y, sprime.x, sprime.y) - (Map.Octile(node.x, node.y, s.x, s.y) + Map.Octile(node.x, node.y, sprime.x, sprime.y)));
                if (heurDiff < 0.01f ){
                    if(costOtherPath(node,s,sprime) +0.01f > Map.Octile(node.x, node.y, s.x, s.y) + Map.Octile(node.x, node.y, sprime.x, sprime.y)){
                        sgTLGraph.AddEdge(s,sprime);
                    }
                }
            }
        }
        
    }

    public void computeTL(){
        //if already computed ealry out
        if(isTL) return;

        int count = 0;
        isTL = true;
        bool necessary = false;

        UnityEngine.Debug.Log("# of nodes : "+ VertexCountTL);
        UnityEngine.Debug.Log("# of edges : "+ EdgeCountTL);
        List<Vector2Int> neighborhood = new List<Vector2Int>();
        foreach (Vector2Int node in sgTLGraph.getNodes()){
            if(!sgTLGraph.Contains(node)) continue;
            
            necessary = false;
            neighborhood.Clear();
            AddNeighborhood(node.x, node.y, neighborhood);
            foreach(Pair<Vector2Int, Vector2Int> p in PairList<Vector2Int>.uniquePair(neighborhood))
            {
                Vector2Int s = p.Elem;
                Vector2Int sprime = p.ElemB;
                if ( (s.x != sprime.x || s.y != sprime.y) && IsNecessaryToConnect(node,s,sprime)){
                        necessary = true;
                        break;
                    }
                if(necessary)break;
            }
            if(!necessary){
                PruneSubgoal(node);
                count++;
            }
        }

        UnityEngine.Debug.Log("# of global nodes : "+ VertexCountTL);
        UnityEngine.Debug.Log("# of global edges : "+ EdgeCountTL);      
    }

    public void markGlobal(){
        foreach (Vector2Int p in sgTLGraph.getNodes()) {
            map.setMark(AStar.SELECTEDNODES, p.x, p.y);
        }
    }

    public int CharacterX(){
        return map.CharacterX();
    }

    public int CharacterY(){
        return map.CharacterY();
    }

    internal void AddNeighborhood(int x, int y, List<Vector2Int> neighborhood, bool onlyGlobal = false )
    {
        Vector2Int cell = new Vector2Int(x,y);
        if( !onlyGlobal && sgGraph.Contains(cell)){
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

    public LinkedList<Vector2Int> Neighborhood(int x, int y)
    {
        return sgGraph.neighborhood(new Vector2Int(x, y));
    }
    public LinkedList<Vector2Int> NeighborhoodTL(int x, int y)
    {
        return sgTLGraph.neighborhood(new Vector2Int(x, y));
    }

    public LinkedList<Vector2Int> linkedNodesTL(int x, int y)
    {
        return sgTLGraph.linkedNodes(new Vector2Int(x, y));
    }

    public bool Contains(int x, int y)
    {
        return (isTL ? sgTLGraph.Contains(new Vector2Int(x, y)) : sgGraph.Contains(new Vector2Int(x, y)) );
    }

    public bool isSubgoal(int x, int y)
    {
        if (isTL) return isGlobalSubgoal(x, y);
        else return map.isSubgoal(x, y);
    }

    public bool isSubgoal(Vector2Int p)
    {
        if (isTL) return isGlobalSubgoal(p.x, p.y);
        else return map.isSubgoal(p.x, p.y);
    }

    public bool isGlobalSubgoal(int x, int y)
    {
        return map.isSubgoal(x, y) && sgTLGraph.Contains(new Vector2Int(x, y));
    }

    public bool isGlobalSubgoal(Vector2Int p)
    {
        return isGlobalSubgoal(p.x, p.y);
    }
}
