using System;
using System.Linq;

using Unity.VisualScripting;

using UnityEngine;

namespace EdTech
{
    [Serializable]
    public class Tile : MonoBehaviour
    {
        public GameTileData Data = new GameTileData();

        public EdWorld World;
        public GameObject GOForeground { get; set; }
        public SpriteRenderer SRForeground { get; set; }
        public GameObject GOBackground { get; set; }
        public SpriteRenderer SRBackground { get; set; }
        public Collider2D Collider { get; set; }
        public BoxCollider2D BoxCollider { get; set; }
        public Rigidbody2D Rigidbody { get; set; }

        public void Update()
        {
            TileExtensions.Tick(this, Time.time);
        }
    }

    public static class TileExtensions
    {
        public static GameObject Spawn(Transform parent, EdTile tile, int z, EdBoard board, float levelHeight, EdWorld world)
        {
            var finalHandle = tile.FinalHandle;
            var tileDef = EdWorldData.GetTileDef(finalHandle);
            if (tileDef == null)
                return null;
            var tileName = tile?.Name;
            if (string.IsNullOrEmpty(tileName))
            {
                if (tileDef.TileFlags.HasFlag(EdTileFlags.NamedTileDef))
                    tileName = tileDef.Ref;
                else
                    tileName = tile.X + "," + tile.Y;
            }

            var rawSize = EdExtensions.GetTileSize(tileDef, board.TileWidth, board.TileHeight);
            var size = rawSize * EdWorldExtensions.UnitScale;

            var position = EdExtensions.GetTilePosition(
                board.TileWidth, 
                board.TileHeight, 
                tileDef.OffsetX, 
                tileDef.OffsetY, 
                board.GapWidth, 
                board.GapHeight, 
                tile.X, 
                tile.Y);
            position.x += rawSize.x * 0.5f;
            position.y = levelHeight - position.y - rawSize.y * 0.5f;
            var anchorFactors = tileDef.Anchor.GetAnchorFactors();
            anchorFactors.x *= rawSize.x;
            anchorFactors.y *= rawSize.y;
            var cellAnchorFactors = tileDef.CellAnchor.GetAnchorFactors();
            cellAnchorFactors.x *= board.TileWidth;
            cellAnchorFactors.y *= board.TileHeight;
            position.x -= anchorFactors.x + cellAnchorFactors.x;
            position.y += anchorFactors.y - cellAnchorFactors.y;
            position *= EdWorldExtensions.UnitScale;

            var collisionMode = EdExtensions.GetCollisionMode(tileDef, board, tile.Collision);
            return Spawn(parent, tileName, tileDef, position.x, position.y, z,
                size.x, size.y, board, levelHeight, world, collisionMode,
                tile.Tags, tile.Id, tile.ReplaceTileDefTags);
        }

        public static GameObject Spawn(
            Transform parent,
            string tileName,
            EdTileDef tileDef,
            float realX, float realY,
            int z,
            float width, float height,
            EdBoard board, float levelHeight, EdWorld world,
            EdTileCollisionMode tileCollisionMode,
            int[] tags,
            long id,
            bool replaceTileDefTags)
        {
            if (tileDef.TileFlags.HasFlag(EdTileFlags.CloneTileDef))
                tileDef = EdTileDef.Clone(tileDef);

            var go = new GameObject(tileName);
            go.transform.SetParent(parent);

            go.transform.localScale = new Vector3(1, 1, 1);

            var tile = go.AddComponent<Tile>();
            tile.World = world;

            ref var data = ref tile.Data;
            data.RealX = realX;
            data.RealY = realY;
            data.SpawnX = realX;
            data.SpawnY = realY;
            data.Width = width;
            data.Height = height;
            data.TileDef = tileDef;
            data.Board = board;
            data.SortingOrder = z;
            data.LevelHeight = levelHeight;
            data.Collision = tileCollisionMode;
            data.Pose = tileDef.Handle;

            var tileDefTags = tileDef.Tags ?? new int[0];
            var tileTags = tags ?? new int[0];
            data.Tags = replaceTileDefTags ? tileTags : tileTags.Union(tileDefTags).ToArray();
            data.Id = id;

            Initialize(tile);

            return go;
        }

