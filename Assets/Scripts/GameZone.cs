using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameZone : MonoBehaviour, IObserver
{

    public GameObject character;
    public Camera cam;
    private List<Transform> characterList;
    private Map map;
    private Tile ground;
    private Tile path;
    private Tile reachable;
    private Tile scanned;
    private Tile used;
    private Tile wall;
    private Tile npc;
    public float tileSize {get; private set;}
    private InputAdapter input;
    private Controller c;

    private void Awake(){
        try {
			MapReader mapReader = new MapReader("Assets/output.txt");
			map = mapReader.read();
            map.Attach(this);
			Debug.Log("Map size : " + map.Width() +","+ map.Height());
            tileSize = Screen.height/(2*cam.orthographicSize);
            ground = TilesResourcesLoader.GetTileByName("Ground");
            path = TilesResourcesLoader.GetTileByName("path");
            reachable = TilesResourcesLoader.GetTileByName("reachable");
            scanned = TilesResourcesLoader.GetTileByName("scanned");
            used = TilesResourcesLoader.GetTileByName("used");
            wall = TilesResourcesLoader.GetTileByName("Wall");
            npc = TilesResourcesLoader.GetTileByName("NPC");
            c = new Controller(map,cam);
            c.setGraphicInterface(this);
            characterList = new List<Transform>();
            input = new InputAdapter(this,c);
		} catch (Exception e) {
			Debug.LogError(e);
            Application.Quit();
		}
        Paint(true);
    }

    private void Paint(bool initPaint = false){
        Tilemap levelMap = GetComponentsInChildren<Tilemap>()[0];
        
        //levelMap.ClearAllTiles();
        for(int j = 0; j < map.Height(); j++)
        {
            for(int i = 0; i < map.Width(); i++)
            {
                if(map.isWall(i,j)){
                    levelMap.SetTile(new Vector3Int(i,-j,0), wall);
                } else {
                    switch(map.mark(i,j)){
                    case AStar.REACHABLE:
                        levelMap.SetTile(new Vector3Int(i,-j,0), reachable );
                        break;
                    case AStar.SCANNED:
                        levelMap.SetTile(new Vector3Int(i,-j,0), scanned );
                        break;
                    case AStar.PATH:
                        levelMap.SetTile(new Vector3Int(i,-j,0), path );
                        break;
                    /*case AStar.USED:
                        levelMap.SetTile(new Vector3Int(i,-j,0), used);
                        break;*/
                    default:
                        levelMap.SetTile(new Vector3Int(i,-j,0), ground );
                        break;

                }
                }
                if(map.isCharacter(i,j)){
                    if(initPaint){
                        UnityEngine.GameObject newChar = Instantiate(character);
                        characterList.Add(newChar. GetComponent<Transform>());
                        characterList[characterList.Count-1].position = new Vector3(i+0.5f,-j+0.5f,-1);
                        c.centerCam();
                    }
                } 
            }
        }
        if(c.pause) {
            for(int i =0; i < map.nbChar(); i++) characterList[i].gameObject.GetComponent<SpriteRenderer>().color = Color.yellow;
        } else {
            for(int i =0; i < map.nbChar(); i++) characterList[i].gameObject.GetComponent<SpriteRenderer>().color = c.isSelectable(i) ? Color.yellow : Color.red;
        }
        characterList[map.currentChar()].gameObject.GetComponent<SpriteRenderer>().color = Color.magenta;
    }


    void IObserver.Update(ISubject subject){
        Paint();
    }

    public void shift(float dX, float dY, int charNb) {
		characterList[charNb].position = new Vector3(map.CharacterX(charNb) +0.5f + dX, -map.CharacterY(charNb) +0.5f + dY, characterList[charNb].position.z);
	}

    void Update(){
        input.inputCheck();
    }

    internal void updateTileSize()
    {
        tileSize = Screen.height/(2*cam.orthographicSize);
    }
}
