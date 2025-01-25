using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EdTech
{
    public partial class EdWorld : MonoBehaviour
    {
        [HideInInspector] [NonSerialized] public Dictionary<long, Tile> TileIds = new Dictionary<long, Tile>();

        public const float UnitScale = EdWorldExtensions.UnitScale;

        public string SpawnOnStart = "";
        public Action<EdWorld> PostStart;

        public static EdWorld _instance;
        public static EdWorld Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<EdWorld>();
                    if (_instance == null)
                    {
                        var go = new GameObject("EdWorld");
                        _instance = go.AddComponent<EdWorld>();
                    }
                }
                return _instance;
            }
        }

        public SOLevel[] Levels;
        public SpriteMapItem[] OverrideSpriteMap;
        public SOSpriteMap SpriteMap;
        [HideInInspector] public Dictionary<string, Sprite> _spriteIdMap;
        [HideInInspector] public Dictionary<string, Sprite> _overrideSpriteIdMap;
        public Func<Transform, EdTile, int, EdBoard, GameObject> CustomSpawnTileFunc;

        public bool IsPaused { get; set; }

        private void Awake()
        {
            EdWorldExtensions.Awake(this);
        }

        public void Start()
        {
            EdWorldExtensions.Start(this);
            PostStart?.Invoke(this);
        }
    }

    public static class EdWorldExtensions
    {
        public const float UnitScale = 0.02f;

        public static void Awake(EdWorld world)
        {
            if (EdWorld._instance == null)
                EdWorld._instance = world;
            else if (EdWorld._instance != world)
                GameObject.Destroy(world.gameObject);

            if (world._spriteIdMap == null) RebuildSpriteCacheFromSpriteMap(world);
        }

        public static void Start(EdWorld world)
        {
            ResetSpriteCache(world);
            RebuildSpriteCacheFromSpriteMap(world);
            if (!string.IsNullOrWhiteSpace(world.SpawnOnStart))
            {
                SpawnLevelById(world, world.SpawnOnStart);
            }
        }

        public static Sprite GetUnitySpriteById(this EdWorld world, string id)
        {
            if (id == null) return null;
            if (world._overrideSpriteIdMap == null) RebuildOverrideSpriteIdMap(world);
            if (world._overrideSpriteIdMap.TryGetValue(id, out var sprite))
                return sprite;
            if (world._spriteIdMap == null) RebuildSpriteCache(world);
            if (world._spriteIdMap.TryGetValue(id, out sprite))
                return sprite;
            return null;
        }

        public static void RebuildOverrideSpriteIdMap(this EdWorld world)
        {
            var map = new Dictionary<string, Sprite>();
            if (world.OverrideSpriteMap == null) world.OverrideSpriteMap = new SpriteMapItem[0];
            foreach (var spriteMapItem in world.OverrideSpriteMap)
            {
                map[spriteMapItem.Id] = spriteMapItem.Sprite;
            }
            world._overrideSpriteIdMap = map;
        }

        public static void ResetSpriteCache(this EdWorld world)
        {
            world._spriteIdMap = null;
        }

        public static void RebuildSpriteCacheFromSpriteMap(this EdWorld world)
        {
            world._spriteIdMap = new Dictionary<string, Sprite>();
            if (world.SpriteMap?.Items == null) return;
            foreach (var spriteMapItem in world.SpriteMap.Items)
            {
                world._spriteIdMap[spriteMapItem.Id] = spriteMapItem.Sprite;
            }
        }

        public static void RebuildSpriteCache(this EdWorld world)
        {
            if (world.SpriteMap?.Items != null && world.SpriteMap.Items.Length > 0)
            {
                RebuildSpriteCacheFromSpriteMap(world);
                return;
            }
        }

        public static GameObject Spawn(this EdWorld world, Transform parent, EdTile tile, int z, EdBoard board, float levelHeight)
        {
            if (world.CustomSpawnTileFunc != null)
                return world.CustomSpawnTileFunc(parent, tile, z, board);

            return TileExtensions.Spawn(parent, tile, z, board, levelHeight, world);
        }

        public static GameObject Spawn(this EdWorld world, Transform parent, EdBoard board, int z, float levelHeight)
        {
            var instance = new GameObject(board.Name);
            instance.transform.SetParent(parent);
            instance.transform.localPosition = new Vector3(0, 0);
            instance.AddComponent<Board>();
            
            if (board.Walls != null)
            {
                var polygonCollider = instance.AddComponent<PolygonCollider2D>();
                polygonCollider.useDelaunayMesh = true;
                polygonCollider.pathCount = board.Walls.Length;
                for (int i = 0; i < board.Walls.Length; i++)
                {
                    var points = board.Walls[i].Polygon.Select(p => new Vector2(
                        p.x * UnitScale,
                        (levelHeight - p.y) * UnitScale
                        )).ToArray();
                    polygonCollider.SetPath(i, points);
                }
            }

            foreach (var tile in board.Tiles)
            {
                var tileInstance = Spawn(world, instance.transform, tile, z, board, levelHeight);
                if (tileInstance != null)
                    tileInstance.transform.SetParent(instance.transform);
            }
            
            if (board.DataContainerId > 0)
            {
                EdDataContainer.InitializeMonoBehaviours(board.DataContainerId, instance);
                EdDataContainer.SendVariableSetMessages(board.DataContainerId, instance);
            }

            instance.SendMessage("BoardSpawned", board, SendMessageOptions.DontRequireReceiver);

            return instance;
        }

        public static GameObject Spawn(this EdWorld world, Transform parent, EdQuad quad, int z, float levelHeight)
        {
            var sprite = GetUnitySpriteById(world, quad.SpriteId);
            if (sprite == null) return null;

            var cropWidth = sprite.bounds.size.x * sprite.pixelsPerUnit;
            var cropHeight = sprite.bounds.size.y * sprite.pixelsPerUnit;
            float tileX = cropWidth;
            float tileY = cropHeight;

            var instance = new GameObject(quad.Name);
            instance.transform.SetParent(parent);
            instance.transform.localScale = new Vector3(quad.Width * UnitScale, quad.Height * UnitScale, 1);
            //var position = new Vector3(quad.X + quad.Width * 0.5f * UnitScale, levelHeight - quad.Height - quad.Y + quad.Height * 0.5f * UnitScale, 0);
            var position = new Vector3(quad.X + quad.Width * 0.5f, levelHeight - quad.Y - quad.Height * 0.5f, 0);
            //position.x += quad.Width * 0.5f;
            position *= UnitScale;
            instance.transform.localPosition = position;

            var srGo = new GameObject("SpriteRenderer");
            srGo.transform.SetParent(instance.transform);
            srGo.transform.localPosition = new Vector3(0, 0);
            var spriteRenderer = srGo.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;

            if (quad.Tile == EdTileMode.Tile)
            {
                spriteRenderer.drawMode = SpriteDrawMode.Tiled;
                if (quad.TileX > 0)
                {
                    if (quad.TileRelativeCoords)
                        tileX = quad.Width / (float)quad.TileX;
                    else
                        tileX = (float)quad.TileX;
                }
                if (quad.TileY > 0)
                {
                    if (quad.TileRelativeCoords)
                        tileY = quad.Height / (float)quad.TileY;
                    else
                        tileY = (float)quad.TileY;
                }
                spriteRenderer.size = new Vector2(tileX, tileY);
                spriteRenderer.transform.localScale = new Vector3(1.0f / tileX, 1.0f / tileY, 1);
            }
            else
            {
                var collider = instance.AddComponent<BoxCollider2D>();
                Vector2 colliderSize = collider.size;
                Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
                Vector3 scale = Vector3.one;// spriteSubObject.transform.localScale;
                scale.x = colliderSize.x / spriteSize.x;
                scale.y = colliderSize.y / spriteSize.y;
                srGo.transform.localScale = scale;
                GameObject.DestroyImmediate(collider);
            }
            spriteRenderer.sortingOrder = z * 2 + 1;
            var color = EdExtensions.GetColor(quad.Tint);
            color.a *= quad.Opacity;
            spriteRenderer.color = color;
            spriteRenderer.enabled = quad.IsVisible;
            return instance;
        }

        public static GameObject SpawnLevelPreviewStamps(this EdWorld world, EdLevel level)
        {
            if (level?.PreviewStamps == null) return default;
            if (level.PreviewStamps.Length == 0) return default;
            var instance = new GameObject($"Previews {level.Name}");
            instance.AddComponent<DieOnStart>();

            var z = 0;
            foreach (var stamp in level.PreviewStamps)
            {
                var stampQuad = ToQuad(stamp, null);
                var stampInstance = Spawn(world, instance.transform, stampQuad, z++, (float)level.Height);
                if (stampInstance != null)
                    stampInstance.transform.SetParent(instance.transform);
            }

            return instance;
        }

        public static GameObject Spawn(this EdWorld world, EdLevel level)
        {
            if (level == null) return default;
            level = level.FlattenLevel();

            var instance = new GameObject(level.Name);
            //instance.transform.SetParent(transform);
            instance.transform.localPosition = new Vector3(0, 0);
            int z = -1;
            foreach (var thing in level.Things)
            {
                z++;
                if (thing is EdBoard board)
                {
                    if (board.Stamps != null)
                    {
                        foreach (var stamp in board.Stamps)
                        {
                            var stampQuad = ToQuad(stamp, board);
                            var stampInstance = Spawn(world, instance.transform, stampQuad, z++, (float)level.Height);
                            if (stampInstance != null)
                                stampInstance.transform.SetParent(instance.transform);
                        }
                    }

                    var boardInstance = Spawn(world, instance.transform, board, z, (float)level.Height);
                    if (boardInstance != null)
                        boardInstance.transform.SetParent(instance.transform);
                }
                else if (thing is EdQuad quad)
                {
                    var quadInstance = Spawn(world, instance.transform, quad, z, (float)level.Height);
                    if (quadInstance != null)
                        quadInstance.transform.SetParent(instance.transform);
                }
            }

            if (level.DataContainerId > 0)
            {
                EdDataContainer.InitializeMonoBehaviours(level.DataContainerId, instance);
                EdDataContainer.SendVariableSetMessages(level.DataContainerId, instance);
            }

            instance.SendMessage("LevelSpawned", level, SendMessageOptions.DontRequireReceiver);
            return instance;
        }

        public static GameObject SpawnLevelByIndex(this EdWorld world, int index)
        {
            var level = world?.Levels.ElementAtOrDefault(index)?.LevelDef;
            if (level == null) return null;
            return Spawn(world, level);
        }

        public static GameObject SpawnLevelById(this EdWorld world, string id)
        {
            var soLevel = world.Levels.FirstOrDefault(x => x?.LevelDef?.Id == id) ??
                world.Levels.FirstOrDefault(x => x?.LevelDef?.Name == id) ??
                world.Levels.FirstOrDefault(x => x?.name == id);

            var level = soLevel?.LevelDef;
            if (level == null) return default;
            return Spawn(world, level);
        }

        public static EdQuad ToQuad(BoardStamp stamp, EdBoard board)
        {
            return new EdQuad
            {
                SpriteId = stamp.SpriteId,
                X = stamp.X,
                Y = stamp.Y,
                Width = stamp.W,
                Height = stamp.H,
                Id = stamp.SpriteId,
                Name = stamp.SpriteId,
                Opacity = board?.Opacity ?? 1,
                Tint = board?.TintColor ?? EdTileDef.DefaultTintColor,
                BitmapBlendingModeId = board?.BlendModeId ?? default,
                Tile = EdTileMode.Stretch,
                ParallaxX = board?.ParallaxX ?? 1,
                ParallaxY = board?.ParallaxY ?? 1,
                IsVisible = board?.IsVisible ?? true,
            };
        }
    }
}