        public static void Initialize(Tile tile)
        {
            if (tile == null) return;
            ref var data = ref tile.Data;

            var tileDef = data.TileDef;
            var board = data.Board;
            var world = tile.World;

            if (tileDef == null || board == null) return;

            var size = EdWorld.UnitScale * EdExtensions.GetTileSize(tileDef, board.TileWidth, board.TileHeight);
            data.Width = size.x;
            data.Height = size.y;

            if (tileDef.BackgroundColor > 0)
            {
                if (tile.GOBackground == null)
                {
                    var transform = tile.gameObject.transform.Find("Background");
                    if (transform != null)
                        tile.GOBackground = transform.gameObject;
                    else
                    {
                        tile.GOBackground = new GameObject("Background");
                        tile.GOBackground.transform.SetParent(tile.gameObject.transform);
                    }
                }

                if (tile.SRBackground == null)
                    tile.SRBackground = tile.GOBackground.AddOrGetComponent<SpriteRenderer>();

                tile.SRBackground.sortingOrder = data.SortingOrder * 2;
                {
                    var color = Color.white;
                    color.a = board.Opacity;
                    color = color * EdExtensions.GetColor(tileDef.BackgroundColor) * EdExtensions.GetColor(board.TintColor);
                    tile.SRBackground.color = color;
                }
            }
            else
            {
                if (tile.GOBackground == null)
                {
                    var transform = tile.gameObject.transform.Find("Background");
                    if (transform != null)
                        tile.GOBackground = transform.gameObject;
                }
                if (tile.GOBackground != null)
                {
                    GameObject.Destroy(tile.GOBackground);
                    tile.GOBackground = null;
                }
            }

            if (tile.GOForeground == null)
            {
                var transform = tile.gameObject.transform.Find("Foreground");
                if (transform != null)
                    tile.GOForeground = transform.gameObject;
                else
                {
                    tile.GOForeground = new GameObject("Foreground");
                    tile.GOForeground.transform.SetParent(tile.gameObject.transform);
                }
            }

            if (tile.SRForeground == null)
                tile.SRForeground = tile.GOForeground.AddOrGetComponent<SpriteRenderer>();

            tile.SRForeground.sortingOrder = data.SortingOrder * 2 + 1;
            {
                var color = Color.white;
                color.a = board.Opacity;
                color = color * EdExtensions.GetColor(tileDef.TintColor) * EdExtensions.GetColor(board.TintColor);
                tile.SRForeground.color = color;
            }

            tile.gameObject.transform.localPosition = new Vector3(data.RealX, data.RealY, 0);
            tile.gameObject.transform.localScale = new Vector3(size.x, size.y, 1);

            if (tileDef.TileFlags.HasFlag(EdTileFlags.Annotation))
            {
                if (tile.SRBackground != null)
                    tile.SRBackground.enabled = false;
                if (tile.SRForeground != null)
                    tile.SRForeground.enabled = false;
            }

            CreateAnimationInstance(tile);

            if (tileDef.TileFlags.HasFlag(EdTileFlags.CameraCorner))
            {
                tile.gameObject.layer = LayerMask.NameToLayer("Camera");
            }

            if (tileDef.TileFlags.HasFlag(EdTileFlags.CameraTarget))
            {
                var tCameraTarget = tile.transform.Find("CameraTarget");
                if (tCameraTarget == null)
                {
                    tCameraTarget = new GameObject("CameraTarget").transform;
                    tCameraTarget.SetParent(tile.transform);
                    tCameraTarget.localPosition = Vector3.zero;
                    tCameraTarget.gameObject.tag = "CameraTarget";
                }
                var camera = Camera.main;
                if (camera != null)
                    camera.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y, camera.transform.position.z);
            }
            else
            {
                var tCameraTarget = tile.transform.Find("CameraTarget");
                if (tCameraTarget != null)
                    GameObject.Destroy(tCameraTarget.gameObject);
            }

            if (tileDef.TileFlags.HasFlag(EdTileFlags.Player))
            {
                tile.gameObject.tag = "Player";
                tile.gameObject.layer = LayerMask.NameToLayer("Player");
            }

            // Collider            
            UpdateColliderFromTileDef(tile);

            // Initialize MonoBehaviors
            EdDataContainer.InitializeMonoBehaviours(tileDef.DataContainerId, tile.gameObject);

