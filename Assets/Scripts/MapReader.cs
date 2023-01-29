using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MapReader {
	StreamReader reader;
	String fileName;

	public MapReader(string path) {
		reader = new StreamReader(path);
		fileName = path;
	}

	public Map read() {
		Map m = new Map(fileName);
		string line = null;
		int i=0;

		try {
			line = reader.ReadLine();
		} catch (Exception e) {
			Debug.LogError("ReadLine failed : " + e);
			Application.Quit();
            return null;
		}
		while (line.Length > 0) {
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
							Debug.LogError("Unknown character encounter while reading the map");
							Application.Quit();
							break;
					}
					m.setMark(AStar.EMPTY,x,i);
				}
			}
			line = reader.ReadLine();
			i++;
		}
		Debug.Log(m.Height());
		return m;
	}

}
