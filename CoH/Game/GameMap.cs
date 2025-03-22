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

namespace CoH.Game;

public class GameMap : View
{
    public int MapId { get; private set; }
    public Map? Map { get; private set; } = null;
    public List<Texture2D> Tilesets { get; private set; } = []; // (Texture2D, FirstGID)
    private Vector2 RenderCursor = Vector2.Zero;

    public GameMap(int mapId)
        : base()
    {
        MapId = mapId;
        Loader mapLoader = Loader.Default();
        string filePath = Path.Combine(MainWindow.PathToResources, $"{mapId}.tmx");
        if (File.Exists(filePath))
            Map = mapLoader.LoadMap(filePath);
        else
            Log.Error($"MAP: [ID {MapId}] DIDN'T LOAD!!!");
    }

    public override void Load()
    {
        foreach (Tileset tileset in Map?.Tilesets)
        {
            Tilesets.Add(Raylib.LoadTexture(Path.Combine(MainWindow.PathToResources, tileset.Image.Value.Source)));
        }
        base.Load();
    }

    public override void Unload()
    {
        foreach (var texture in Tilesets)
            Raylib.UnloadTexture(texture);
        base.Unload();
    }

    public override void Frame(float deltaTime)
    {
        
    }

    public override void Render(float deltaTime)
    {
        if (Map == null)
            return; // If no map is loaded, then this doesn't do anything, since it...Doesn't have any data.
        // If there is no data, should the game go back to the previous valid view or just doesn't do anything?
        // That shouldn't happen in a normal game, but still, it's something to think about.

        // Renders the background color.
        Raylib.ClearBackground(new Raylib_cs.Color(Map.BackgroundColor.R, Map.BackgroundColor.G, Map.BackgroundColor.B, (byte)255));

        foreach (BaseLayer layer in Map.Layers)
        {
            if (!layer.Visible)
                continue;
            if (layer is TileLayer tileLayer)
            {
                uint width = tileLayer.Width;
                uint height = tileLayer.Height;
                uint[] tileData = tileLayer.Data.Value.GlobalTileIDs.Value;

                for (uint index = 0; index < tileData.Length; index++)
                {
                    uint tileId = tileData[index];
                    if (tileId == 0)
                        continue; // No tile

                    uint x = index % width;
                    uint y = index / width;

                    RenderTile(tileLayer, x, y, tileId);
                }
            }
        }
    }

    private void RenderTile(TileLayer layer, uint x, uint y, uint tileId)
    {
        Tileset tileset = GetTilesetForTile(tileId, out uint trueTileId, out int tilesetIndex);
        Raylib_cs.Color tint = new Raylib_cs.Color(255, 255, 255, 255);
        if (layer.TintColor.HasValue)
        {
            DotTiled.Color tintColor = layer.TintColor.Value;
            tint = new(tintColor.R, tintColor.G, tintColor.B, tintColor.A);
        }

        uint tilesPerRow = tileset.Image.Value.Width / tileset.TileWidth;
        uint srcX = (trueTileId % tilesPerRow) * tileset.TileWidth;
        uint srcY = (trueTileId / tilesPerRow) * tileset.TileHeight;
        Rectangle sourceRect = new(srcX, srcY, tileset.TileWidth, tileset.TileHeight);

        float posX = x * tileset.TileWidth;
        float posY = y * tileset.TileHeight;
        Vector2 drawPosition = new(posX, posY);

        Raylib.DrawTextureRec(Tilesets[tilesetIndex], sourceRect, drawPosition, tint);
    }

    private Tileset GetTilesetForTile(uint tileGid, out uint localTileId, out int tilesetIndex)
    {
        localTileId = 0;
        tilesetIndex = 0;

        Tileset foundTileset = null;
        int i = 0;
        foreach (var tileset in Map.Tilesets)
        {
            if (tileGid >= tileset.FirstGID)
            {
                foundTileset = tileset;
                tilesetIndex = i;
            }
            else
            {
                break;
            }
            i++;
        }

        if (foundTileset != null)
        {
            localTileId = tileGid - foundTileset.FirstGID;
            return foundTileset;
        }
        
        return null; // Should not happen unless there's an error in the map
    }

    public override void RenderGUI(float deltaTime)
    {
        
    }
}
