using CoH.Game.Views;
using Raylib_cs;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotTiled;
using Serilog;

namespace CoH.Editor;

public partial class GameEditor : GUIDrawable
{
    public static ILogger Logger = Log.ForContext("Tag", "Asset Editor");

    public void Frame(float deltaTime)
    {
    }

    public void RenderGUI(float deltaTime)
    {
        // Doesn't allow displaying the actual game? What??!!
        //ImGui.DockSpaceOverViewport();

        if (ImGui.BeginMainMenuBar())
        {
            ImGui.MenuItem("Game Assets Editor", string.Empty, false, false);

            if (ImGui.BeginMenu("Echoes"))
            {
                ImGui.MenuItem("Echo Editor", string.Empty, ref ShowEchoes);

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Abilities & Skills"))
            {
                ImGui.MenuItem("Ability Editor", string.Empty);
                ImGui.MenuItem("Skill Editor", string.Empty);

                ImGui.EndMenu();
            }

            ImGui.EndMainMenuBar();
        }

        RenderEchoes();
    }
}
