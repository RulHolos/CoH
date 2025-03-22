using CoH.GameData;
using DotTiled;
using DotTiled.Serialization;
using Raylib_cs;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoH.Game;

public class GameMap : View
{
    public int MapId { get; private set; }
    public Map? Map { get; private set; } = null;

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
        Raylib.ClearBackground(new Raylib_cs.Color(Map.BackgroundColor.R, Map.BackgroundColor.G, Map.BackgroundColor.B, Map.BackgroundColor.A));
    }

    public override void RenderGUI(float deltaTime)
    {
        
    }
}
