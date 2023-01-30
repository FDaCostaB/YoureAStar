using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class Controller : IEventCollector {
	Map map;
	AStar aStar;
	List<AnimationMove> mouvement;
	GameZone gZone;
	Camera cam;

	public Controller(Map m, Camera c, GameZone gz) {
		cam = c;
		map = m;
		gZone = gz;
		aStar = new AStar(map);
		mouvement = new List<AnimationMove>();
		for(int i =0; i<map.nbChar();i++){
			mouvement.Add(null);
		}
	}

	public void UpdateNodes()
	{
		aStar.UpdateNodes();
	}

    public void RebuildHPA()
    {
        aStar.RebuildHPA();
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
			gZone.updateStart(map.CharacterX(), map.CharacterY());
			return d;
		} else {
			Debug.LogError("Empty Coup");
			return null;
		}
	}

	public void cancelMove()
	{
        if (mouvement[map.currentChar()] != null)
        {
            mouvement[map.currentChar()].stop();
            mouvement[map.currentChar()] = null;
        }
    }

	// Clic dans la case (l, c)
	public void mouseClic(float x, float y) {
		Vector2Int clic = new Vector2Int((int) Math.Floor(x + cam.GetComponent<Transform>().position.x), (int) Math.Floor(y + 1 - cam.GetComponent<Transform>().position.y));
		if( clic.x > map.Width() || clic.x < 0  || clic.y > map.Height() || clic.y < 0 ) return;
		if (map.isCharacter(clic.x, clic.y) && (Parameters.instance.pause || isSelectable(map.findChar(clic.x,clic.y)))){
			map.selectChar(clic.x,clic.y);
		} else {
			if(mouvement[map.currentChar()]!=null){
				mouvement[map.currentChar()].stop();
				mouvement[map.currentChar()]=null;
			}
			if(mouvement[map.currentChar()]==null){
				Move cp = aStar.measurePath(clic.x, clic.y);
				gZone.displayDebug(cp);
				if(cp!=null) {
					mouvement[map.currentChar()] = new AnimationMove(cp, this, gZone);
					if(Parameters.instance.pause) mouvement[map.currentChar()].stop();
				}
			}
		}
		map.Notify();
	}

	public void hover(float x, float y)
	{
		Vector2Int hoverTile = new Vector2Int((int)Math.Floor(x + cam.GetComponent<Transform>().position.x), (int)Math.Floor(y + 1 - cam.GetComponent<Transform>().position.y));
        if(map.isFree(hoverTile.x, hoverTile.y)) 
			gZone.highlightTile(hoverTile.x, hoverTile.y);
	}
        public void debugClic(float x, float y)
	{
        Vector2Int clic = new Vector2Int((int)Math.Floor(x + cam.GetComponent<Transform>().position.x), (int)Math.Floor(y + 1 - cam.GetComponent<Transform>().position.y));
        aStar.debug(clic.x, clic.y); 
		return;
    }

	internal void centerCam(){
		cam.GetComponent<Transform>().position = new Vector3(gZone.currentCharPos().x,gZone.currentCharPos().y,cam.GetComponent<Transform>().position.z);
	}

	internal void lockCam(){
        Parameters.instance.lockCam = !Parameters.instance.lockCam;
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
        int newSize = (int) cam.orthographicSize - (zoom * gZone.camSpeed /2);
        if( newSize > 10)
            cam.orthographicSize = newSize;
		gZone.updateTileSize();
    }

    internal void tooglePause()
    {
        Parameters.instance.pause = !Parameters.instance.pause;
		for(int i =0; i<map.nbChar();i++){
			if(mouvement[i]!=null){
				if(Parameters.instance.pause) mouvement[i].stop();
				else mouvement[i].start();
			}
		}
		map.Notify();
    }

	internal void debug(){
		aStar.debug();
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
