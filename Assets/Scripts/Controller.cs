using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class Controller : IEventCollector {
	Map map;
	public bool pause;
	AStar aStar;
	List<AnimationMove> mouvement;
	GameZone gZone;
	Camera cam;
	float tileSize;

	public Controller(Map m, Camera c, GameZone gz) {
		cam = c;
		map = m;
		pause = false;
		gZone = gz;
		aStar = new AStar(map);
		mouvement = new List<AnimationMove>();
		tileSize = (int) ((float)Screen.height/(2*cam.orthographicSize));
		for(int i =0; i<map.nbChar();i++){
			mouvement.Add(null);
		}
	}

	public void setGraphicInterface() {
		
	}

    public void clock(){
		for(int i =0; i<map.nbChar();i++){
			if(mouvement[i]!=null){
				mouvement[i].tictac();
				if(mouvement[i].IsFinished()){
					mouvement[i]=null;
					map.Notify();
				}
			}
		}
    }

	public Step playStep(Move cp) {
		if (cp != null) {
			Step d = map.playStep(cp);
			return d;
		} else {
			Debug.LogError("Empty Coup");
			return null;
		}
	}

	// Clic dans la case (l, c)
	public void mouseClic(float x, float y) {
		Vector2Int clic = new Vector2Int((int) Math.Floor(x + cam.GetComponent<Transform>().position.x), (int) Math.Floor(y + 1 - cam.GetComponent<Transform>().position.y));
		if( clic.x > map.Width() || clic.x < 0  || clic.y > map.Height() || clic.y < 0 ) return;
		if(map.isCharacter(clic.x,clic.y) && (pause || isSelectable(map.findChar(clic.x,clic.y)))){
			map.selectChar(clic.x,clic.y);
		} else {
			if(mouvement[map.currentChar()]!=null){
				mouvement[map.currentChar()].stop();
				mouvement[map.currentChar()]=null;
			}
			if(mouvement[map.currentChar()]==null){
				Move cp = aStar.measurePath(clic.x, clic.y, map.currentChar());
				if(cp!=null) {
					mouvement[map.currentChar()] = new AnimationMove(cp, this, gZone);
					if(pause)mouvement[map.currentChar()].stop();
				}
			}
		}
		map.Notify();
	}

	internal void centerCam(){
		cam.GetComponent<Transform>().position = new Vector3(map.CharacterX(),-map.CharacterY(),-10);
	}

    internal void up()
    {
        cam.GetComponent<Transform>().position += new Vector3(0,5,0) * gZone.camSpeed;
    }

    internal void left()
    {
        cam.GetComponent<Transform>().position -= new Vector3(5,0,0) * gZone.camSpeed;
    }

    internal void down()
    {
        cam.GetComponent<Transform>().position -= new Vector3(0,5,0) * gZone.camSpeed;
    }

    internal void right()
    {
        cam.GetComponent<Transform>().position += new Vector3(5,0,0) * gZone.camSpeed;
    }

    internal void zoom(int zoom)
    {
        int newSize = (int) cam.orthographicSize - (zoom * gZone.camSpeed);
        if( newSize > 10)
            cam.orthographicSize = newSize;
		gZone.updateTileSize();
    }

    internal void tooglePause()
    {
        pause = !pause;
		for(int i =0; i<map.nbChar();i++){
			if(mouvement[i]!=null){
				if(pause)mouvement[i].stop();
				else mouvement[i].start();
			}
		}
		map.Notify();
    }

	internal void debug(){
		aStar.debug();
	}

	internal void debug(float x, float y){
		Vector2Int clic = new Vector2Int((int) Math.Floor(x + cam.GetComponent<Transform>().position.x), (int) Math.Floor(y + 1 - cam.GetComponent<Transform>().position.y));
		aStar.path(clic.x, clic.y, map.currentChar());
	}

    internal void clear()
    {
        map.eraseMark();
    }

    internal bool isSelectable(int i)
    {
        return mouvement[i]==null;
    }

    internal bool isMoving(int i)
    {
		return mouvement[i]!=null && mouvement[i].StepFinished();
    }
}
