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
    public static void CacheLine(String mapName, int fromX, int fromY, int toX, int toY,  long computationTime, int nbScannedTiles, int openSetMaxSize, int pathLength, String method, OpenSetType setType, Heuristics heuristics, int  heuristicMultiplier)
    {
        Debug.Log(method + " - Computation time : " + computationTime + "\n# scanned cell : " + nbScannedTiles + "\nMax size of Open Set : " + openSetMaxSize + "\nPath length : " + pathLength);
        if (!Parameters.instance.debug) buffer += mapName + ", " + "("+fromX+" - "+fromY+")" + ", " + "("+toX+" - "+toY+")" + ", " + computationTime + ", " + nbScannedTiles + ", " + openSetMaxSize + ", " + pathLength + ", " + method + ", " + setType.ToString() + ", " + heuristics.ToString() + ", " + heuristicMultiplier.ToString() +"\n";
    }

    public static void flush(){
        string path = "Assets/Resources/Log.csv";
        StreamWriter writer = new StreamWriter(path, true);
        writer.Write(buffer);
        writer.Close();
        buffer = "";
    }
}