using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using Unity.VisualScripting;
using UnityEngine.UI;
using TMPro;
using Unity.Mathematics;

public class GameZone : MonoBehaviour, IObserver
{
    public Parameters param;
    public string[] mapsPath;
    public TextAsset mapFile;
    public GameObject pauseMenu;
    public GameObject benchmarkMenu;
    public TextMeshProUGUI xStart;
    public TextMeshProUGUI yStart;
    public TextMeshProUGUI xGoal;
    public TextMeshProUGUI yGoal;
    public TextMeshProUGUI computationTime;
    public TextMeshProUGUI pathLength;
    public TextMeshProUGUI tilesScan;
    public GameObject character;
    public Camera cam;
    public float animSpeed = 0.1f;
    public int camSpeed = 1;
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
    private Tile highlighted;
    private Tilemap levelMap;
    private Tilemap topLayer;
    private Vector2Int currHighlited;
    public float tileSize {get; private set;} 
    private InputAdapter input;
    private Controller c;

    private void Start(){
        param = new Parameters();
        mapFile = Resources.Load<TextAsset>(mapsPath[0]);
        Debug.Log(mapsPath[0]);
        Debug.Log(mapFile);
        characterList = new List<Transform>();
		MapReader mapReader = new MapReader(mapFile);
		map = mapReader.read();
        map.Attach(this);
        updateStart(map.CharacterX(), map.CharacterY());
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
        water = TilesResourcesLoader.GetTileByName("Water");
        highlighted = TilesResourcesLoader.GetTileByName("Highlighted");
        input = new InputAdapter(this,c);
        levelMap = GetComponentsInChildren<Tilemap>()[0];
        topLayer = GetComponentsInChildren<Tilemap>()[1];
        CursorController.instance.SetNormal();
        Data.Init();
        Paint(true);
    }

    public void updateMapPath(int value)
    {
        mapFile = (TextAsset)Resources.Load(mapsPath[value], typeof(TextAsset));
    }

    public void readPaths()
    {
        c.readPaths();
    }

    public void loadMap()
    {
        CursorController.instance.SetLoading();
        c.cancelMove();
        MapReader mapReader = new MapReader(mapFile);
        map = mapReader.read();
        map.Attach(this);
        c = new Controller(map, cam, this);
        input = new InputAdapter(this, c);
        Debug.Log("Map size : " + map.Width() + "," + map.Height());
        Paint(true);
        CursorController.instance.SetNormal();
    }

    public void selectAlgo(int value)
    {
        switch (value)
        {
            case 0:
                Parameters.instance.useSubgoal = false;
                Parameters.instance.useTL = false;
                Parameters.instance.useHPA = false;
                c.UpdateNodes();
                break; 
            case 1:
                Parameters.instance.useSubgoal = true;
                Parameters.instance.useTL = false;
                Parameters.instance.useHPA = false;
                c.UpdateNodes();
                break;
            case 2:
                Parameters.instance.useSubgoal = true;
                Parameters.instance.useTL = true;
                Parameters.instance.useHPA = false;
                c.UpdateNodes();
                break;
            case 3:
                Parameters.instance.useSubgoal = false;
                Parameters.instance.useTL = false;
                Parameters.instance.useHPA = true;
                c.UpdateNodes();
                break;
            default: 
                break;      
        }
        CursorController.instance.SetLoading();
        map.Notify();
        CursorController.instance.SetNormal();
    }

    public void updateHeuristics(int value)
    {
        Debug.Log(value);
        switch (value)
        {
            case 0:
                Parameters.instance.heuristic = Heuristics.Manhattan;
                break;
            case 1:
                Parameters.instance.heuristic = Heuristics.Euclidean;
                break;
            case 2:
                Parameters.instance.heuristic = Heuristics.Chebyshev;
                break;
            case 3:
                Parameters.instance.heuristic = Heuristics.Octile;
                break;
            case 4:
                Parameters.instance.heuristic = Heuristics.Djikstra;
                break;
            default:
                break;
        }
    }

    public void updateBidirectionnal(bool value)
    {
        Parameters.instance.doBidirectionnal= value;
    }

    public void toogleBenchmark()
    {
        benchmarkMenu.SetActive(!benchmarkMenu.activeInHierarchy);
    }

