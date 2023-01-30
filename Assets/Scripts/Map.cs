using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;


public class Map : Subject {
	/* 	A grid cell is an int that encode WALL, WHARACTER, FREE
		One bit encode the presence of each object
	*/
    static int WALL = 1;
	static int CHARACTER = 2;
	static int WATER = 4;

	int [,] content;
	int width, height;
    int charcater = 0;
	public int FreeTiles = 0;
	List<Vector2Int> characterPos = new List<Vector2Int>();

	public String name;

    Directions[] dirs = {Directions.NORTH,Directions.WEST,Directions.EAST,Directions.SOUTH,Directions.NORTHWEST,Directions.NORTHEAST,Directions.SOUTHWEST,Directions.SOUTHEAST};

	public Map(String fileName) {
		name = fileName;
		content = new int[1,1];
		width = 0;
		height = 0;
	}

	public Vector2Int closestFree(int x, int y){
        int i =0;
		while(true){
			foreach(Directions d in dirs){
				Vector2Int moved = Map.move(x,y,d,i+1);
				if(isIn(moved) && isFree(moved) && mark(moved) == AStar.REACHABLE) return moved;
				foreach(Directions card in Map.cardinalOpposite(d)){
					for(int j = 1; j < i+1; j++){
						Vector2Int movedPrime = Map.move(moved.x,moved.y,card,j);
						if(isIn(movedPrime) && isFree(movedPrime) && mark(movedPrime) == AStar.REACHABLE) return movedPrime;
					}

				}
			}
			i++;
		}
	}

	int adjust(int c, int i) {
		while (c <= i) {
			c *= 2;
		}
		return c;
	}

	void resize(int x, int y) {
		int oldX = content.GetLength(0);
		int oldY = content.GetLength(1);
		if ((oldX <= x) || (oldY <= y)) {
			int newY = adjust(oldY, y);
			int newX = adjust(oldX, x);
            Debug.LogWarning("Resized : "+ newX +", "+newY);
			int [,] newTab = new int[newX,newY];
			for (int j=0; j<oldY; j++)
				for (int i=0; i<oldX; i++)
					newTab[i,j] = content[i,j];
			content = newTab;
		}
	}

	public void add(int element, int x, int y) {
		resize(x, y);
		content[x,y] |= element;
		if (width <= x)
			width = x+1;
		if (height <= y)
			height = y+1;
		if (element == 0) FreeTiles++;
	}

	public void delete(int element, int i, int j) {
		content[i,j] &= ~element;
	}
	public void addWall(int x, int y) {
		add(WALL, x, y);
	}

	public void addWater(int x, int y) {
		add(WATER, x, y);
	}

	public void addCharacter(int x, int y) { 
		add(CHARACTER, x, y);
		characterPos.Add(new Vector2Int(x,y));
		FreeTiles++;
	}

	public void selectChar(int x, int y) {
		for(int i = 0; i < characterPos.Count; i++){
			if(characterPos[i].x==x && characterPos[i].y==y ) charcater = i;
		}
		Notify();
	}

	public int findChar(int x, int y) {
		for(int i = 0; i < characterPos.Count; i++){
			if(characterPos[i].x==x && characterPos[i].y==y ) return i;
		}
		return -1;
	}

	public int currentChar() {
		return charcater;
	}

	public Vector2Int currentCharPos() {
		return characterPos[charcater];
	}
	
	public int nbChar() {
		return characterPos.Count;
	}

	public int Height() {
		return height;
	}

	public int Width() {
		return width;
	}

	public static Vector2Int move(int x, int y, Directions d, int times=1){
		Vector2Int res = new Vector2Int(x,y);
		if(d==Directions.NORTHWEST || d==Directions.WEST || d==Directions.SOUTHWEST ) res.x = x-times;
		if(d==Directions.NORTHEAST || d==Directions.EAST || d==Directions.SOUTHEAST ) res.x = x+times;
		if(d==Directions.NORTHEAST || d==Directions.NORTH || d==Directions.NORTHWEST )res.y = y-times;
		if(d==Directions.SOUTHEAST || d==Directions.SOUTH || d==Directions.SOUTHWEST ) res.y = y+times;
		return res;
	}

	public static Vector2Int move(Vector2Int p, Directions d, int times=1){
		return move(p.x, p.y, d, times);
	}

