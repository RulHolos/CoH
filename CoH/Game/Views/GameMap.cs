using CoH.GameData;
using DotTiled;
using DotTiled.Serialization;
using Raylib_cs;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;

namespace CoH.Game.Views;

public class MapException : Exception
{
    public MapException() { }

    public MapException(string message)
        : base(message) { }

    public MapException(string message, Exception inner)
        : base(message, inner) { }
}

/// <summary>
/// Associated with a Tile, this dictates how the player should behave when colliding with this tile.
/// </summary>
public enum TileType
{
    None = -1,
    Collide,
    Water,
    Waterfall,
    RampLeft,
    RampRight,
    RampDown,
    RampUp,
    Grass,
    BlueGrass,
}

public partial class GameMap : View
{
    private static string PathToMap => Path.Combine(MainWindow.PathToResources, "Maps");

    public int MapId { get; private set; }
    public Map? Map { get; private set; } = null;
    public List<Texture2D> Tilesets { get; private set; } = [];
    public Player Player { get; private set; }
    public Camera2D WorldCamera = new() { Zoom = 1.0f };
    public Camera2D ScreenCamera = new() { Zoom = 1.0f };
    public float ScaleFactor = 4f;
    public RenderTexture2D RenderTarget;
    public Texture2D cloud;

    public Vector4 BackgroundPattern { get; private set; } = Vector4.Zero;

    public GameMap(int mapId, bool fromSave = false)
        : base($"MAP {mapId}")
    {
        MapId = mapId;
        Player = new(this);

        if (fromSave)
            Player.Position = SaveFile.SaveData.PositionOnMap;
    }

    public override void Load()
    {
        Loader mapLoader = Loader.Default();
        string filePath = Path.Combine(PathToMap, $"{MapId}.tmx");
        if (File.Exists(filePath))
            Map = mapLoader.LoadMap(filePath);
        else
            Logger.Error($"MAP DOESN'T EXIST!!!");

        // Map didn't load, abort loading everything.
        // Should the map not loading crash the game or do something else?
        // For now, it throws an exception, because there is no obvious expected behaviour in this stage of development.
        if (Map == null)
            throw new MapException($"Map didn't load");

        Tilesets.AddRange(from Tileset tileset in Map.Tilesets
                          select Raylib.LoadTexture(Path.Combine(PathToMap, tileset.Image.Value.Source)));

        RenderTarget = Raylib.LoadRenderTexture((int)(MainWindow.GameViewport.X / ScaleFactor), (int)(MainWindow.GameViewport.Y / ScaleFactor));

        cloud = Raylib.LoadTexture(Path.Combine(PathToMap, "Back", "cloud.png"));

        Logger.Debug($"Map Loaded successfully");
        Logger.Debug($"Contains {Tilesets.Count} tileset(s)");
        Logger.Debug($"Map size is {Map.Width}x{Map.Height} tiles");

        Player.Load();

        base.Load();

        DialogManager.GetDialog(1);
    }

    public override void Unload()
    {
        foreach (var texture in Tilesets)
            Raylib.UnloadTexture(texture);
        Raylib.UnloadRenderTexture(RenderTarget);

        Raylib.UnloadTexture(cloud);

        Player.Unload();
        
        base.Unload();
    }

    public override void Frame(float deltaTime)
    {
        if (Map == null) return;

        Player.Frame(deltaTime);

#if DEBUG
        float tempFactor = ScaleFactor;

        if (Raylib.IsKeyPressed(KeyboardKey.F3))
            ShowGUI = !ShowGUI;

        if (Raylib.IsKeyPressed(KeyboardKey.F2))
            IgnoreCollisions = !IgnoreCollisions;

        if (Raylib.IsKeyPressed(KeyboardKey.Q))
            tempFactor += 0.1f;
        else if (Raylib.IsKeyPressed(KeyboardKey.E))
            tempFactor -= 0.1f;

        if (tempFactor != ScaleFactor)
        {
            Raylib.UnloadRenderTexture(RenderTarget);
            RenderTarget = Raylib.LoadRenderTexture((int)(MainWindow.GameViewport.X / tempFactor), (int)(MainWindow.GameViewport.Y / tempFactor));
        }  

        ScaleFactor = tempFactor;

        SaveFile.SaveData.CurrentMapId = MapId;

        if (Raylib.IsKeyPressed(KeyboardKey.S))
            SaveFile.Save();
#endif
    }

