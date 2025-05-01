// Comment this line if you want to allow the use of the built-in Game Asset editor (events, echoes, trainers, ...)
// The editor doesn't allow editing of maps, images (sprite, animation)
#define EDITOR_MODE

using System;
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
using CoH.Game.Views;
using CoH.GameData;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CoH.Assets.DataSheets;
using CoH.Editor;

namespace CoH;

/*
 * For maps: Use Tiled and DotTiled(for parsing)
 * Put the player on the "Player" layer. It MUST be named "Player", not "player" or anything other.
 * Named layers:
 * Player (Object Layer) : Where to render the player in the tile order.
 * Events (Tile Layer) : All tiles in the event layer are treated as events, they have custom attributes on tiled.
 * 
 * For skills and other battle things, do a .lua support for attacks. Like, animating them and defining behaviour.
 * 
 * Hot reloading of files in maps and objects?
 * Like, do I hotreload the items when the file changes? Yes.
 */

public static class MainWindow
{
    public static Version? VersionNumber = Assembly.GetEntryAssembly()?.GetName().Version;
#if DEBUG
    // ONLY FOR DEBUG. Because I don't want to have to recompile every asset everytime.
    public static string PathToResources = Path.Combine("D:\\Bordel sans nom\\Trucs en rapport avec Myara\\Jeux\\Myara 2 (Itération 2)\\CoH\\CoH", "Assets");
    public static string PathToSave = Path.Combine("D:\\Bordel sans nom\\Trucs en rapport avec Myara\\Jeux\\Myara 2 (Itération 2)\\CoH\\CoH", "Save");
#else
    public static string PathToResources = Path.Combine(Directory.GetCurrentDirectory(), "Assets");
    public static string PathToSave = Path.Combine(Directory.GetCurrentDirectory(), "Save");
#endif
    public static Vector2 GameViewport;

    public static View FirstView { get; private set; } = new MainMenu();
    public static View? CurrentView { get; set; }

    public static GameEditor? GameEditor { get; private set; }

    public static ILogger CoreLogger = Log.ForContext("Tag", "Core");

    private static bool QuitFlag = false;

    /// <summary>
    /// Initializes the game window and starts the game loop.<br/>
    /// This is a blocking method.
    /// </summary>
    public static void Initialize()
    {
        CreateLogger();

        Config conf = Configuration.Load();
        SaveFile.Load();

        CreateRaylibContext();
        DataSheetsHandler.Load();

        while (!Raylib.IsWindowReady())
            continue;

        Startup();

        while (!Raylib.WindowShouldClose())
        {
            if (QuitFlag)
                break;

            float dt = Raylib.GetFrameTime();

            BeforeFrame(dt);
            Frame(dt);
            AfterFrame(dt);

            BeforeRender(dt);
            Render(dt);
#if DEBUG
            RenderGUI(dt);
#endif
            AfterRender(dt);
        }

        Dispose();

        rlImGui.Shutdown();
        Raylib.CloseAudioDevice();
        Raylib.CloseWindow();
    }

    private static unsafe void CreateLogger()
    {
        // Delete the log everytime it launches the game again.
        string pathToLog = Path.Combine(Directory.GetCurrentDirectory(), "Engine.log");
        if (File.Exists(pathToLog))
            File.Delete(pathToLog);

        const string template = "{Timestamp:dd-MM-yyyy HH:mm:ss} [{Level:u3}] [{Tag}] {Message:lj}{NewLine}{Exception}";

        Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
#endif
            .WriteTo.Console(outputTemplate: template)
            .WriteTo.File("Engine.log",
                rollingInterval: RollingInterval.Infinite,
                outputTemplate: template,
                rollOnFileSizeLimit: true)
            .CreateLogger();

        Raylib.SetTraceLogCallback(&RaylibLogBridge.LogCallback);

        CoreLogger.Debug("Launching the game in DEBUG mode. GUI will be accessible.");
    }

    private static void CreateRaylibContext()
    {
        ref Config conf = ref Configuration.Default;

        GameViewport = new(conf.WindowSizeX, conf.WindowSizeY);

        Raylib.SetConfigFlags(ConfigFlags.Msaa4xHint | ConfigFlags.HighDpiWindow | ConfigFlags.VSyncHint);
        Raylib.InitWindow(conf.WindowSizeX, conf.WindowSizeY, $"Myara 2 ~ Cycle of Hakurama | v{VersionNumber}");
        Raylib.SetExitKey(KeyboardKey.Null);
        Raylib.SetTargetFPS(120);

        Raylib.InitAudioDevice();

        rlImGui.Setup(true, true);
        ImGui.GetIO().ConfigWindowsMoveFromTitleBarOnly = true;
    }

    public static void Dispose()
    {
        CurrentView?.Unload();
    }

    #region GameLoop

    /// <summary>
    /// Initializes all the main things that needs to be initialized at startup.<br/>
    /// Only runs once BEFORE any frame or render are called.<br/>
    /// You <i><b>shouldn't</b></i> do any frame or render calculation in the method scope.
    /// </summary>
    private static void Startup()
    {
#if EDITOR_MODE
        GameEditor = new();
#endif

        FirstView.Load();
    }

    private static void BeforeFrame(float deltaTime)
    {
        CurrentView?.BeforeFrame(deltaTime);
    }

    /// <summary>
    /// Computes a single game frame.
    /// </summary>
    /// <param name="deltaTime">DeltaTime IN SECONDS</param>
    private static void Frame(float deltaTime)
    {
        CurrentView?.Frame(deltaTime);
        GameEditor?.Frame(deltaTime);
    }

    private static void AfterFrame(float deltaTime)
    {
        CurrentView?.AfterFrame(deltaTime);
    }

    #region Rendering

    /// <summary>
    /// Prepares the viewport.
    /// </summary>
    /// <param name="vinf">Viewport information generated by this method.</param>
    /// <param name="deltaTime">DeltaTime IN SECONDS</param>
    private static void BeforeRender(float deltaTime)
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);
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
    private static void AfterRender(float deltaTime)
    {
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

        //ImGui.ShowDemoWindow();

#if !EDITOR_MODE
        CurrentView?.RenderGUI(deltaTime);
#endif
        GameEditor?.RenderGUI(deltaTime);

        rlImGui.End();
    }

#endregion

    public static void QuitGame()
    {
        QuitFlag = true;
    }

    unsafe static class RaylibLogBridge
    {
        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static void LogCallback(int logLevel, sbyte* text, sbyte* args)
        {
            var message = Logging.GetLogMessage(new IntPtr(text), new IntPtr(args));

            ILogger raylibLog = Log.ForContext("Tag", "Raylib");

            switch ((TraceLogLevel)logLevel)
            {
                case TraceLogLevel.Debug:
                    raylibLog.Debug("{Message}", message);
                    break;
                case TraceLogLevel.Info:
                    raylibLog.Information("{Message}", message);
                    break;
                case TraceLogLevel.Warning:
                    raylibLog.Warning("{Message}", message);
                    break;
                case TraceLogLevel.Error:
                    raylibLog.Error("{Message}", message);
                    break;
                case TraceLogLevel.Fatal:
                    raylibLog.Fatal("{Message}", message);
                    break;
                default:
                    raylibLog.Information("{Message}", message);
                    break;
            }
        }
    }
}
