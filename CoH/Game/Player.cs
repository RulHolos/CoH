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
    public Vector2 Position = Vector2.Zero; // Coords on the map.
    public Vector2 TargetPosition = Vector2.Zero; // Tile moving to.
    public float WalkSpeed = 4.0f; // Tiles per second
    public float RunSpeed = 8.0f; // Tiles per second
    public Texture2D PlayerTexture;
    public sbyte FacingDirection = 3; // 0 = Left, 1 = Right, 2 = Up, 3 = Down

    private bool isMoving = false;
    private Vector2 movementDirection = Vector2.Zero;

    public Player()
    {
        Load();
    }

    public override void Load()
    {
        base.Load();
    }

    public override void Unload()
    {   
        base.Unload();
    }

    public override void Frame(float dt)
    {
        if (!isMoving)
        {
            if (Raylib.IsKeyDown(KeyboardKey.Right))
            {
                movementDirection = new Vector2(1, 0);
                FacingDirection = 1;
            }
            else if (Raylib.IsKeyDown(KeyboardKey.Left))
            {
                movementDirection = new Vector2(-1, 0);
                FacingDirection = 0;
            }
            else if (Raylib.IsKeyDown(KeyboardKey.Up))
            {
                movementDirection = new Vector2(0, -1);
                FacingDirection = 2;
            }
            else if (Raylib.IsKeyDown(KeyboardKey.Down))
            {
                movementDirection = new Vector2(0, 1);
                FacingDirection = 3;
            }
            else
            {
                movementDirection = Vector2.Zero;
            }

            if (movementDirection != Vector2.Zero)
            {
                isMoving = true;
                TargetPosition = Position + movementDirection;
            }
        }
        else
        {
            // Getting the actual run/walk speed.
            float currentSpeed = IsRunning() ? RunSpeed : WalkSpeed;

            // Moving the player
            Vector2 moveStep = movementDirection * currentSpeed * dt;
            Position += moveStep;

            // Check for reach (or pass) the tile position
            if (Vector2.Distance(Position, TargetPosition) <= currentSpeed * dt)
            {
                Position = TargetPosition; // Snap to tile
                isMoving = false;
            }
        }
    }

    /// <summary>
    /// Checks if the player is currently running (X and both shift keys).<br/>
    /// Doesn't check for gamepad yet: TODO
    /// </summary>
    /// <returns>True if the player is running. Otherwise; false.</returns>
    public static bool IsRunning()
    {
        return
            Raylib.IsKeyDown(KeyboardKey.X)
            || Raylib.IsKeyDown(KeyboardKey.LeftShift)
            || Raylib.IsKeyDown(KeyboardKey.RightShift);
    }

    public override void Render(float dt)
    {
        // Mockup for player: 2 tiles height.
        Raylib.DrawRectangle((int)(Position.X * 16), (int)(Position.Y * 16) - 16, 16, 16, Raylib_cs.Color.White);
        Raylib.DrawRectangle((int)(Position.X * 16), (int)(Position.Y * 16), 16, 16, Raylib_cs.Color.Gray);

        // TODO: Actual player image.
        //Raylib.DrawTexture(PlayerTexture, (int)(Position.X * 32), (int)(Position.Y * 32), Raylib_cs.Color.White);
    }
}