            // Initialize Variables
            EdDataContainer.SendVariableSetMessages(tileDef.DataContainerId, tile.gameObject);

            world.SendMessage("ReportNewTile", tile, SendMessageOptions.DontRequireReceiver);
            Camera.main.SendMessage("ReportNewTile", tile, SendMessageOptions.DontRequireReceiver);
        }

        public static void UpdateSizeFromTileDef(Tile tile)
        {
            if (tile == null) return;
            ref var data = ref tile.Data;
            var tileDef = data.TileDef;
            var board = data.Board;
            var size = EdWorld.UnitScale * EdExtensions.GetTileSize(tileDef, board.TileWidth, board.TileHeight);
            data.Width = size.x;
            data.Height = size.y;
            data.RealX += tileDef.OffsetX * EdWorld.UnitScale;
            data.RealY -= tileDef.OffsetY * EdWorld.UnitScale;
            tile.gameObject.transform.localScale = new Vector3(size.x, size.y, 1);
            tile.gameObject.transform.localPosition = new Vector3(data.RealX, data.RealY, tile.gameObject.transform.localPosition.z);
        }

        public static void UpdateColliderFromTileDef(Tile tile)
        {
            ref var data = ref tile.Data;
            var tileDef = data.TileDef;
            if (data.Collision == EdTileCollisionMode.None)
            {
                // No Collider
                if (tile.BoxCollider != null)
                    tile.BoxCollider = tile.GetComponent<BoxCollider2D>();
                if (tile.BoxCollider != null)
                {
                    GameObject.Destroy(tile.BoxCollider);
                    tile.BoxCollider = null;
                }
            }
            else
            {
                if (tile.BoxCollider == null)
                    tile.BoxCollider = tile.gameObject.AddOrGetComponent<BoxCollider2D>();
                tile.BoxCollider.isTrigger = data.Collision == EdTileCollisionMode.Trigger;
            }
            tile.Collider = tile.BoxCollider;
            if (tile.BoxCollider != null)
            {
                if (tileDef.ColliderPadding.IsZero())
                {
                    tile.BoxCollider.size = Vector2.one;
                    tile.BoxCollider.offset = new Vector2(0, 0);
                }
                else
                {
                    ref var padding = ref tileDef.ColliderPadding;
                    var bcSize = Vector2.one;
                    var realPaddingWidth = (1.0f - padding.Left - padding.Right) * bcSize.x;
                    var realPaddingHeight = (1.0f - padding.Top - padding.Bottom) * bcSize.y;
                    tile.BoxCollider.size = new Vector2(realPaddingWidth, realPaddingHeight);
                    var realOffsetX = (padding.Left - padding.Right) * 0.5f * bcSize.x;
                    var realOffsetY = (padding.Bottom - padding.Top) * 0.5f * bcSize.y;
                    tile.BoxCollider.offset = new Vector2(realOffsetX, realOffsetY);
                }
            }

            // RigidBody
            if (tileDef.PhysicsSimulationMode == EdPhysicsSimulationMode.None)
            {
                if (tile.Rigidbody != null)
                    tile.Rigidbody = tile.GetComponent<Rigidbody2D>();
                if (tile.Rigidbody != null)
                {
                    GameObject.Destroy(tile.Rigidbody);
                    tile.Rigidbody = null;
                }
            }
            else
            {
                if (tile.Rigidbody == null)
                    tile.Rigidbody = tile.gameObject.AddOrGetComponent<Rigidbody2D>();
                var rb = tile.Rigidbody;
                RigidbodyConstraints2D constraints = default;
                if (!tileDef.TileFlags.HasFlag(EdTileFlags.CanRotate))
                    constraints |= RigidbodyConstraints2D.FreezeRotation;
                if (tileDef.TileFlags.HasFlag(EdTileFlags.PositionLockX))
                    constraints |= RigidbodyConstraints2D.FreezePositionX;
                if (tileDef.TileFlags.HasFlag(EdTileFlags.PositionLockY))
                    constraints |= RigidbodyConstraints2D.FreezePositionY;
                rb.constraints = constraints;
                rb.bodyType = tileDef.PhysicsSimulationMode == EdPhysicsSimulationMode.Dynamic ? RigidbodyType2D.Dynamic :
                    tileDef.PhysicsSimulationMode == EdPhysicsSimulationMode.Kinematic ? RigidbodyType2D.Kinematic :
                    RigidbodyType2D.Static;
                rb.gravityScale = tileDef.GravityMultiplierY;
            }
        }

