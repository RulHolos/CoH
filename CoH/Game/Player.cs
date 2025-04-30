using CoH.Game.Views;
using CoH.GameData;
using DotTiled;
using ImGuiNET;
using Raylib_cs;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CoH.Game;

public enum FacingDirection
{
    Down,
    Left,
    Right,
    Up,
}

public class Player : GameObject
{
    public Vector2 Position = Vector2.Zero; // Coords on the map.
    public Vector2 TargetPosition = Vector2.Zero; // Tile moving to.
    public float WalkSpeed = 4.0f; // Tiles per second
    public float RunSpeed = 8.0f; // Tiles per second
    public Texture2D PlayerTexture;
    public FacingDirection FacingDirection = FacingDirection.Down;

    private bool isMoving = false;
    private bool isVirtualMoving = false;
    private bool isSwimming = false;
    private Vector2 movementDirection = Vector2.Zero;

    public bool CanMove = true;

    private int moveTimer = 0;
    private int Timer = 0;

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

    private float directionPressTimer = 0f;
    private const float MaxDirectionOnlyPressTime = 2f;

    public override void Frame(float dt)
    {
        if (!isMoving && CanMove)
        {
            Vector2 newMovementDirection = Vector2.Zero;
            FacingDirection newFacingDirection = FacingDirection;
            bool directionPressed = false;

            if (Raylib.IsKeyDown(KeyboardKey.Down))
            {
                newMovementDirection = new Vector2(0, 1);
                newFacingDirection = FacingDirection.Down;
                directionPressed = true;
            }
            else if (Raylib.IsKeyDown(KeyboardKey.Up))
            {
                newMovementDirection = new Vector2(0, -1);
                newFacingDirection = FacingDirection.Up;
                directionPressed = true;
            }
            else if (Raylib.IsKeyDown(KeyboardKey.Right))
            {
                newMovementDirection = new Vector2(1, 0);
                newFacingDirection = FacingDirection.Right;
                directionPressed = true;
            }
            else if (Raylib.IsKeyDown(KeyboardKey.Left))
            {
                newMovementDirection = new Vector2(-1, 0);
                newFacingDirection = FacingDirection.Left;
                directionPressed = true;
            }

            if (directionPressed)
            {
                directionPressTimer += dt * 60f; // convert dt to approximate frame count
                FacingDirection = newFacingDirection;

                Vector2 newTargetPosition = Position + newMovementDirection;
                TargetPosition = newTargetPosition;

                if (directionPressTimer >= MaxDirectionOnlyPressTime && CanMoveToTile(newTargetPosition))
                {
                    movementDirection = newMovementDirection;
                    isMoving = true;
                }

                isVirtualMoving = true;
            }
            else
            {
                directionPressTimer = 0f;
                isVirtualMoving = false;
            }
        }
        else
        {
            float currentSpeed = IsRunning() ? RunSpeed : WalkSpeed;

            Vector2 moveStep = movementDirection * currentSpeed * dt;
            Position += moveStep;

            if (Vector2.Distance(Position, TargetPosition) <= currentSpeed * dt)
            {
                Position = TargetPosition;
                TargetPosition = Position + movementDirection;
                SaveFile.SaveData.PositionOnMap = Position;
                SaveFile.SaveData.FacingDir = FacingDirection;
                isMoving = false;
            }
        }

        if (Raylib.IsKeyPressed(KeyboardKey.W))
            Interact(TargetPosition);

        if (isVirtualMoving)
            moveTimer++;
        else
            moveTimer = 0;

        Timer++;
    }