	public static int moveX(int x, Directions d){
		if(d==Directions.NORTHWEST || d==Directions.WEST || d==Directions.SOUTHWEST ) return --x;
		if(d==Directions.NORTHEAST || d==Directions.EAST || d==Directions.SOUTHEAST ) return ++x;
		return x;
	}

	public static int moveY(int y, Directions d){
		if(d==Directions.NORTHEAST || d==Directions.NORTH || d==Directions.NORTHWEST ) return --y;
		if(d==Directions.SOUTHEAST || d==Directions.SOUTH || d==Directions.SOUTHWEST ) return ++y;
		return y;
	}

	public bool isWall(int x, int y) {
		return (content[x,y] & WALL) != 0;
	}

	public bool isWater(int x, int y) {
		return (content[x,y] & WATER) != 0;
	}

	public bool isCharacter(int x, int y) {
		return (content[x, y] & CHARACTER) != 0;
	}

	public bool isFree(int x, int y) {
		return isIn(x,y) && !isWall(x,y) && !isWater(x,y);
	}

	public bool isFree(Vector2Int p) {
		return isFree(p.x,p.y);
	}

    public bool isFree(GridTile p)
    {
        return isFree(p.x, p.y);
    }

    public bool isFree(int x, int y, Directions d) {
		int newX = moveX(x,d); int newY = moveY(y,d);
		return newX>=0 && newX<Width() && newY>=0 && newY<Height() && isFree(newX,newY);
	}

	public bool isIn(int x, int y){
		return x>=0 && x<Width() && y>=0 && y<Height();
	}

	public bool isIn(Vector2Int p) {
		return isIn(p.x,p.y);
	}

	public int CharacterY() {
		return characterPos[charcater].y;
	}

	public int CharacterX() {
		return characterPos[charcater].x;
	}

    public int CharacterY(int i) {
		return characterPos[i].y;
	}

	public int CharacterX(int i) {
		return characterPos[i].x;
	}

    public int mark(int x, int y) {
		return (content[x,y] >> 8) & 0xFFFFFF;
	}

	public int mark(int x, int y, Directions d) {
		int newX = moveX(x,d); int newY = moveY(y,d);
		if(newX>=0 && newX<Width() && newY>=0 && newY<Height()) return ((content[newX,newY] >> 8) & 0xFFFFFF);
		else return 0x000000;
	}

	public int mark(Vector2Int p) {
		return (content[p.x,p.y] >> 8) & 0xFFFFFF;
	}

	public bool isSubgoal(int x, int y){
		return mark(x,y) == AStar.NODES || mark(x,y) == AStar.SELECTEDNODES;
	}

	public bool isSubgoal(Vector2Int p){
		return isSubgoal(p.x,p.y);
	}

	public void setMark(int m, int x, int y, bool force = false) {
		if(!isSubgoal(x,y) || m == AStar.SELECTEDNODES || m == AStar.NODES ||force)
			content[x,y] = (content[x,y] & 0xFF) | (m << 8);
	}

	public void eraseMark(){
		for (int y = 0; y < Height(); y++) {
			for (int x = 0; x < Width(); x++) {
				if(!isSubgoal(x,y) && mark(x,y) != AStar.EMPTY) setMark(AStar.REACHABLE, x, y);
                if (mark(x, y) == AStar.SELECTEDNODES) setMark(AStar.NODES, x, y);
            }
		}
	}

    public void eraseNodes()
    {
        for (int y = 0; y < Height(); y++)
        {
            for (int x = 0; x < Width(); x++)
            {
                if (mark(x, y) == AStar.NODES || mark(x, y) == AStar.SELECTEDNODES) setMark(AStar.REACHABLE, x, y, true);
            }
        }
    }

