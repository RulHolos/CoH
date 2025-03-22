﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Numerics;

using Raylib_cs;
using rlImGui_cs;
using ImGuiNET;
using CoH.Game;
using Serilog;

namespace CoH;

/*
 * For maps: Use Tiled and DotTiled(for parsing)
 * Put the player on the "Player" layer. It MUST be named "Player", not "player" or anything other.
 * Named layers:
 * Player (Object Layer) : Where to render the player in the tile order.
 * Events (Tile Layer) : 
 */

internal static class MainWindow
{
    public static Version? VersionNumber = Assembly.GetEntryAssembly()?.GetName().Version;
    public static string PathToResources = Path.Combine(Directory.GetCurrentDirectory(), "Assets");

    private static RenderTexture2D _renderTarget;
    private static Texture2D borderTexture;

    public static View FirstView { get; private set; } = new MainMenu();

    public static View? CurrentView { get; set; }

    /// <summary>
    /// Initializes the game window and starts the game loop.<br/>
    /// This is a blocking method.
    /// </summary>
    public static void Initialize()
    {
        Raylib.SetConfigFlags(ConfigFlags.Msaa4xHint | ConfigFlags.HighDpiWindow | ConfigFlags.VSyncHint | ConfigFlags.ResizableWindow);
        Raylib.InitWindow(1920/2, 1080/2, $"Myara 2 ~ Cycle of Hakurama | v{VersionNumber}");
        Raylib.SetExitKey(KeyboardKey.Null);
        Raylib.SetTargetFPS(60);

        Raylib.InitAudioDevice();

        Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .WriteTo.File("Engine.log",
            rollingInterval: RollingInterval.Infinite,
            rollOnFileSizeLimit: true)
        .CreateLogger();

        rlImGui.Setup(true, true);
        ImGui.GetIO().ConfigWindowsMoveFromTitleBarOnly = true;

        borderTexture = Raylib.LoadTexture(Path.Combine(Directory.GetCurrentDirectory(), "border.jpg"));

        _renderTarget = Raylib.LoadRenderTexture(960, 720);

        while (!Raylib.IsWindowReady())
            continue;
        
        Startup();

        while (!Raylib.WindowShouldClose())
        {
            float dt = Raylib.GetFrameTime();

            BeforeFrame(dt);
            Frame(dt);
            AfterFrame(dt);

            BeforeRender(out ViewportInfo vinf, dt);
            Render(dt);
            AfterRender(vinf, dt);

            RenderGUI(dt);
        }

        Dispose();

        rlImGui.Shutdown();
        Raylib.CloseAudioDevice();
        Raylib.CloseWindow();
    }

    public static void Dispose()
    {

    }

    #region GameLoop

    /// <summary>
    /// Initializes all the main things that needs to be initialized at startup.<br/>
    /// Only runs once BEFORE any frame or render are called.<br/>
    /// You <i><b>shouldn't</b></i> do any frame or render calculation in the method scope.
    /// </summary>
    private static void Startup()
    {
        FirstView.Load();
    }

    private static void BeforeFrame(float deltaTime)
    {
        
    }

    /// <summary>
    /// Computes a single game frame.
    /// </summary>
    /// <param name="deltaTime">DeltaTime IN SECONDS</param>
    private static void Frame(float deltaTime)
    {
        CurrentView?.Frame(deltaTime);
    }

    private static void AfterFrame(float deltaTime)
    {
        CurrentView?.AfterFrame(deltaTime);
    }

    #region Rendering

    private struct ViewportInfo
    {
        public Vector2 size;
        public Vector2 pos;
        public Vector2 scaleSize;
    }

    /// <summary>
    /// Prepares the viewport.
    /// </summary>
    /// <param name="deltaTime">DeltaTime IN SECONDS</param>
    private static void BeforeRender(out ViewportInfo vinf, float deltaTime)
    {
        int windowWidth = Raylib.GetScreenWidth();
        int windowHeight = Raylib.GetScreenHeight();

        int viewportWidth = 960;
        int viewportHeight = 720;

        // Initial scale based on height
        float scale = (float)windowHeight / viewportHeight;

        // Ensure the scaled width does not exceed the window width
        if (viewportWidth * scale > windowWidth)
        {
            scale = (float)windowWidth / viewportWidth;
        }

        // Compute final scaled viewport size
        int scaledViewportWidth = (int)(viewportWidth * scale);
        int scaledViewportHeight = (int)(viewportHeight * scale);

        // Center the viewport inside the window
        int viewportX = (windowWidth - scaledViewportWidth) / 2;
        int viewportY = (windowHeight - scaledViewportHeight) / 2;

        vinf = new ViewportInfo()
        {
            size = new Vector2(windowWidth, windowHeight),
            pos = new Vector2(viewportX, viewportY),
            scaleSize = new Vector2(scaledViewportWidth, scaledViewportHeight)
        };

        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);

        // Draw borders
        Raylib.DrawTexturePro(
            borderTexture,
            new Rectangle(0, 0, borderTexture.Width, borderTexture.Height),
            new Rectangle(0, 0, windowWidth, windowHeight),
            new Vector2(0, 0), 0f,
            Color.White
        );

        // Renders the viewport
        Raylib.BeginTextureMode(_renderTarget);
        Raylib.ClearBackground(Color.DarkGray);
    }

    /// <summary>
    /// Renders the game screen.
    /// </summary>
    /// <param name="deltaTime">DeltaTime IN SECONDS</param>
    private static void Render(float deltaTime)
    {
        CurrentView?.Render(deltaTime);
    }

    /// <summary>
    /// Finalizes the viewport.
    /// </summary>
    /// <param name="deltaTime">DeltaTime IN SECONDS</param>
    private static void AfterRender(ViewportInfo vinf, float deltaTime)
    {
        Raylib.EndTextureMode();

        // Draw the viewport texture scaled inside the window
        Raylib.DrawTexturePro(
            _renderTarget.Texture,
            new Rectangle(0, 0, vinf.size.X, -vinf.size.Y), // Flip Y-axis
            new Rectangle(vinf.pos.X, vinf.pos.Y, vinf.scaleSize.X, vinf.scaleSize.Y),
            new Vector2(0, 0),
            0f,
            Color.White
        );;

        Raylib.EndDrawing();
    }

    #endregion

    /// <summary>
    /// Renders the ImGui windows.
    /// </summary>
    /// <param name="deltaTime">DeltaTime IN SECONDS</param>
    private static void RenderGUI(float deltaTime)
    {
        rlImGui.Begin();

        // Do things.
        CurrentView?.RenderGUI(deltaTime);

        rlImGui.End();
    }

    #endregion
}
