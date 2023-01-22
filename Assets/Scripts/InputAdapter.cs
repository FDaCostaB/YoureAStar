using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputAdapter {
    public float animTime;
    private float time = 0;
    private Controller control;
    private GameZone gameZone;
    
    public InputAdapter(GameZone gz, Controller c){
        gameZone = gz;
        control = c;
        animTime = 0.0166f;
    }

    public void inputCheck(){
        time += Time.deltaTime;
        if(time >= animTime){
            control.clock();
            time=0;
        }
        if (Input.GetMouseButtonDown(0)){
            Vector3 mousePos = Input.mousePosition;
            Vector2 clicCase = new Vector2( (mousePos.x - Screen.width/2) / gameZone.tileSize, (-mousePos.y + Screen.height/2) / gameZone.tileSize);
            control.mouseClic( clicCase.x , clicCase.y );
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            control.up();
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            control.left();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            control.down();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            control.right();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            control.centerCam();
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            control.clear();
            control.tooglePause();
        }
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            control.clear();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            control.debug();
        }
        if((int) Input.mouseScrollDelta.y != 0){
            control.zoom((int) Input.mouseScrollDelta.y);
        }

    }
}
