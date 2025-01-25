#if UNITY_EDITOR
using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace EdTech
{
    [CustomEditor(typeof(Tile))]
    public class TileEditor : Editor
    {
        string toMimic;

        static int jsonMode = 1;

        public override void OnInspectorGUI()
        {
            var tile = (Tile)target;

            DrawDefaultInspector();

            GUILayout.Space(10);
            toMimic = GUILayout.TextField(toMimic);
            if (GUILayout.Button("Mimic"))
            {
                TileExtensions.Mimic(tile, toMimic);
            }
            if (GUILayout.Button("Become"))
            {
                TileExtensions.Become(tile, toMimic);
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Initialize"))
            {
                TileExtensions.Initialize(tile);
            }

            if (GUILayout.Button("CreateAnimationInstance"))
            {
                TileExtensions.CreateAnimationInstance(tile);
            }

            if (GUILayout.Button("SetSprite"))
            {
                TileExtensions.SetSprite(tile);
            }

            if (GUILayout.Button("GarbageCollect"))
            {
                TileExtensions.GarbageCollect(tile);
            }

            if (GUILayout.Button("Ghost"))
            {
                TileExtensions.Ghost(tile);
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Hide Details"))
                jsonMode = 0;
            if (GUILayout.Button("Show Payload"))
                jsonMode = 1;
            if (GUILayout.Button("Show All"))
                jsonMode = 2;

            if (jsonMode == 2)
            {
                var data = JsonUtility.ToJson(tile.Data, true);
                GUILayout.TextArea(data);
            }
            else if (jsonMode == 1)
            {
                var data = JsonUtility.ToJson(tile.Data.Payload, true);
                GUILayout.TextArea(data);
            }
        }
    }
}
#endif
