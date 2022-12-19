using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MapReader {
	StreamReader reader;


	public MapReader(string path) {
		reader = new StreamReader(path); 
	}

	public Map read() {
		Map m = new Map();
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
						m.addWall(x,i);
						break;
					case '@':
						m.addCharacter(x,i);
						break;
					case ' ':
						break;
					default:
						Debug.LogError("Unknown character encounter while reading the map");
						Application.Quit();
                        break;
					}
				}
			}
			line = reader.ReadLine();
			i++;
		}
		return m;
	}

}
