using DotTiled.Serialization;
using DotTiled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raylib_cs;
using Serilog;

namespace CoH.Game.Views;

public class MainMenu : View
{
    public MainMenu()
        : base()
    {

    }

    public override void Load()
    {
        base.Load();
    }

    public override void Unload()
    {
        base.Unload();
    }

    public override void Frame(float deltaTime)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.W))
        {
            Log.Information($"Pressed New Game at Timer = {Timer}");
            GoToNextView(new GameMap(0));
        }
    }

    public override void Render(float deltaTime)
    {

    }

    public override void RenderGUI(float deltaTime)
    {

    }
}
