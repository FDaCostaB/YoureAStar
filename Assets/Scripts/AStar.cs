using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;


public class AStar {
    public const int REACHABLE = 0x00000F;
    public const int SCANNED = 0x0000F0;
    public const int PATH = 0x000F00;
    //public const int USED = 0x00F000;
    bool refresh = true;
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

        if(refresh || doRefresh){
            map.eraseMark();
            while (reachable.Count > 0) {
                curr = reachable[0];
                reachable.RemoveAt(0);
                AddNeighborhood(curr.x, curr.y, 0, reachable, true);
                map.setMark(REACHABLE, curr.x, curr.y);
            }
        }

        return;

    }

    List<Vector2Int> AddNeighborhood(int x, int y, int color, List<Vector2Int> res, bool doMark = false){
        if(x-1 >= 0 && map.isFree(x-1,y) && map.mark(x-1,y) == color){
            res.Add(new Vector2Int(x-1,y));
            if(doMark) map.setMark(REACHABLE, x-1, y);
        }
        if(y-1 >= 0 && map.isFree(x,y-1) && map.mark(x,y-1) == color){
            res.Add(new Vector2Int(x,y-1));
            if(doMark) map.setMark(REACHABLE, x, y-1);
        }
        if(x+1 < map.Width() && map.isFree(x+1,y) && map.mark(x+1,y) == color){
            res.Add(new Vector2Int(x+1,y));
            if(doMark) map.setMark(REACHABLE, x+1, y);
        }
        if(y+1 < map.Height() && map.isFree(x,y+1) && map.mark(x,y+1) == color){
            res.Add(new Vector2Int(x,y+1));
            if(doMark) map.setMark(REACHABLE, x, y+1);
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
        List<Vector2Int> explore = new List<Vector2Int>();
        List<Vector2Int> voisins =  new List<Vector2Int>();
        List<Move> res =  new List<Move>();
        Vector2Int[,] pred = new Vector2Int[map.Width(),map.Height()];
        int minDH = int.MaxValue;
        int fromX = map.CharacterX();
        int fromY = map.CharacterY();
        Vector2Int curr,min= new Vector2Int(-1,-1);
        int [,] dist;

        //Marquer les cases atteignable
        mark(true);
        refresh=false;
        Move cp = new Move(map, charNb);

        //si inatteignable on joue rien
        if(map.mark(toX,toY)!=REACHABLE){
            UnityEngine.Debug.Log("Aucun chemin existe" );
            refresh=true;
            return null;
        }


        dist = new int[map.Width(), map.Height()];
        for (int y = 0; y < map.Height(); y++) {
            for (int x = 0; x < map.Width(); x++) {
                dist[x,y] = int.MaxValue;
            }
        }
        dist[fromX,fromY]=0;

        min=new Vector2Int(fromX,fromY);
        explore.Add(min);
        while(explore.Count > 0){

            minDH = int.MaxValue;
            min = new Vector2Int(-1,-1);
            foreach(Vector2Int p in explore){
                if( (min.x==-1 && min.y==-1) || minDH > distHeuristic(dist,p.x,p.y,toX,toY)){
                    minDH = distHeuristic(dist,p.x,p.y,toX,toY);
                    min = p;
                }
            }


            //si min==destination
            if(min.x == toX && min.y == toY){
                refresh=true;
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

            bool remove=false;
            for(int i = 0; i < explore.Count && !remove; i++){
                remove=false;
                Vector2Int p = explore[i];
                if(p.x==min.x && p.y==min.y){
                    explore.RemoveAt(i);
                    remove = true;
                    cp.scanned++;
                    map.setMark(SCANNED,min.x,min.y);
                }
            }

            //vider voisins
            voisins.Clear();


            //pour tous les v voisins de u mettre à jour la distance
            AddNeighborhood(min.x,min.y,REACHABLE,voisins);
            foreach(Vector2Int p in voisins){
                if( dist[min.x,min.y] + 1 < dist[p.x,p.y]){
                    dist[p.x,p.y] = dist[min.x,min.y] +1;
                    explore.Add(p);
                    pred[p.x,p.y] = min;
                }
            }
            
        }

        refresh=true;
        return null;
    }

    int distHeuristic(int [,]dist, int pt1x, int pt1y, int pt2x, int pt2y){
        return Mathf.Abs(pt2x-pt1x) + Mathf.Abs(pt2y-pt1y) + dist[pt1x,pt1y];
        // Djikstra's version
        // return dist[pt1x][pt1y]+1;
    }

}