        public static void CreateAnimationInstance(Tile tile)
        {
            ref var data = ref tile.Data;
            var tileDef = data.TileDef;
            data.AnimationInstance = new AnimationInstance
            {
                Rate = tileDef.Animation.Rate,
                CurrentFrame = 0,
                FrameDurations = tileDef.Animation.KeyFrames.Select(kf => new Vector2(kf.DurationMin, kf.DurationMax)).ToArray(),
                IsPlaying = true,
                Mode = tileDef.Animation.Mode,
                FrameTime = UnityEngine.Random.value * tileDef.Animation.InitialTimeVariance,
                VirtualFrameNumber = 0
            };

            if (tileDef.Animation.KeyFrames.Length > 0)
            {
                SetSprite(tile, tileDef.Animation.KeyFrames[0]);
            }
        }

        public static void SetSprite(Tile tile)
        {
            if (tile == null) return;
            ref var data = ref tile.Data;
            var frame = data.TileDef.Animation.KeyFrames.FirstOrDefault();
            SetSprite(tile, frame);
        }

        public static EdSpriteKeyFrame? GetCurrentSpriteFrame(Tile tile)
        {
            if (tile == null) return null;
            ref var data = ref tile.Data;
            if (data.AnimationInstance == null)
                return data.TileDef.Animation.KeyFrames.FirstOrDefault();

            if (data.AnimationInstance.CurrentFrame >= data.TileDef.Animation.KeyFrames.Length)
                return data.TileDef.Animation.KeyFrames.LastOrDefault();

            return data.TileDef.Animation.KeyFrames[data.AnimationInstance.CurrentFrame];
        }

        public static void SetSprite(Tile tile, EdSpriteKeyFrame frame)
        {
            if (tile == null) return;

            var sprite = tile.World.GetUnitySpriteById(frame.SpriteId);
            var spriteW = 1.0f;
            var spriteH = 1.0f;
            if (sprite != null)
            {
                Vector2 originalSize = Vector2.one;
                Vector2 newSize = sprite.rect.size;
                spriteW = originalSize.x * sprite.pixelsPerUnit / newSize.x;
                spriteH = originalSize.y * sprite.pixelsPerUnit / newSize.y;
            }

            if (tile.SRForeground == null)
            {
                if (tile.GOForeground == null)
                {
                    var transform = tile.gameObject.transform.Find("Foreground");
                    if (transform != null)
                        tile.GOForeground = transform.gameObject;
                    else
                    {
                        tile.GOForeground = new GameObject("Foreground");
                        tile.GOForeground.transform.SetParent(tile.gameObject.transform);
                    }
                }

                tile.SRForeground = tile.GOForeground.AddOrGetComponent<SpriteRenderer>();
            }

            if (tile.SRForeground != null)
            {
                tile.SRForeground.sprite = sprite;
                var fx = frame.FlipX;
                if (tile.Data.SpriteFlipX) fx = !fx;
                tile.SRForeground.flipX = fx;
                fx = frame.FlipY;
                if (tile.Data.SpriteFlipY) fx = !fx;
                tile.SRForeground.flipY = fx;

                var scale = new Vector3(spriteW, spriteH, 1);
                if (!tile.Data.TileDef.TileFlags.HasFlag(EdTileFlags.SpriteAutoSizeOff))
                    tile.SRForeground.transform.localScale = scale;
            }
        }

        public static void Mimic(Tile me, EdTileDef toMimic, bool force = false)
        {
            if (toMimic == null && !force) return;
            if (me.Data.Pose == toMimic.Handle && !force) return;
            me.Data.AnimationInstance = null;
            me.Data.Pose = toMimic.Handle;
            EdTileDef.Mimic(me.Data.TileDef, toMimic);
            var oldSize = me.gameObject.transform.localScale;
            var oldCenter = me.gameObject.transform.localPosition;
            if (toMimic.TileFlags.HasFlag(EdTileFlags.MimicSize))
                UpdateSizeFromTileDef(me);
            if (toMimic.TileFlags.HasFlag(EdTileFlags.MimicCollider))
                UpdateColliderFromTileDef(me);
            else
            {
                if (me.BoxCollider != null)
                {
                    // scale collider size to match new scale retaining the old size
                    var newSize = me.gameObject.transform.localScale;
                    var scale = new Vector2(newSize.x / oldSize.x, newSize.y / oldSize.y);
                    me.BoxCollider.size = new Vector2(me.BoxCollider.size.x / scale.x, me.BoxCollider.size.y / scale.y);
                    me.BoxCollider.offset = new Vector2(me.BoxCollider.offset.x / scale.x, me.BoxCollider.offset.y / scale.y);
                }
            }
            CreateAnimationInstance(me);
        }

