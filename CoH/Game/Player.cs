using CoH.Game.Views;
using CoH.GameData;
using DotTiled;
using Raylib_cs;
using Serilog;
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
    public sbyte FacingDirection = 0; // 0 = Down, 1 = Up, 2 = Right, 3 = Left

    private bool isMoving = false;
    private bool isVirtualMoving = false;
    private bool isSwimming = false;
    private Vector2 movementDirection = Vector2.Zero;

    private int timer = 0;

    private readonly GameMap Mappe;

    public Player(GameMap map)
    {
        Mappe = map;
    }

    public override void Load()
    {
        PlayerTexture = Raylib.LoadTexture(Path.Combine(MainWindow.PathToResources, "Player", "GirlChip.png"));

        base.Load();
    }

    public override void Unload()
    {   
        Raylib.UnloadTexture(PlayerTexture);

        base.Unload();
    }

    public override void Frame(float dt)
    {
        if (!isMoving)
        {
            Vector2 newMovementDirection = Vector2.Zero;
            sbyte newFacingDirection = FacingDirection;

            if (Raylib.IsKeyDown(KeyboardKey.Down))
            {
                newMovementDirection = new Vector2(0, 1);
                newFacingDirection = 0;
                isVirtualMoving = true;
            }
            else if (Raylib.IsKeyDown(KeyboardKey.Up))
            {
                newMovementDirection = new Vector2(0, -1);
                newFacingDirection = 3;
                isVirtualMoving = true;
            }
            else if (Raylib.IsKeyDown(KeyboardKey.Right))
            {
                newMovementDirection = new Vector2(1, 0);
                newFacingDirection = 2;
                isVirtualMoving = true;
            }
            else if (Raylib.IsKeyDown(KeyboardKey.Left))
            {
                newMovementDirection = new Vector2(-1, 0);
                newFacingDirection = 1;
                isVirtualMoving = true;
            }
            else
            {
                isVirtualMoving = false;
            }

            Vector2 newTargetPosition = Position + newMovementDirection;
            if (newMovementDirection != Vector2.Zero && CanMoveToTile(newTargetPosition))
            {
                movementDirection = newMovementDirection;
                isMoving = true;
                TargetPosition = newTargetPosition;
            }
            FacingDirection = newFacingDirection;
        }
        else
        {
            float currentSpeed = Raylib.IsKeyDown(KeyboardKey.X) ? RunSpeed : WalkSpeed;

            Vector2 moveStep = movementDirection * currentSpeed * dt;
            Position += moveStep;

            if (Vector2.Distance(Position, TargetPosition) <= currentSpeed * dt)
            {
                Position = TargetPosition;
                SaveFile.SaveData.PositionOnMap = Position;
                SaveFile.SaveData.FacingDir = FacingDirection;
                isMoving = false;
            }
        }

        if (isVirtualMoving)
            timer++;
        else
            timer = 0;
    }

    /// <summary>
    /// Allows movement by default unless a collision or other cases are found.
    /// </summary>
    /// <param name="targetPos">The tile to check.</param>
    /// <returns>True if the player can move to this tile. Otherwise; false.</returns>
    private bool CanMoveToTile(Vector2 targetPos)
    {
        /*
         * Idea: Go with a tileset with numbers for the tiles collision properties (can have multiple collision layers for complex maps).
         * Like TPDP does it basically. That way I don't have to check each and every single tile layer for every tiles, only the collision layers.
         */

        foreach (BaseLayer layer in Mappe.Map!.Layers)
        {
            if (layer is TileLayer tileLayer)
            {
                uint tileIndex = (uint)(targetPos.Y * tileLayer.Width + targetPos.X);
                uint tileId = tileLayer.Data.Value.GlobalTileIDs.Value[tileIndex];

                if (tileId == 0) return true; // No tile, allow movement

                Tileset? tileset = Mappe.GetTilesetForTile(tileId, out uint trueTileId, out _);
                if (tileset != null)
                {
                    Tile? tile = tileset.Tiles.FirstOrDefault(t => t.ID == trueTileId);
                    if (tile != null && tile.Properties != null)
                    {
                        Log.Debug($"Checking tile {tile.ID}.");
                        foreach (var p in tile.Properties)
                            Log.Debug(p.Name);

                        // TODO: Check for other tile properties.
                        if (tile.Properties.Any(p => p.Name == "Collision"))
                        {
                            return false; // Tile has "Collide" property, can't walk on it
                        }
                        if (tile.Properties.Any(p => p.Name == "Water"))
                        {
                            return isSwimming;
                        }
                    }
                }
            }
        }
        return true; // Allow movement by default is all other checks failed or were ignored.
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
        int animationSpeed = 16;
        int[] animationFrames = [0, 1, 0, 2]; // Return to first animation between each frame. (It's weird if it doesn't)

        int frameIndex = (timer / animationSpeed) % animationFrames.Length;
        int frame = animationFrames[frameIndex];

        int multiplier = FacingDirection;
        bool isRunning = Raylib.IsKeyDown(KeyboardKey.X);

        if (isRunning && isVirtualMoving)
            multiplier += 4;

        Raylib.DrawTextureRec(PlayerTexture,
            new Rectangle(frame * 32, multiplier * 32, 32, 32),
            new Vector2((Position.X * 16) - 8, (Position.Y * 16) - 16),
            Raylib_cs.Color.White);
    }
}
