using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public static class Data
{
    private static String buffer;

    public static void Init(){
        buffer = "";
    }
    public static void CacheLine(String mapName, int vertexCount, int edgeCount, int buildTime, int fromX, int fromY, int toX, int toY,  long computationTime, int nbScannedTiles, int openSetMaxSize, int pathLength, String method, OpenSetType setType, Heuristics heuristics)
    {
        Debug.Log(method + " - Computation time : " + computationTime + "\n# scanned cell : " + nbScannedTiles + "\nMax size of Open Set : " + openSetMaxSize + "\nPath length : " + pathLength);
        buffer += mapName + ", " + vertexCount+ ", " + edgeCount + ", " + buildTime + ", " + "("+fromX+" - "+fromY+")" + ", " + "("+toX+" - "+toY+")" + ", " + computationTime + ", " + nbScannedTiles + ", " + openSetMaxSize + ", " + pathLength + ", " + method + ", " + setType.ToString() + ", " + heuristics.ToString() +"\n";
    }

    public static void flush(){
        string path = "Assets/Resources/Log.csv";
        StreamWriter writer = new StreamWriter(path, true);
        writer.Write(buffer);
        writer.Close();
    }
}