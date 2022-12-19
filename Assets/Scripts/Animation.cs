using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Animation {
	int slowness;
	int counter;

	public Animation(int l) {
		slowness = l;
	}

	public abstract void update();

	public void tictac() {
		counter++;
		if (counter >= slowness) {
			update();
			counter = 0;
		}
	}

	public virtual bool IsFinished() {
		return false;
	}
}
