using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public class GameZone : MonoBehaviour, IObserver
{

    public TextAsset mapFile;
    public GameObject character;
    public Camera cam;
    public Parameters param;
    public float animSpeed = 0.1f;
    public int camSpeed = 1;
    public bool redraw;
    private List<Transform> characterList;
    private Map map;
    private Tile ground;
    private Tile path;
    private Tile reachable;
    private Tile scanned;
    private Tile nodes;
    private Tile selectedNodes;
    private Tile wall;
    private Tile water;
    private Tile npc;
    public float tileSize {get; private set;}
    private InputAdapter input;
    private Controller c;

    private void Awake(){
        Debug.Log(AssetDatabase.GetAssetPath(mapFile));
        characterList = new List<Transform>();
		MapReader mapReader = new MapReader( AssetDatabase.GetAssetPath(mapFile));
		map = mapReader.read();
        map.AttachParameters(param);
        map.Attach(this);
        c = new Controller(map, cam, this);
		Debug.Log("Map size : " + map.Width() +","+ map.Height());
        tileSize = Screen.height/(2*cam.orthographicSize);
        ground = TilesResourcesLoader.GetTileByName("Ground");
        path = TilesResourcesLoader.GetTileByName("path");
        reachable = TilesResourcesLoader.GetTileByName("reachable");
        scanned = TilesResourcesLoader.GetTileByName("scanned");
        nodes = TilesResourcesLoader.GetTileByName("nodes");
        selectedNodes = TilesResourcesLoader.GetTileByName("selectedNodes");
        wall = TilesResourcesLoader.GetTileByName("Wall");
        npc = TilesResourcesLoader.GetTileByName("NPC");
        water = TilesResourcesLoader.GetTileByName("Water");
        input = new InputAdapter(this,c);
        Data.Init();
        Paint(true);
    }

    private void Paint(bool initPaint = false){
        Tilemap levelMap = GetComponentsInChildren<Tilemap>()[0];

        for(int j = 0; j < map.Height(); j++)
        {
            for(int i = 0; i < map.Width(); i++)
            {
                if(map.isWall(i,j)){
                    levelMap.SetTile(new Vector3Int(i,-j,0), wall);
                } else if(map.isWater(i,j)){
                    levelMap.SetTile(new Vector3Int(i,-j,0), water);
                } else {
                    if(param.debug){
                        switch(map.mark(i,j)){
                            case AStar.REACHABLE:
                                levelMap.SetTile(new Vector3Int(i,-j,0), reachable );
                                break;
                            case AStar.EMPTY:
                                levelMap.SetTile(new Vector3Int(i,-j,0), ground );
                                break;
                            case AStar.SCANNED:
                                levelMap.SetTile(new Vector3Int(i,-j,0), scanned );
                                break;
                            case AStar.PATH:
                                levelMap.SetTile(new Vector3Int(i,-j,0), path );
                                break;
                            case AStar.NODES:
                                levelMap.SetTile(new Vector3Int(i,-j,0), nodes );
                                break;
                            case AStar.SELECTEDNODES:
                                levelMap.SetTile(new Vector3Int(i,-j,0), selectedNodes );
                                break;
                            default:
                                levelMap.SetTile(new Vector3Int(i,-j,0), ground );
                                break;

                        }
                    } else levelMap.SetTile(new Vector3Int(i,-j,0), ground );
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
        characterList[map.currentChar()].gameObject.GetComponent<SpriteRenderer>().color = Color.magenta;
    }


    void IObserver.Update(ISubject subject){
        if(param.debug) Paint();
        if(c.pause) {
            for(int i =0; i < map.nbChar(); i++) characterList[i].gameObject.GetComponent<SpriteRenderer>().color = Color.yellow;
        } else {
            for(int i =0; i < map.nbChar(); i++) characterList[i].gameObject.GetComponent<SpriteRenderer>().color = c.isSelectable(i) ? Color.yellow : Color.red;
        }
        characterList[map.currentChar()].gameObject.GetComponent<SpriteRenderer>().color = Color.magenta;
    }

    public void shift(float dX, float dY, int charNb) {
		characterList[charNb].position = new Vector3(map.CharacterX(charNb) +0.5f + dX, -map.CharacterY(charNb) +0.5f + dY, characterList[charNb].position.z);
	}

    void Update(){
        input.inputCheck();
        animSpeed = Mathf.Clamp(animSpeed, 0.01f, 1f);
        if(redraw){
            redraw=false;
            Paint();
        }
        
        if(param.lockCam)c.centerCam();
    }

    public Vector3 currentCharPos(){
        return characterList[map.currentChar()].gameObject.GetComponent<Transform>().position;
    }

    internal void updateTileSize()
    {
        tileSize = Screen.height/(2*cam.orthographicSize);
    }
}