    /// <summary>
    /// Allows movement by default unless a collision or other cases are found.
    /// </summary>
    /// <param name="targetPos">The tile to check.</param>
    /// <returns>True if the player can move to this tile. Otherwise; false.</returns>
    private bool CanMoveToTile(Vector2 targetPos)
    {
        if (Mappe.IgnoreCollisions) // Only in debug mode. Disables collision with tiles.
            return true;

        /*
         * Idea: Go with a tileset with numbers for the tiles collision properties (can have multiple collision layers for complex maps).
         * Like TPDP does it basically. That way I don't have to check each and every single tile layer for every tiles, only the collision layers.
         */

        foreach (BaseLayer layer in Mappe.Map!.Layers)
        {
            if (layer is TileLayer tileLayer)
            {
                if (!tileLayer.Name.Contains("collision", StringComparison.CurrentCultureIgnoreCase))
                    continue;
                uint tileIndex = (uint)(targetPos.Y * tileLayer.Width + targetPos.X);
                uint tileId;
                try { tileId = tileLayer.Data.Value.GlobalTileIDs.Value[tileIndex]; }
                catch { tileId = 0; }

                if (tileId == 0) { isSwimming = false; return true; } // No tile, allow movement and reset swimming state.

                Tileset? tileset = Mappe.GetTilesetForTile(tileId, out uint trueTileId, out _);
                if (tileset != null)
                {
                    if (trueTileId == (int)TileType.Collide) return false; // Collision
                    if (trueTileId == (int)TileType.Water) return isSwimming; // Water collider
                }
            }
        }
        return true; // Allow movement by default is all other checks failed or were ignored.
    }

    private void Interact(Vector2 targetPos)
    {
        foreach (BaseLayer layer in Mappe.Map!.Layers)
        {
            if (layer is TileLayer tileLayer)
            {
                if (!tileLayer.Name.Contains("collision", StringComparison.CurrentCultureIgnoreCase))
                    continue;
                uint tileIndex = (uint)(targetPos.Y * tileLayer.Width + targetPos.X);
                uint tileId;
                try { tileId = tileLayer.Data.Value.GlobalTileIDs.Value[tileIndex]; }
                catch { tileId = 0; }

                if (tileId == 0) return;

                Tileset? tileset = Mappe.GetTilesetForTile(tileId, out uint trueTileId, out _);
                if (tileset != null)
                {
                    if (trueTileId == (int)TileType.Water && !isSwimming)
                    {
                        isSwimming = true;
                        Position = TargetPosition;
                        Mappe.Logger.Information("Pressed Water");
                    }
                }
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

    // TODO: There's some stuttering when moving sometimes. Maybe relating to the dt or the timer reseting too late or too early?
    public void DoRender(float dt, bool upsideDown = false)
    {
        bool isRunning = IsRunning();

        int animationSpeed = isRunning ? 10 : 16;
        int[] animationFrames = [0, 1, 0, 2]; // Return to first animation between each frame. (It's weird if it doesn't)

        int frameIndex = (moveTimer / animationSpeed) % animationFrames.Length;
        int frame = animationFrames[frameIndex];

        FacingDirection facingDir = FacingDirection;
        if (upsideDown && facingDir == FacingDirection.Right)
            facingDir = FacingDirection.Left;
        else if (upsideDown && facingDir == FacingDirection.Left)
            facingDir = FacingDirection.Right;

        sbyte multiplier = (sbyte)facingDir;

        if (isRunning && isVirtualMoving)
            multiplier += 4;

        if (!upsideDown)
        {
            Raylib.DrawTextureRec(PlayerTexture,
                new Rectangle(frame * 32, multiplier * 32, 32, 32),
                new Vector2((Position.X * 16) - 8, (Position.Y * 16) - 16),
                Raylib_cs.Color.White);
        }
        else
        {
            Raylib.DrawTexturePro(PlayerTexture,
                new(frame * 32, multiplier * 32, 32, 32),
                new((Position.X * 16) - 8, (Position.Y * 16) - 16, 32, -32),
                new(32, 64-8),
                180f,
                Raylib_cs.Color.White);
        }   
    }

    public override void Render(float dt)
    {
        DoRender(dt);
    }

    public void RenderUpsidedown(float dt)
    {
        DoRender(dt, true);
    }

    public override void RenderGUI(float dt)
    {
        ImGui.SeparatorText("Player speed");

        ImGui.SliderFloat("Player walking speed", ref WalkSpeed, 0f, 100f);
        ImGui.SliderFloat("Player running speed", ref RunSpeed, 0f, 100f);

        if (ImGui.Button("Reset player speed"))
        {
            WalkSpeed = 4f;
            RunSpeed = 8f;
        }

        ImGui.Checkbox("Can Move", ref CanMove);
    }
}
