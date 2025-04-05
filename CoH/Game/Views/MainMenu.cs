using DotTiled.Serialization;
using DotTiled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raylib_cs;
using Serilog;
using CoH.GameData;

namespace CoH.Game.Views;

public partial class MainMenu : View
{
    public MainMenu()
        : base()
    {
    }

    public override void Load()
    {
        // New Game
        widgets.Add(new(0, true,
            () => {
                GoToNextView(new GameMap(0));
                Log.Debug("New Game");
            }
        ));
        // Continue
        widgets.Add(new(1, SaveFile.SaveExists,
            () =>
            {
                GoToNextView(new GameMap(SaveFile.SaveData.CurrentMapId, true));
                Log.Debug("Continue");
            }
        ));
        // Options
        widgets.Add(new(2, true,
            () =>
            {

            }
        ));

        base.Load();
    }

    public override void Unload()
    {
        widgets.Clear();

        base.Unload();
    }

    public override void Frame(float deltaTime)
    {
        foreach (Widget widget in widgets)
            widget.ShakeTimer = Math.Max(0, widget.ShakeTimer - 0.1f);

        if (Lock) return;

        if (Raylib.IsKeyPressed(KeyboardKey.Up))
        {
            widgetIndex = widgets.Previous().Index;
            CurrentWidget.ShakeTimer = 1;
            Log.Debug($"New Widget Selected: {widgetIndex}");
            // Play Select Sound;
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.Down))
        {
            widgetIndex = widgets.Next().Index;
            CurrentWidget.ShakeTimer = 1;
            Log.Debug($"New Widget Selected: {widgetIndex}");
            // Play Select Sound;
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.W))
        {
            CurrentWidget.Callback?.Invoke();
            // Play OK Sound;
        }

        foreach (Widget widget in widgets)
            widget.SelectTimer = Balance(widget.SelectTimer, 0, 1, 1 / 4, CurrentWidget == widget);
    }

    private static float Balance(float v, float a, float b, float speed, bool state)
    {
        if (state)
            v = Math.Min(v + speed, b);
        else
            v = Math.Max(a, v - speed);
        return v;
    }
}
