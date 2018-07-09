using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainManager))]
public class TerrainManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TerrainManager myScript = (TerrainManager)target;

        GUILayout.Label("Zoom level");
        int zoom = int.Parse(GUILayout.TextField("0"));

        GUILayout.BeginHorizontal();
        GUILayout.Label("X From");
        int fromX = int.Parse(GUILayout.TextField("0"));
        GUILayout.Label("To");
        int toX = int.Parse(GUILayout.TextField("0"));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Y From");
        int fromY = int.Parse(GUILayout.TextField("0"));
        GUILayout.Label("To");
        int toY = int.Parse(GUILayout.TextField("0"));
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Load Terrain"))
        {
            myScript.LoadTerrain(zoom, fromX, fromY, toX, toY);
        }
    }

}
