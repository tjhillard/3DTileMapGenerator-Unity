using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (TileMapGenerator))]
public class MapEditor : Editor {

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        TileMapGenerator map = target as TileMapGenerator;

        if (GUILayout.Button("Generate TileMap"))
        {
            map.GenerateMap();
        }
       
    }
}
