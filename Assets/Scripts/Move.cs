using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move {
	List<Step> steps;
	public int scanned;
	public int charNb;
	public int openSetMaxSize;
	public long time;
	Map map;	

	public Move(Map n, int cNb) {
		steps = new List<Step>();
		scanned = 0;
		openSetMaxSize = 0;
		map = n;
		charNb = cNb;
		time = -1;
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
	public float length;

	public Step(int dX, int dY, int vX, int vY, float l = 1) {
		fromX = dX;
		fromY = dY;
		toX = vX;
		toY = vY;
		length = l;
	}
}