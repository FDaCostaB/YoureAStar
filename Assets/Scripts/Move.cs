using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move {
	List<Step> steps;
	public int scanned;
	public int charNb;
	public int openSetMaxSize;
	Map map;	

	public Move(Map n, int cNb) {
		steps = new List<Step>();
		scanned = 0;
		openSetMaxSize = 0;
		map = n;
		charNb = cNb;
	}

	public void deplace(int dX, int dY, int vX, int vY) {
		steps.Add(new Step(dX, dY, vX, vY));
	}


	public List<Step> Steps() {
		return steps;
	}

	public void setOpenSetMaxSize(int size){
		openSetMaxSize = Mathf.Max(size,openSetMaxSize);
	}

}

public class Step {
	public int fromX, fromY, toX, toY;

	public Step(int dX, int dY, int vX, int vY) {
		fromX = dX;
		fromY = dY;
		toX = vX;
		toY = vY;
	}
}

public class Mark {
    public int line, column, value;

    public Mark(int x, int y, int val) {
        line = y;
        column = x;
        value = val;
    }
}