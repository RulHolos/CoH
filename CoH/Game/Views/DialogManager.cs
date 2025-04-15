using CoH.GameData;
using ImGuiNET;
using Raylib_cs;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoH.Game.Views;

/*
 * For the dialog start:
 * First line if the name of the speaker, second line is the portrait image.
 * 
 * For the dialog cues:
 * \p : Name of the player.
 * \b : Waits for input before continuing text.
 * \c : Clears the whole dialog box.
 * \q : Yes/No box.
 * \s : Indicates that this line and the one after are new speaker (change the speaker name and its image). First line: image, second line: speaker name.
 * \x : Change the dialog image without changing the name. Used for different expressions.
 */

/// <summary>
/// Class for managing Dialogs of a view.<br/>
/// Every view has a DialogManager instance attached to it.
/// </summary>
public class DialogManager : AssetConsumer
{
    public virtual ILogger Logger { get; set; }
    public bool DialogFinished => false;

    private Dictionary<string, Texture2D> DialogImages { get; set; } = [];

    private int dialogId { get; set; }
    private string[]? currentDialog { get; set; }
    private int lineIndex = 0;

    // Doesn't inherit base() because it would create a cyclic class reference,
    // since View already creates a DialogManager instance.
    public DialogManager()
    {
        Logger = Log.ForContext("Tag", "Dialog -");
    }

    /// <summary>
    /// Loads all of the dialog images.
    /// </summary>
    public void Load()
    {
        Logger = Log.ForContext("Tag", $"Dialog {dialogId}");

        for (int i = 0; i < currentDialog!.Length; i++)
        {
            if (currentDialog[i].Length > "\\c".Length) // idk what I'm doing so this is more a failsafe than anything.
            {
                string commandCharacter = currentDialog[i][0..2];
                if (commandCharacter.Contains("\\c") || commandCharacter.Contains("\\x"))
                {
                    Logger.Debug("Contains \\c or \\x");
                    //Raylib.LoadTexture(Get the image path and load it);
                }
            }
        }
    }

    public void Unload()
    {
        foreach (Texture2D tex in DialogImages.Values)
            Raylib.UnloadTexture(tex);
        DialogImages.Clear();
    }

    public void Frame(float deltaTime)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.W))
            return; // TODO: Pass dialog with \b.
    }

    public void Render(float deltaTime)
    {
        
    }

    private int debugTextId = 0;
    public void RenderGUI(float deltaTime)
    {
        if (ImGui.Begin("Dialog Debugger"))
        {
            if (ImGui.CollapsingHeader("Current Dialog"))
            {
                for (int i = 0; i < currentDialog?.Length; i++)
                {
                    ImGui.Text($"({i})> {currentDialog[i]}");
                }
            }
            if (ImGui.CollapsingHeader("Parsed Current Dialog"))
            {
                for (int i = 0; i < currentDialog?.Length; i++)
                {
                    ImGui.Text($"({i})> {ParseDialogLine(currentDialog[i])}");
                }
            }
            ImGui.Spacing();
            ImGui.Text($"Current line index: {lineIndex}");
            if (ImGui.Button("Reset Current Dialog"))
            {
                lineIndex = 0;
            }

            ImGui.SeparatorText("Start dialogs");

            ImGui.InputInt("Dialog ID", ref debugTextId);
            if (ImGui.Button("Start dialog"))
            {
                GetDialog(debugTextId);
            }

            ImGui.End();
        }
    }

    /// <summary>
    /// Gets the dialog content from the dialog id and reset the dialog manager state.
    /// </summary>
    /// <param name="id">The dialog id to display.</param>
    public void GetDialog(int id)
    {
        string fileName = $"{id:00000}.txt";
        string filePath = Path.Combine(MainWindow.PathToResources, "Text", fileName[0..3], fileName);
        if (File.Exists(filePath))
        {
            Unload();

            dialogId = id;
            using StreamReader sr = new(filePath);
            string fileContent = sr.ReadToEnd();
            currentDialog = fileContent.Split("\n", options: StringSplitOptions.TrimEntries);
            lineIndex = 0;

            Load();
        }
        else
            Logger.Debug($"Dialog {fileName} doesn't exist");
    }

    /// <summary>
    /// Gets the dialog content from the dialog id and reset the dialog manager state.
    /// </summary>
    /// <param name="id">The dialog id to display.</param>
    public void GetDialog(string id) => GetDialog(int.Parse(id));

    public string ParseDialogLine(string? lineToParse = null)
    {
        string parsedLine = lineToParse ?? currentDialog![lineIndex];

        parsedLine = parsedLine.Replace("\\p", SaveFile.SaveData.TrainerName);

        if (lineToParse == null)
            lineIndex++;

        return parsedLine;
    }
}
