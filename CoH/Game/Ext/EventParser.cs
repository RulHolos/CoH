using CoH.Game.Views;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using Serilog;
using CoH.GameData;

namespace CoH.Game.Ext;

public class ScriptedEvent : GUIDrawable
{
    public ILogger? Logger { get; set; }

    private Script? script;
    private string eventPath = string.Empty;
    private Coroutine? curCoroutine;

    public ScriptedEvent()
    {
        Logger = Log.ForContext("Tag", "ScriptedEvent");
    }

    public ScriptedEvent(string filePath)
        : base()
    {
        LoadEvent(filePath);
    }

    public void LoadEvent(string filePath)
    {
        filePath = Path.Combine(MainWindow.PathToResources, "Events", Path.ChangeExtension(filePath, ".lua"));
        if (File.Exists(filePath))
        {
            eventPath = filePath;
            script = new();
            script.RegisterDelegates(Text, SetFlag, GetFlag);

            script.DoFile(filePath);
            Logger?.Information($"Event {filePath} loaded");
        }
        else
        {
            Logger?.Error($"Event {filePath} doesn't exist");
        }
    }

    public void Interact()
    {
        if (script == null)
            return;

        DynValue? func = script.Globals.Get("Interact");

        if (func?.Function != null)
        {
            Coroutine co = script.CreateCoroutine(func.Function).Coroutine;
            curCoroutine = co;

            co.Resume();
        }
        else
            Logger?.Warning($"Event has no Interact function.");
    }
    
    #region Script Globals

    public DynValue Text(string textPath, bool yesno = false)
    {
        DialogManager dm = MainWindow.CurrentView!.DialogManager;

        DynValue yield = DynValue.NewYieldReq(null);

        dm.GetDialog(textPath, yesno, (bool yesnoresult) =>
        {
            if (curCoroutine!.State == CoroutineState.Suspended)
                curCoroutine.Resume(DynValue.NewBoolean(yesnoresult));
        });

        return yield;
    }

    /// <summary>
    /// Sets a flag value into the save file.
    /// </summary>
    /// <param name="key">The name of the file</param>
    /// <param name="value">The value of the flag</param>
    /// <returns>The value of <paramref name="value"/></returns>
    public static bool SetFlag(string key, bool value)
    {
        SaveFile.SaveData.SetFlag(key, value);
        return value;
    }

    /// <summary>
    /// Gets a value for the matching flag.
    /// </summary>
    /// <param name="key">The name of the flag</param>
    /// <returns>The value of the flag. Returns false if the flag doesn't exist.</returns>
    public static bool GetFlag(string key)
    {
        return SaveFile.SaveData.GetFlag(key);
    }

    public static void WildBattle(int EchoId, int styleNumber, int level)
    {

    }

    #endregion
    #region GUI

    private string debugEventId = string.Empty;
    public void RenderGUI(float deltaTime)
    {
        DynValue? func = script?.Globals.Get("Interact");
        DynValue? onWalk = script?.Globals.Get("WalkOn");

        if (ImGui.Begin("Event Debugger"))
        {
            ImGui.Text($"Current event: {Path.GetFileName(eventPath)}");

            ImGui.Spacing();
            ImGui.SeparatorText("Start events");

            ImGui.InputText("Event Name", ref debugEventId, 2024);
            if (ImGui.Button("Load event"))
            {
                LoadEvent(debugEventId);
            }

            ImGui.BeginDisabled(func?.Function == null || script == null);
            if (ImGui.Button("Interact"))
            {
                Interact();
            }
            ImGui.EndDisabled();
            ImGui.SameLine();
            ImGui.BeginDisabled(onWalk?.Function == null || script == null);
            if (ImGui.Button("WalkOn"))
            {
                if (onWalk?.Function != null)
                    script!.Call(onWalk.Function);
                else
                    Logger?.Warning($"Event {debugEventId} has no WalkOn function.");
            }
            ImGui.EndDisabled();

            ImGui.End();
        }
    }

    #endregion
}