    public static Directions[] cardinalAssociated(Directions d){
        if(d == Directions.NORTH || d == Directions.WEST || d == Directions.EAST || d == Directions.SOUTH) {
			Directions[] empty = new Directions[0];
			return empty;
		}
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

		public static Directions[] cardinalOpposite(Directions d){
        if(d == Directions.NORTH || d == Directions.WEST || d == Directions.EAST || d == Directions.SOUTH) {
			Directions[] empty = new Directions[0];
			return empty;
		}
        Directions[] res = new Directions[2];
        switch(d){
            case Directions.NORTHWEST:
			    res[0] = Directions.SOUTH;
                res[1] = Directions.EAST;
                break;
            case Directions.SOUTHWEST:
                res[0] = Directions.NORTH;
                res[1] = Directions.EAST;
                break;
            case Directions.NORTHEAST:
                res[0] = Directions.SOUTH;
                res[1] = Directions.WEST;
                break;
            case Directions.SOUTHEAST:
			    res[0] = Directions.NORTH;
                res[1] = Directions.WEST;
                break;

        }
        return res;
    }

	public Step playStep(Move cp) {
		Step d = cp.Steps()[0];
		//if(cp.charNb == charcater)setMark(AStar.USED,d.fromX,d.fromY);
        cp.Steps().RemoveAt(0);
		if (isFree(d.toX, d.toY)) {
			delete(CHARACTER, CharacterX(cp.charNb), CharacterY(cp.charNb));
			characterPos[cp.charNb] = new Vector2Int(d.toX,d.toY);
			add(CHARACTER, d.toX, d.toY);
		}
		return d;
	}

	public float distHeuristic(int pt1x, int pt1y, int pt2x, int pt2y){
        //dist is g i.e. the cost from the start to pt1 the rest is the heuristic h
		int multiplier = Parameters.instance.heuristicMultiplier;
        switch(Parameters.instance.heuristic){
            case Heuristics.Manhattan:
                return multiplier * Manhattan(pt1x, pt1y, pt2x, pt2y);
            case Heuristics.Euclidean:
                return multiplier * Euclidean(pt1x, pt1y, pt2x, pt2y);
            case Heuristics.Chebyshev:
				return multiplier * Chebyshev(pt1x, pt1y, pt2x, pt2y);
            case Heuristics.Octile:
                return multiplier * Octile(pt1x, pt1y, pt2x, pt2y);
            case Heuristics.Djikstra:
                return 1;
            default:
                return 1;
        }

    }

	public static float Manhattan(int pt1x, int pt1y, int pt2x, int pt2y){
		int dx = Mathf.Abs(pt2x - pt1x);
        int dy = Mathf.Abs(pt2y - pt1y);
		return Mathf.Abs(pt2x - pt1x) + Mathf.Abs(pt2y - pt1y); 
	}
	public static float Euclidean(int pt1x, int pt1y, int pt2x, int pt2y){
		return Mathf.Sqrt(Mathf.Abs(pt2x - pt1x) + Mathf.Abs(pt2y - pt1y));
	}
	public static float Chebyshev(int pt1x, int pt1y, int pt2x, int pt2y){
		return (Mathf.Abs(pt2x - pt1x) + Mathf.Abs(pt2y - pt1y)) - (Mathf.Abs(pt2x - pt1x) > Mathf.Abs(pt2y - pt1y) ? Mathf.Abs(pt2y - pt1y) : Mathf.Abs(pt2x - pt1x));
	}
	public static float Octile(int pt1x, int pt1y, int pt2x, int pt2y){
		return (Mathf.Abs(pt2x - pt1x) + Mathf.Abs(pt2y - pt1y)) + (1.4f - 2) * (Mathf.Abs(pt2x - pt1x) > Mathf.Abs(pt2y - pt1y) ? Mathf.Abs(pt2y - pt1y) : Mathf.Abs(pt2x - pt1x));
	}

	//Check if caller might benits from bidirectionnal
    public List<Vector2Int> AddNeighborhood(int x, int y, int color, List<Vector2Int> res, bool doMark = false){
        byte neighborhood = 0x0;
        for(int i=0; i<8;i++){
            Directions d = (Directions)(1<<i);
            neighborhood |=  (byte) ((isFree(x,y,d) && ((mark(x,y,d) & color) != 0) )  ? 0x1 << i : 0x0);
        }
        for(int i=0; i<8;i++){
            Directions d = (Directions)(1<<i);
			if( (neighborhood | MaskFree.maskFree[i]) == 0xFF){
				res.Add(new Vector2Int(Map.moveX(x,d), Map.moveY(y,d)));
				if(doMark) setMark(AStar.REACHABLE, Map.moveX(x,d), Map.moveY(y,d));
			}
        }
        return res;
		}

	public void clear()
	{
        for (int j = 0; j < height; j++)
            for (int i = 0; i < width; i++)
                content[i, j] = 0;
    }

}
