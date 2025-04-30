using CoH.Game.Views;
using Raylib_cs;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotTiled;

namespace CoH.Editor;

public class GameEditor : GUIDrawable
{
    private bool ShowEditor = false;

    public void Frame(float deltaTime)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Delete))
            ShowEditor = !ShowEditor;
    }

    public void RenderGUI(float deltaTime)
    {
        ImGui.DockSpaceOverViewport();

        if (ImGui.BeginMainMenuBar())
        {
            ImGui.BeginMenu("fd");

            ImGui.EndMainMenuBar();
        }

        if (ImGui.Begin("Game Editor"))
        {
            ImGui.End();
        }
    }
}
