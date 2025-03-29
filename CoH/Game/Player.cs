using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CoH.Game;

public class Player : GameObject
{
    public Vector2 Position = Vector2.Zero;
    public Texture2D PlayerTexture;

    public Player()
    {
        Load();
    }

    public override void Load()
    {
        // TODO: Load PlayerTexture
        base.Load();
    }

    public override void Unload()
    {
        // TODO: Unload PlayerTexture
        base.Unload();
    }

    public override void Frame(float dt)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Right))
            Position.X++;
        if (Raylib.IsKeyPressed(KeyboardKey.Left))
            Position.X--;
        if (Raylib.IsKeyPressed(KeyboardKey.Up))
            Position.Y--;
        if (Raylib.IsKeyPressed(KeyboardKey.Down))
            Position.Y++;
    }

    public override void Render(float dt)
    {
        
    }
}
