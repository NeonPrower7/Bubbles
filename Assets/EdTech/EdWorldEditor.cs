#if UNITY_EDITOR
using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace EdTech
{
    [CustomEditor(typeof(EdWorld))]
    public class EdWorldEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var world = (EdWorld)target;

            if (world.Levels != null)
            {
                foreach (var level in world.Levels)
                {
                    var def = level?.LevelDef;
                    if (def == null)
                        continue;
                    //if (GUILayout.Button($"Spawn {def.Name}"))
                    //{
                    //    world.SpawnLevelById(def.Id);
                    //}

                    if (GUILayout.Button($"Set {def.Name}"))
                    {
                        world.SpawnOnStart = level.name;
                        EditorUtility.SetDirty(world);
                        EditorSceneManager.MarkSceneDirty(world.gameObject.scene);
                    }
                }

                GUILayout.Space(10);
                GUILayout.Label("Previews");
                foreach (var level in world.Levels)
                {
                    var def = level.LevelDef;
                    if (def == null)
                        continue;
                    if (GUILayout.Button($"Preview {def.Name}"))
                    {
                        world.SpawnLevelPreviewStamps(level.LevelDef);
                    }
                }
            }

            GUILayout.Space(10);

            //if (GUILayout.Button("Reload Sprite Map"))
            //{
            //    world.SpriteMap = null;
            //    world.ResetSpriteCache();
            //    world.RebuildSpriteCacheFromSpriteMap();
            //}

            if (GUILayout.Button("Reload Defaults"))
            {
                LoadDefaults(world);
            }
            GUILayout.Space(10);

            GUILayout.Label($"Paused: {world.IsPaused}");
            DrawDefaultInspector();
        }

        
        [MenuItem("GameObject/EdTech/EdWorld", priority = 100)]
        public static void CreateEdWorld()
        {
            var edWorld = CreateEdWorldAndReturn();
            Selection.activeGameObject = edWorld.gameObject;
        }

        static void LoadDefaults(EdWorld world)
        {
            // Load SpriteMap from Assets
            {
                GUID.TryParse("e652406e44954aa4fa02b8d8ad7056ec", out var guid);
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var spriteMap = AssetDatabase.LoadAssetAtPath<SOSpriteMap>(path);
                world.SpriteMap = spriteMap;
                world.ResetSpriteCache();
                world.RebuildSpriteCacheFromSpriteMap();
            }

            // Load LevelDefs from Assets
            List<SOLevel> sol = new List<SOLevel>();
            foreach (var guid in EdWorldData.LevelGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var levelDef = AssetDatabase.LoadAssetAtPath<SOLevel>(path);
                sol.Add(levelDef);
            }
            world.Levels = sol.ToArray();
        }

        static EdWorld CreateEdWorldAndReturn()
        {
            var go = new GameObject("EdWorld");
            var world = go.AddComponent<EdWorld>();
            LoadDefaults(world);

            return world;
        }

        public static void SpawnLevel(string levelId)
        {
            var world = FindAnyObjectByType<EdWorld>();
            if (world == null)
                world = CreateEdWorldAndReturn();

            var go = world.SpawnLevelById(levelId);
            Selection.activeGameObject = go;
        }
    }
}
#endif