    /// <summary>
    /// Two camera setups to allow for pixel-perfect rendering.
    /// </summary>
    /// <param name="deltaTime">DeltaTime (in seconds).</param>
    public override void Render(float deltaTime)
    {
        if (Map == null)
            return;

        float virtualRatio = MainWindow.GameViewport.X / MainWindow.GameViewport.X / ScaleFactor;
        float tileSize = Map.TileWidth;
        Vector2 playerTilePos = new(Player.Position.X * tileSize, Player.Position.Y * tileSize);

        // Offsets the camera to the player position (top-left + half-rendertarget offset)
        Vector2 cameraOffset = new(RenderTarget.Texture.Width / 2, RenderTarget.Texture.Height / 2);
        ScreenCamera.Target = playerTilePos - (cameraOffset - new Vector2(tileSize / 2));

        // Transforms the world camera (pixel) to the upscaled screen camera.
        WorldCamera.Target.X = MathF.Round(ScreenCamera.Target.X);
        ScreenCamera.Target.X -= WorldCamera.Target.X;
        ScreenCamera.Target.X *= virtualRatio;

        WorldCamera.Target.Y = MathF.Round(ScreenCamera.Target.Y);
        ScreenCamera.Target.Y -= WorldCamera.Target.Y;
        ScreenCamera.Target.Y *= virtualRatio;

        // Camera and render target things
        Rectangle sourceRec = new(0.0f, 0.0f, RenderTarget.Texture.Width, -(float)RenderTarget.Texture.Height);
        Rectangle destRec = new(-virtualRatio, -virtualRatio, MainWindow.GameViewport.X + virtualRatio * 2, MainWindow.GameViewport.Y + virtualRatio * 2);

        Raylib.BeginTextureMode(RenderTarget);

        Raylib.ClearBackground(new(0x57, 0x9E, 0xBB));

        Raylib.BeginMode2D(WorldCamera);

        // ### BACKGROUND TEXTURE ### //
        float cloudSize = cloud.Width;
        float cloudOffsetX = this.Timer / 64;
        int tilesX = (int)(MainWindow.GameViewport.X / cloudSize) + 3;
        int tilesY = (int)(MainWindow.GameViewport.Y / cloudSize) + 3;

        for (int y = -1; y < tilesY; y++)
        {
            for (int x = -1; x < tilesX; x++)
            {
                float drawX = x * cloudSize - (cloudOffsetX % cloudSize);
                float drawY = y * cloudSize - (0 % cloudSize);
                Raylib.DrawTexture(cloud, (int)drawX, (int)drawY, Raylib_cs.Color.White);
            }
        }

        Player.RenderUpsidedown(deltaTime);

        // ### ACTUAL RENDERING ### //
        foreach (BaseLayer layer in Map.Layers)
        {
            if (!layer.Visible)
                continue;
            if (layer is ObjectLayer objectLayer)
            {
                if (objectLayer.Name.Equals("player", StringComparison.CurrentCultureIgnoreCase))
                {
                    Player.Render(deltaTime);
                }
            }
            if (layer is TileLayer tileLayer)
            {
                uint width = tileLayer.Width;
                uint height = tileLayer.Height;
                uint[] tileData = tileLayer.Data.Value.GlobalTileIDs.Value;

                // ### AVOID RENDERING OF OFFSCREEN TILES ### //
                int occlusionRadius = 8;
                int playerTileX = (int)Player.Position.X;
                int playerTileY = (int)Player.Position.Y;

                //Vector2 occlusionX = new(playerTileX - occlusionRadius)
                int startX = playerTileX - occlusionRadius;
                int endX = playerTileX + occlusionRadius;
                int startY = playerTileY - occlusionRadius;
                int endY = playerTileY + occlusionRadius;

                bool bgPatternExists = tileLayer.Name.Contains("Background", StringComparison.CurrentCultureIgnoreCase);

                for (int y = startY; y <= endY; y++)
                {
                    for (int x = startX; x <= endX; x++)
                    {
                        if(x >= 0 && y >= 0 && x < width && y < height)
                        {
                            uint index = (uint)(y * tileLayer.Width + x);
                            uint tileId = tileData[index];

                            if (tileId == 0)
                                continue;

                            RenderTile(tileLayer, x, y, tileId, deltaTime);
                        }
                        // Infinite pattern (2x2 pattern on the top-right on the "Background" tile layer.
                        else if (bgPatternExists)
                        {
                            int patternX = Math.Abs(x % 2);
                            int patternY = Math.Abs(y % 2);

                            uint patternTileId = 0;
                            if (patternY < 2 && patternX < 2)
                            {
                                int patternTileX = patternX;
                                int patternTileY = patternY;
                                uint patternIndex = (uint)(patternTileY * width + patternTileX);

                                if (patternIndex < tileData.Length)
                                    patternTileId = tileData[patternIndex];
                            }

                            if (patternTileId != 0)
                                RenderTile(tileLayer, x, y, patternTileId, deltaTime);
                        }
                    }
                }
            }
        }

        Raylib.EndMode2D();

        Raylib.EndTextureMode();

        Raylib.BeginMode2D(ScreenCamera);
        Raylib.DrawTexturePro(RenderTarget.Texture, sourceRec, destRec, Vector2.Zero, 0.0f, Raylib_cs.Color.White);
        Raylib.EndMode2D();
    }

