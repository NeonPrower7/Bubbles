using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using EdTech;
using Unity.VisualScripting;

namespace EdTech
{
    public enum EdAnimationMode
    {
        None,
        Loop,
        PingPong,
        Once
    }
    
    public enum EdTileCollisionMode
    {
        None = 0,
        Solid = 1,
        Trigger = 2,
        OneWay = 3,
        OptimizedWall = 4
    }
    
    public enum EdTileCollisionOverride
    {
        Default = 0,
        Ignore = 2,
        Wall = 1,
        OptimizedWall = 3
    }

    [Serializable]
    public abstract class EdRecord
    {
        public string Id;
    }

    [Serializable]
    public class EdAsset : EdRecord
    {
        public uint Handle;
        public string Ref;
    }

    [Serializable]
    public class EdBoard : EdThing
    {
        public int Width;
        public int Height;
        public EdTile[] Tiles;
        public uint BackgroundColor;
        public float Opacity;
        public uint TintColor;
        public int BlendModeId;
        public float GapWidth;
        public float GapHeight;
        public float TileWidth;
        public float TileHeight;
        public EdTileCollisionOverride Collision;
        public BoardStamp[] Stamps;
        public PolygonCollection[] Walls;
        public long DataContainerId;
    }
    
    [Serializable]
    public class BoardStamp
    {
        public float X;
        public float Y;
        public float W;
        public float H;
        public string SpriteId;
    }

    [Serializable]
    public class PolygonCollection
    {
        public Vector2[] Polygon;

        public PolygonCollection(params float[] coords)
        {
            Polygon = new Vector2[coords.Length / 2];
            for (var i = 0; i < coords.Length; i += 2)
                Polygon[i / 2] = new Vector2(coords[i], coords[i + 1]);
        }

        public PolygonCollection()
        {
            Polygon = new Vector2[0];
        }
    }

    [Serializable]
    public partial class EdLevel : EdRecord
    {
        public string Name;
        public double Width;
        public double Height;
        public EdThing[] Things;
        public EdBoard[] Boards;
        public EdQuad[] Quads;
        public string[] Ids;
        public BoardStamp[] PreviewStamps;
        public long DataContainerId;
    }

    [Serializable]
    public class EdQuad : EdThing
    {
        public string SpriteId;
        public float Width;
        public float Height;
        public EdTileMode Tile;
        public float TileX;
        public float TileY;
        public bool TileRelativeCoords;
        public float U;
        public float V;
        public uint Fill;
        public uint Tint;
        public float Opacity;
        public int BitmapBlendingModeId;
        public bool FlipX;
        public bool FlipY;
    }

