using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationMove : Animation {
	Move mv;
	bool stepFinished;
	bool isFinished;
	bool pause;
	float progress, animSpeed;
    GameZone gZone;
	Controller control;
	Step d;

	public AnimationMove(Move move, Controller c, GameZone gz) : base(1) { // 
		animSpeed = gz.animSpeed;
		mv = move;
        gZone = gz;
		control = c;
		stepFinished = true;
		isFinished = false;
		pause = false;
		d = null;
	}

	public override void update() {
		animSpeed = gZone.animSpeed;
		IEnumerator<Step> it = mv.Steps().GetEnumerator();
		if(stepFinished && it.MoveNext() && !isFinished && !pause){
			d = control.playStep(mv);
			stepFinished = false;
		}
		if(!stepFinished){
			float dX = (d.fromX - d.toX)*(1 - progress);
			float dY = (d.fromY - d.toY)*(1 - progress);
			gZone.shift(dX, -dY, mv.charNb);
		}
		if(!pause || !stepFinished){
			progress += animSpeed;
			if (progress > 1) {
				progress = 0;
				stepFinished = true;
				if(mv.Steps().Count == 0){
					isFinished = true;
					gZone.shift(0, 0, mv.charNb);
				}
			}
		}

	}

	public override bool IsFinished() {
		return isFinished;
	}

	public bool StepFinished() {
		return stepFinished;
	}


	public void stop(){
		pause = true;
	}

	public void start(){
		pause = false;
		progress = 0;
	}

}
