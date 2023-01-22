using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public static class Data
{
    public static void WriteLine(String mapName, int graphSize, int buildTime, int fromX, int fromY, int toX, int toY,  int computationTime,int nbScannedTiles, int openSetMaxSize, int pathLength, String method, OpenSetType setType, Heuristics heuristics)
    {
        string path = "Assets/Resources/Log.csv";
        StreamWriter writer = new StreamWriter(path, true);
        String Line = mapName + ", " + graphSize+ ", " + buildTime + ", " + "("+fromX+" - "+fromY+")" + ", " + "("+toX+" - "+toY+")" + ", " + computationTime + ", " + nbScannedTiles + ", " + openSetMaxSize + ", " + pathLength + ", " + method + ", " + setType.ToString() + ", " + heuristics.ToString();
        writer.WriteLine(Line);
        Debug.Log(method + " - Computation time : " + computationTime + "\n# scanned cell : " + nbScannedTiles + "\nMax size of Open Set : " + openSetMaxSize + "\nPath length : " + pathLength);
        writer.Close();
    }
}