    private void RenderTile(TileLayer layer, int x, int y, uint tileId, float deltaTime)
    {
        Tileset? tileset = GetTilesetForTile(tileId, out uint trueTileId, out int tilesetIndex);
        if (tileset == null)
            return;

        Tile? tile = tileset.Tiles.FirstOrDefault(t => t.ID == (int)trueTileId);

        // ### FRAME ANIMATION ### //
        if (tile != null && tile.Animation.Count > 0)
        {
            int animationTime = 0;

            foreach (var frame in tile.Animation)
                animationTime += (int)(frame.Duration * 60 / 1000);

            int currentTime = Timer % animationTime;

            int elapsedTime = 0;
            foreach (var frame in tile.Animation)
            {
                elapsedTime += (int)(frame.Duration * 60 / 1000);
                if (currentTime < elapsedTime)
                {
                    trueTileId = frame.TileID;
                    break;
                }
            }
        }

        var tint = Raylib_cs.Color.White;
        if (layer.TintColor.HasValue)
        {
            var tintColor = layer.TintColor.Value;
            tint = new(tintColor.R, tintColor.G, tintColor.B, tintColor.A);
        }

        uint tilesPerRow = tileset.Image.Value.Width / tileset.TileWidth;
        uint srcX = trueTileId % tilesPerRow * tileset.TileWidth;
        uint srcY = trueTileId / tilesPerRow * tileset.TileHeight;
        Rectangle sourceRect = new(srcX, srcY, tileset.TileWidth, tileset.TileHeight);

        float posX = x * tileset.TileWidth;
        float posY = y * tileset.TileHeight;
        Rectangle drawPosition = new(posX, posY, tileset.TileWidth, tileset.TileHeight);

        Raylib.DrawTexturePro(Tilesets[tilesetIndex], sourceRect, drawPosition, Vector2.Zero, 0, tint);
    }

    public Tileset? GetTilesetForTile(uint tileGid, out uint localTileId, out int tilesetIndex)
    {
        localTileId = 0;
        tilesetIndex = 0;

        Tileset? foundTileset = null;
        int i = 0;
        foreach (var tileset in Map!.Tilesets)
        {
            if (tileGid >= tileset.FirstGID)
            {
                foundTileset = tileset;
                tilesetIndex = i;
            }
            else
                break;
            i++;
        }

        if (foundTileset != null)
            localTileId = tileGid - foundTileset.FirstGID;

        return foundTileset;
    }

    public (Tile?, TileType) GetTileAtPosition(Vector2 tilePos, int layerIndex, uint tileId)
    {
        Tileset? tileset = GetTilesetForTile(tileId, out uint trueTileId, out _);
        if (tileset != null)
        {
            Tile? tile = tileset.Tiles.FirstOrDefault(t => t.ID == trueTileId);
            if (tile != null && tile.Properties != null)
            {
                Logger.Debug($"Checking tile {tile.ID}.");
                foreach (var p in tile.Properties)
                    Logger.Debug(p.Name);

                // TODO: Check for other tile properties.
                if (tile.Properties.Any(p => p.Name == "Collision"))
                {
                    return (tile, TileType.Collide); // Tile has "Collision" property, can't walk on it
                }
            }
        }
        return (null, TileType.None);
    }
}
