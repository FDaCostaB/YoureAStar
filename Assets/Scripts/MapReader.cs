using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MapReader {
	TextAsset mapFile;
	string name;

	public MapReader(TextAsset map) {
		mapFile = map;
		name = map.name;
	}

	public Map read() {
		Map m = new Map(name);
		string line = null;
		int i=0;
        List<string> lines = new List<string>(mapFile.text.Split('\n'));

		line = lines[0];
		while ( i < lines.Count) {
			if (line[0] == ';') {
				return m;
			} else {
				for (int x=0; x<line.Length; x++) {
					switch(line[x]) {
						case '#':
						case '@':
						case 'O':
						case 'T':
							m.addWall(x,i);
							break;
						case 'X':
							m.addCharacter(x,i);
							break;
						case '%':
						case 'W':
							m.addWater(x,i);
							break;
						case ' ':
						case '$':
						case '.':
						case 'G':
						case 'S':
							m.add(0,x,i);
							break;
						default:
							break;
					}
					m.setMark(AStar.EMPTY,x,i);
				}
			}
			line = lines[i];
			i++;
		}
		Debug.Log(m.Height());
		return m;
	}

}