    public void updateDebug(bool value)
    {
        CursorController.instance.SetLoading();
        Parameters.instance.debug = value;
        Paint();
        CursorController.instance.SetNormal();
    }

    public void updateCamSpeed(float value)
    {
        camSpeed = (int) value;
    }

    public void updateAnimSpeed(float value)
    {
        animSpeed = Mathf.Clamp(value, 0.01f, 1f);
    }

    public void tooglePause(bool pause)
    {
        c.clear();
        c.tooglePause();
        c.cancelMove();
        if (pause)
        {
            pauseMenu.active = true;
            pauseMenu.transform.localPosition = new float3(-400, -225, 0); 
        }
        else
        {
            pauseMenu.active = false;
            pauseMenu.transform.position = new float3(-190, -225, 0);
        }
    }

    public void updateHeuristicMult(float value)
    {
        Parameters.instance.heuristicMultiplier = (int)value;
    }

    public void updateLayerNb(String value)
    {
        Parameters.instance.layerNb = int.Parse(value);
        CursorController.instance.SetLoading();
        c.RebuildHPA();
        CursorController.instance.SetNormal();
    }

    public void updateClusterSize(String value)
    {
        Parameters.instance.clusterSize = int.Parse(value);
        CursorController.instance.SetLoading();
        c.RebuildHPA();
        CursorController.instance.SetNormal();
    }

    public void updateStart(int x, int y)
    {
        xStart.text = x.ToString();
        yStart.text = y.ToString();
    }

    public void updateGoal(int x, int y)
    {
        xGoal.text = x.ToString();
        yGoal.text = y.ToString();
    }
    public void displayDebug(Move m)
    {
        computationTime.text = m.time.ToString()+" ms";
        pathLength.text = m.Steps().Count.ToString() +" steps";
        tilesScan.text = m.scanned.ToString()+" tiles";
    }

    public void displayTimedOut()
    {
        computationTime.text = "Timed out";
        pathLength.text = "Timed out";
        tilesScan.text = "Timed out";
    }

    private void Paint(bool initPaint = false){
        if (initPaint)
        {
            levelMap.ClearAllTiles();
            foreach(Transform character in characterList)
            {
                Destroy(character.gameObject);
            }
            characterList.Clear();

        }
        for(int j = 0; j < map.Height(); j++)
        {
            for(int i = 0; i < map.Width(); i++)
            {
                if(map.isWall(i,j)){
                    levelMap.SetTile(new Vector3Int(i,-j,0), wall);
                } else if(map.isWater(i,j)){
                    levelMap.SetTile(new Vector3Int(i,-j,0), water);
                } else {
                    if(Parameters.instance.debug){
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
                                if (Parameters.instance.useSubgoal || Parameters.instance.useHPA) levelMap.SetTile(new Vector3Int(i, -j, 0), nodes);
                                else levelMap.SetTile(new Vector3Int(i, -j, 0), reachable);
                                break;
                            case AStar.SELECTEDNODES:
                                levelMap.SetTile(new Vector3Int(i, -j, 0), selectedNodes);
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
        if(Parameters.instance.debug) Paint();
        if(Parameters.instance.pause) {
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
        if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0) {
            Vector3 mousePos = Input.mousePosition;
            Vector2 clicCase = new Vector2((mousePos.x - Screen.width / 2) / tileSize, (-mousePos.y + Screen.height / 2) / tileSize);
            c.hover(clicCase.x, clicCase.y);
        }

        if (Parameters.instance.lockCam)c.centerCam();
    }

    public Vector3 currentCharPos(){
        return characterList[map.currentChar()].gameObject.GetComponent<Transform>().position;
    }

    internal void updateTileSize()
    {
        tileSize = Screen.height/(2*cam.orthographicSize);
    }

    internal void highlightTile(int i, int j)
    {
        if (currHighlited.x == i && currHighlited.y == j) return;
        updateGoal(i, j);
        topLayer.SetTile(new Vector3Int(currHighlited.x, -currHighlited.y, -1), null);
        topLayer.SetTile(new Vector3Int(i, -j, -1), highlighted);
        currHighlited.x = i;
        currHighlited.y = j;
    }
}