    [Serializable]
    public class EdSprite : EdAsset
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public string UnityName;
    }

    [Serializable]
    public struct EdSpriteKeyFrame
    {
        public string SpriteId;
        public float DurationMin;
        public float DurationMax;
        public bool FlipX;
        public bool FlipY;

        public EdSpriteKeyFrame(string spriteId, float durationMin, float durationMax, bool flipX, bool flipY)
        {
            SpriteId = spriteId;
            DurationMin = durationMin;
            DurationMax = durationMax;
            FlipX = flipX;
            FlipY = flipY;
        }

        public EdSpriteKeyFrame(string spriteId, float durationMin, float durationMax)
        {
            SpriteId = spriteId;
            DurationMin = durationMin;
            DurationMax = durationMax;
            FlipX = false;
            FlipY = false;
        }

        public EdSpriteKeyFrame(string spriteId, float duration)
        {
            SpriteId = spriteId;
            DurationMin = duration;
            DurationMax = duration;
            FlipX = false;
            FlipY = false;
        }

        public EdSpriteKeyFrame(string spriteId)
        {
            SpriteId = spriteId;
            DurationMin = 1;
            DurationMax = 1;
            FlipX = false;
            FlipY = false;
        }
    }

    [Serializable]
    public class EdSpriteSheet : EdAsset
    {
        public string UnityGuid;
        public string RelativePath;
        public EdSprite[] Sprites;
    }

    [Serializable]
    public abstract class EdThing : EdRecord
    {
        public float X;
        public float Y;
        public string Name;
        public bool IsVisible;
        public float ParallaxX;
        public float ParallaxY;
    }

    [Serializable]
    public class EdTile
    {
        public int X;
        public int Y;
        public int[] Tags;
        public uint FinalHandle;
        public string Name;
        public EdTileCollisionOverride Collision;
        public long Id;
        public bool ReplaceTileDefTags;

        public EdTile()
        {
        }

        public EdTile(int x, int y, uint finalHandle, string name, int iCollisionOverride, int[] tags = null, long id = 0, bool replaceTileDefTags = false)
		{
			X = x;
			Y = y;
			FinalHandle = finalHandle;
            Name = name;
            Collision = (EdTileCollisionOverride)(iCollisionOverride);
            Tags = tags;
            Id = id;
            ReplaceTileDefTags = replaceTileDefTags;
		}
    }

    [Serializable]
    public enum TileAnchor
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    [Serializable]
    public struct EdTileAnimation
    {
        public EdAnimationMode Mode;
        public EdSpriteKeyFrame[] KeyFrames;
        public float Rate;
        public float InitialTimeVariance;

        public static EdTileAnimation Clone(EdTileAnimation animation)
        {
            var clone = new EdTileAnimation
            {
                Mode = animation.Mode,
                Rate = animation.Rate,
                InitialTimeVariance = animation.InitialTimeVariance
            };

            if (animation.KeyFrames != null)
            {
                clone.KeyFrames = new EdSpriteKeyFrame[animation.KeyFrames.Length];
                for (var i = 0; i < animation.KeyFrames.Length; i++)
                    clone.KeyFrames[i] = animation.KeyFrames[i];
            }

            return clone;
        }
    }

    [Serializable]
    public class EdDataContainer
    {
        public Dictionary<string, string> Variables = new Dictionary<string, string>();
        public Dictionary<Type, Action<object>> ComponentInitializers = new Dictionary<Type, Action<object>>();
        public Dictionary<Type, Action<object>> MonoBehaviourInitializers = new Dictionary<Type, Action<object>>();

        public void InitializeMonoBehaviours(GameObject go)
		{
			if (MonoBehaviourInitializers == null) return;

			foreach (var pair in MonoBehaviourInitializers)
			{
				var component = go.GetComponent(pair.Key);
				if (component == null)
                    component = go.AddComponent(pair.Key);

				pair.Value(component);
			}
		}

        public static void InitializeMonoBehaviours(long containerId, GameObject go)
        {
            if (EdWorldData.DataContainers.TryGetValue(containerId, out var container))
                container.InitializeMonoBehaviours(go);
        }

        public static void SendVariableSetMessages(long containerId, GameObject go)
        {
            if (EdWorldData.DataContainers.TryGetValue(containerId, out var container))
            {
                if (container.Variables == null) return;
                foreach (var pair in container.Variables)
                    go.SendMessage("Set" + pair.Key, pair.Value, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    [Serializable]
    public struct RectF
    {
        public float Left;
        public float Top;
        public float Right;
        public float Bottom;

        public RectF(float left, float top, float right, float bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public readonly bool IsZero() => Left == 0 && Top == 0 && Right == 0 && Bottom == 0;
        public readonly bool HasValue() => Left != 0 || Top != 0 || Right != 0 || Bottom != 0;
        public readonly override string ToString() => $"({Left}, {Top}, {Right}, {Bottom})";
    }

    [Serializable]
    public class EdTileDef : EdAsset
    {
        public const uint DefaultTintColor = 4294967295u;

        public string Name;
        public string Script;
        public bool OverrideSize;
        public float OverrideWidth;
        public float OverrideHeight;
        public float OffsetX;
        public float OffsetY;
        public float RowSpan = 1;
        public float ColSpan = 1;
        public uint BackgroundColor;
        public uint TintColor = DefaultTintColor;
        public EdTileCollisionMode CollisionMode;
        public EdTileFlags TileFlags;
        public EdPhysicsSimulationMode PhysicsSimulationMode;
        public float GravityMultiplierY;
        public EdTileAnimation Animation;
        public int[] Tags;
        public int LayerId;
        public long DataContainerId;
        public RectF ColliderPadding;
        public TileAnchor Anchor;
        public TileAnchor CellAnchor;

        public override string ToString()
        {
            return Name ?? Ref ?? Id;
        }

        public static EdTileDef Clone(EdTileDef tileDef)
        {
            var clone = new EdTileDef
            {
                Name = tileDef.Name,
                Script = tileDef.Script,
                Animation = tileDef.Animation,
                OverrideSize = tileDef.OverrideSize,
                OverrideWidth = tileDef.OverrideWidth,
                OverrideHeight = tileDef.OverrideHeight,
                OffsetX = tileDef.OffsetX,
                OffsetY = tileDef.OffsetY,
                RowSpan = tileDef.RowSpan,
                ColSpan = tileDef.ColSpan,
                BackgroundColor = tileDef.BackgroundColor,
                TintColor = tileDef.TintColor,
                CollisionMode = tileDef.CollisionMode,
                TileFlags = tileDef.TileFlags,
                PhysicsSimulationMode = tileDef.PhysicsSimulationMode,
                GravityMultiplierY = tileDef.GravityMultiplierY,
                Tags = tileDef.Tags ?? new int[0] { },
                Id = tileDef.Id,
                Ref = tileDef.Ref,
                Handle = tileDef.Handle,
                LayerId = tileDef.LayerId,
                DataContainerId = tileDef.DataContainerId,
                ColliderPadding = tileDef.ColliderPadding,
                Anchor = tileDef.Anchor,
                CellAnchor = tileDef.CellAnchor
            };

            clone.Animation = EdTileAnimation.Clone(tileDef.Animation);
            
            return clone;
        }

        public static void Mimic(EdTileDef me, EdTileDef toMimic)
        {
            me.BackgroundColor = toMimic.BackgroundColor;
            me.TintColor = toMimic.TintColor;
            me.Animation = toMimic.Animation;
            if (toMimic.TileFlags.HasFlag(EdTileFlags.MimicSize))
            {
                me.OverrideSize = toMimic.OverrideSize;
                me.OverrideWidth = toMimic.OverrideWidth;
                me.OverrideHeight = toMimic.OverrideHeight;
                me.OffsetX += toMimic.OffsetX;
                me.OffsetY = toMimic.OffsetY - me.OffsetY;
                me.RowSpan = toMimic.RowSpan;
                me.ColSpan = toMimic.ColSpan;
                me.ColliderPadding = toMimic.ColliderPadding;
                me.Anchor = toMimic.Anchor;
                me.CellAnchor = toMimic.CellAnchor;
            }
        }
    }

    [Flags]
    public enum EdTileFlags
    {
        None = 0,
        Player = 1,
        Type0 = 1 << 2,
        Type1 = 1 << 3,
        Type2 = 1 << 4,
        Type3 = 1 << 5,
        Type4 = 1 << 6,
        Disregard = 1 << 7,
        NamedTileDef = 1 << 8,
        Preserve = 1 << 9,
        GenerateId = 1 << 10,
        CanRotate = 1 << 11,
        PositionLockX = 1 << 12,
        PositionLockY = 1 << 13,
        Simulate = 1 << 14,
        SpriteAutoSizeOff = 1 << 15,
        MimicSize = 1 << 16,
        Save = 1 << 17 | GenerateId,
        MimicCollider = 1 << 18,
        //Reserved8 = 1 << 19,
        //Reserved9 = 1 << 20,
        //Reserved10 = 1 << 21,
        //Reserved11 = 1 << 22,
        //Reserved12 = 1 << 23,
        //Reserved13 = 1 << 24,
        BlockHint = 1 << 25,
        CameraCorner = BlockHint | Type2,
        KiloTexture = 1 << 26,
        OptimizedWall = KiloTexture,
        CloneTileDef = 1 << 27,
        Annotation = 1 << 28,
        CameraTarget = 1 << 29,
    }

    public enum EdPhysicsSimulationMode
    {
        None,
        Static,
        Kinematic,
        Dynamic
    }

    public enum EdTileMode
    {
        Tile,
        Stretch,
        FlipX,
        FlipY,
        FlipXY,
    }

    [Serializable]
    public class SpriteMapItem
    {
        public string Id;
        public Sprite Sprite;
    }

    [Serializable]
    public class AnimationInstance
    {
        public Vector2[] FrameDurations;
        public int CurrentFrame;
        public int VirtualFrameNumber;
        public float FrameTime;
        public float Rate = 1;
        public EdAnimationMode Mode;
        public float LastTime;
        public bool IsPlaying = true;

        public void Rewind()
        {
            VirtualFrameNumber = 0;
            CurrentFrame = 0;
            FrameTime = 0;
        }
    }

    public static class AnimationInstanceExtensions
    {
        public static void Tick(this AnimationInstance instance, float currentTime)
        {
            if (!instance.IsPlaying)
            {
                instance.LastTime = currentTime;
                return;
            }

            if (instance.FrameDurations == null || instance.FrameDurations.Length == 0)
            {
                instance.IsPlaying = false;
                return;
            }

            if (instance.Mode == EdAnimationMode.None)
            {
                instance.IsPlaying = false;
                return;
            }

            var dt = instance.Rate * (currentTime - instance.LastTime);
            instance.LastTime = currentTime;
            instance.FrameTime -= dt;
            if (instance.FrameTime < 0)
                Next(instance);
        }

        static void Next(AnimationInstance instance)
        {
            instance.VirtualFrameNumber++;
            if (instance.Mode == EdAnimationMode.PingPong)
            {
                if (instance.VirtualFrameNumber < instance.FrameDurations!.Length)
                    instance.CurrentFrame = instance.VirtualFrameNumber;
                else if (instance.VirtualFrameNumber < instance.FrameDurations.Length * 2)
                {
                    if (instance.VirtualFrameNumber == instance.FrameDurations.Length)
                        instance.VirtualFrameNumber++;
                    instance.CurrentFrame = (2 * instance.FrameDurations.Length) - 1 - instance.VirtualFrameNumber;
                }
                else
                {
                    instance.VirtualFrameNumber = 1;
                    instance.CurrentFrame = 1;
                }
            }
            else if (instance.Mode == EdAnimationMode.Loop)
            {
                if (instance.VirtualFrameNumber >= instance.FrameDurations!.Length)
                {
                    instance.VirtualFrameNumber = 0;
                    instance.CurrentFrame = 0;
                }
                else instance.CurrentFrame = instance.VirtualFrameNumber;
            }
            else if (instance.Mode == EdAnimationMode.Once)
            {
                if (instance.VirtualFrameNumber >= instance.FrameDurations!.Length)
                {
                    instance.IsPlaying = false;
                    instance.CurrentFrame = instance.FrameDurations.Length - 1;
                }
                else instance.CurrentFrame = instance.VirtualFrameNumber;
            }

            if (instance.FrameDurations == null)
                return;

            if (instance.FrameDurations.Length < instance.CurrentFrame || instance.CurrentFrame < 0)
                instance.CurrentFrame = 0;

            var range = instance.FrameDurations![instance.CurrentFrame];
            var nextFrameTime = UnityEngine.Random.Range(range.x, range.y);
            instance.FrameTime = (float)nextFrameTime;
        }
    }

    [Serializable]
    public struct GameTileData
    {
        public float SpawnX;
        public float SpawnY;
        public float RealX;
        public float RealY;
        public float Width;
        public float Height;
        public EdTileDef TileDef;
        public EdBoard Board;
        public int SortingOrder;
        public float LevelHeight;
        public EdTileCollisionMode Collision;
        public AnimationInstance AnimationInstance;
        public uint Pose;
        public GameTilePayload Payload;
        public int[] Tags;
        public long Id;
        public bool SpriteFlipX;
        public bool SpriteFlipY;
    }

    [Serializable]
    public partial struct GameTilePayload
    {
    }
}

public static class EdExtensions
{
    public static EdTileCollisionMode GetCollisionMode(EdTileDef tileDef, EdBoard board, EdTileCollisionOverride tile = EdTileCollisionOverride.Default)
    {
        var mode = tileDef.CollisionMode;
        if (board.Collision == EdTileCollisionOverride.OptimizedWall) return EdTileCollisionMode.OptimizedWall;
        if (tile == EdTileCollisionOverride.OptimizedWall) return EdTileCollisionMode.OptimizedWall;
        if (board.Collision == EdTileCollisionOverride.Ignore) return EdTileCollisionMode.None;
        if (tile == EdTileCollisionOverride.Ignore) return EdTileCollisionMode.None;
        if (mode == EdTileCollisionMode.OptimizedWall) return EdTileCollisionMode.OptimizedWall;
        if (board.Collision == EdTileCollisionOverride.Wall) return EdTileCollisionMode.Solid;
        if (tile == EdTileCollisionOverride.Wall) return EdTileCollisionMode.Solid;
        return mode;
    }

    public static Color GetColor(this uint val)
    {
        if (val == 0) return Color.white;

        // ARGB
        var a = (val >> 24) & 0xFF;
        var r = (val >> 16) & 0xFF;
        var g = (val >> 8) & 0xFF;
        var b = val & 0xFF;
        return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
    }

    public static Vector2 GetTileSize(this EdTileDef tileDef, float tileWidth, float tileHeight)
    {
        Vector2 size = Vector2.one;
        if (tileDef != null)
        {
            if (tileDef.OverrideSize)
            {
                if (tileDef.OverrideWidth > 0) size.x = tileDef.OverrideWidth;
                if (tileDef.OverrideHeight > 0) size.y = tileDef.OverrideHeight;
            }
            else
            {
                float colSpan = tileDef.ColSpan;
                float rowSpan = tileDef.RowSpan;
                size.x = Mathf.Max(1.0f, tileWidth * colSpan);
                size.y = Mathf.Max(1.0f, tileHeight * rowSpan);
            }
        }
        return size;
    }

    public static Vector2 GetTilePosition(float boardTileWidth, float boardTileHeight, float offsetX, float offsetY, float gapWidth, float gapHeight, int x, int y)
    {
        var position = new Vector2(x * (boardTileWidth + gapWidth), y * (boardTileHeight + gapHeight));
        position.x += offsetX;
        position.y += offsetY;
        return position;
    }

    public static T AddOrGetComponent<T>(this GameObject go) where T : Component
    {
        var component = go.GetComponent<T>();
        if (component == null)
            component = go.AddComponent<T>();
        return component;
    }

    public static EdLevel FlattenLevel(this EdLevel o)
    {
        var level = new EdLevel
        {
            Name = o.Name,
            Width = o.Width,
            Height = o.Height,
            DataContainerId = o.DataContainerId
        };

        var things = new List<EdThing>();
        if (o.Things != null)
            things.AddRange(o.Things);

        if (o.Ids != null)
        {
            var boards = o.Boards?.ToDictionary(b => b.Id, b => b) ?? new Dictionary<string, EdBoard>();
            var quads = o.Quads?.ToDictionary(q => q.Id, q => q) ?? new Dictionary<string, EdQuad>();

            foreach (var id in o.Ids)
            {
                if (boards.TryGetValue(id, out var board))
                    things.Add(board);
                else if (quads.TryGetValue(id, out var quad))
                    things.Add(quad);
            }
        }

        level.Things = things.ToArray();
        return level;
    }

    public static Vector2 GetAnchorFactors(this TileAnchor anchor)
    {
        return anchor switch
        {
            TileAnchor.TopLeft => new Vector2(0, 0),
            TileAnchor.TopCenter => new Vector2(0.5f, 0),
            TileAnchor.TopRight => new Vector2(1, 0),
            TileAnchor.MiddleLeft => new Vector2(0, 0.5f),
            TileAnchor.MiddleCenter => new Vector2(0.5f, 0.5f),
            TileAnchor.MiddleRight => new Vector2(1, 0.5f),
            TileAnchor.BottomLeft => new Vector2(0, 1),
            TileAnchor.BottomCenter => new Vector2(0.5f, 1),
            TileAnchor.BottomRight => new Vector2(1, 1),
            _ => new Vector2(0, 0)
        };
    }
}
