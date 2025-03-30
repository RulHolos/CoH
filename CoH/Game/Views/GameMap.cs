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

public partial class GameMap : View
{
    public int MapId { get; private set; }
    public Map? Map { get; private set; } = null;
    public List<Texture2D> Tilesets { get; private set; } = [];
    public Player Player = new();
    public Camera2D WorldCamera = new() { Zoom = 1.0f };
    public Camera2D ScreenCamera = new() { Zoom = 1.0f };
    public float ScaleFactor = 4f;
    public RenderTexture2D RenderTarget;

#if DEBUG
    private bool ShowGUI = true;
#else
    private bool ShowGUI = false;
#endif

    public GameMap(int mapId)
        : base()
    {
        MapId = mapId;
    }

    public override void Load()
    {
        Loader mapLoader = Loader.Default();
        string filePath = Path.Combine(MainWindow.PathToResources, $"{MapId}.tmx");
        if (File.Exists(filePath))
            Map = mapLoader.LoadMap(filePath);
        else
            Log.Error($"MAP: [ID {MapId}] DIDN'T LOAD!!!");

        if (Map == null) return;

        Tilesets.AddRange(from Tileset tileset in Map.Tilesets
                          select Raylib.LoadTexture(Path.Combine(MainWindow.PathToResources, tileset.Image.Value.Source)));

        RenderTarget = Raylib.LoadRenderTexture((int)(MainWindow.GameViewport.X / ScaleFactor), (int)(MainWindow.GameViewport.Y / ScaleFactor));
        
        base.Load();
    }

    public override void Unload()
    {
        foreach (var texture in Tilesets)
            Raylib.UnloadTexture(texture);
        Raylib.UnloadRenderTexture(RenderTarget);
        
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
#endif
    }

    /// <summary>
    /// Two camera setups to allow for pixel-perfect rendering.
    /// </summary>
    /// <param name="deltaTime">DeltaTime (in seconds).</param>
    public override void Render(float deltaTime)
    {
        if (Map == null) return; // If no map is loaded, then this doesn't do anything, since it...Doesn't have any data.
        // If there is no data, should the game go back to the previous valid view or just doesn't do anything?
        // That shouldn't happen in a normal game, but still, it's something to think about.

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

        Raylib.ClearBackground(new(Map.BackgroundColor.R, Map.BackgroundColor.G, Map.BackgroundColor.B, (byte)255));

        Raylib.BeginMode2D(WorldCamera);

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
                uint[] tileData = tileLayer.Data.Value.GlobalTileIDs.Value;

                for (uint index = 0; index < tileData.Length; index++)
                {
                    uint tileId = tileData[index];
                    if (tileId == 0)
                        continue; // No tile

                    uint x = index % width;
                    uint y = index / width;

                    RenderTile(tileLayer, x, y, tileId, deltaTime);
                }
            }
        }

        Raylib.EndMode2D();

        Raylib.EndTextureMode();

        Raylib.BeginMode2D(ScreenCamera);
        Raylib.DrawTexturePro(RenderTarget.Texture, sourceRec, destRec, Vector2.Zero, 0.0f, Raylib_cs.Color.White);
        Raylib.EndMode2D();
    }

    private void RenderTile(TileLayer layer, uint x, uint y, uint tileId, float deltaTime)
    {
        Tileset? tileset = GetTilesetForTile(tileId, out uint trueTileId, out int tilesetIndex);
        if (tileset == null) return;

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

    private Tileset? GetTilesetForTile(uint tileGid, out uint localTileId, out int tilesetIndex)
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
}