        public static void Mimic(Tile me, string toMimic)
        {
            if (uint.TryParse(toMimic, out var handle))
            {
                var tileDef = EdWorldData.GetTileDef(handle);
                if (tileDef != null)
                    Mimic(me, tileDef);
            }
            else
            {
                var tileDef = EdWorldData.GetTileDef(toMimic);
                if (tileDef != null)
                    Mimic(me, tileDef);
            }
        }

        public static GameObject Become(Tile me, EdTileDef toBecome)
        {
            return Spawn(me, toBecome, Vector2.zero, true);
        }

        public static long GetNextId(EdWorld world)
        {
            long id;
            do
            {
                id = UnityEngine.Random.Range(0, int.MaxValue);
            } while (world.TileIds.ContainsKey(id));
            return id;
        }

        public static GameObject Spawn(Tile me, EdTileDef toBecome, Vector2 shift, bool destroySelf)
        {
            var parent = me.transform.parent;
            var localX = me.transform.localPosition.x + shift.x;
            var localY = me.transform.localPosition.y + shift.y;
            var world = me.World;
            var tileName = me.name;

            ref var data = ref me.Data;
            var z = data.SortingOrder;
            var levelHeight = data.LevelHeight;
            var board = data.Board;
            var width = data.Width;
            var height = data.Height;

            if (destroySelf)
                GameObject.Destroy(me.gameObject);

            var id = destroySelf ? data.Id : GetNextId(world);
            var collisionMode = toBecome.CollisionMode;
            var go = Spawn(
                parent, tileName, toBecome, localX, localY, z,
                width, height, board, levelHeight, world, collisionMode,
                me.Data.Tags, id, true);

            return go;
        }

        public static void Become(Tile me, string toBecome)
        {
            if (uint.TryParse(toBecome, out var handle))
            {
                var tileDef = EdWorldData.GetTileDef(handle);
                if (tileDef != null)
                    Become(me, tileDef);
            }
            else
            {
                var tileDef = EdWorldData.GetTileDef(toBecome);
                if (tileDef != null)
                    Become(me, tileDef);
            }
        }

        public static void TickAnimation(Tile tile, float gameTime)
        {
            if (tile == null) return;
            ref var data = ref tile.Data;
            if (data.AnimationInstance?.IsPlaying != true)
                return;
            if (data.TileDef.TileFlags.HasFlag(EdTileFlags.Annotation))
                return;
            if (data.AnimationInstance.Mode == EdAnimationMode.None)
            {
                SetSprite(tile, data.TileDef.Animation.KeyFrames.FirstOrDefault());
                return;
            }

            var lastFrame = data.AnimationInstance.CurrentFrame;
            data.AnimationInstance.Tick(gameTime);
            if (lastFrame != data.AnimationInstance.CurrentFrame)
            {
                var length = data.TileDef.Animation.KeyFrames.Length;
                if (length == 0) return;
                if (length <= data.AnimationInstance.CurrentFrame)
                {
                    lastFrame = 0;
                    data.AnimationInstance.CurrentFrame = 0;
                }
                SetSprite(tile, data.TileDef.Animation.KeyFrames[data.AnimationInstance.CurrentFrame]);
            }
        }

        public static void Tick(Tile tile, float gameTime)
        {
            if (tile.World.IsPaused) return;

            if (tile.Data.AnimationInstance != null)
                TickAnimation(tile, Time.time);
        }

        public static void GarbageCollect(Tile tile)
        {
            if (tile == null) return;
            GameObject.Destroy(tile.gameObject);
        }

        public static void Ghost(Tile tile)
        {
            if (tile == null) return;
            GameObject.Destroy(tile);
        }
    }
}
