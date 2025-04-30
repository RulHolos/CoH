using CoH.Game.Views.Battles;
using DotTiled;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CoH.Game.Views;

public partial class GameMap
{
    private bool RenderTextureWin = false;
    public bool IgnoreCollisions = false;
    public bool RenderDialogManager = false;
    public bool RenderEventManager = false;

    public override void RenderGUI(float deltaTime)
    {
        if (!ShowGUI)
            return;

        if (ImGui.BeginMainMenuBar())
        {
            ImGui.BeginMenu($"MAP: [ID {MapId}] - {Raylib.GetFPS()}FPS", false);

            if (ImGui.BeginMenu("Tools"))
            {
                ImGui.MenuItem("Textures", string.Empty, ref RenderTextureWin);
                ImGui.MenuItem("Ignore Collisions", "F2", ref IgnoreCollisions);
                if (ImGui.MenuItem("Start Battle"))
                    GoToNextView(new Battle());

                Player.RenderGUI(deltaTime);

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Dialog"))
            {
                ImGui.MenuItem("Debugger", string.Empty, ref RenderDialogManager);

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Events"))
            {
                ImGui.MenuItem("Debugger", string.Empty, ref RenderEventManager);

                ImGui.EndMenu();
            }

            ImGui.EndMainMenuBar();
        }

        TextureWin();
        if (RenderDialogManager) DialogManager.RenderGUI(deltaTime);
        if (RenderEventManager) CurrentEvent?.RenderGUI(deltaTime);
    }

    private void TextureWin()
    {
        if (!RenderTextureWin)
            return;

        if (ImGui.Begin("Loaded Textures", ref RenderTextureWin))
        {
            if (ImGui.CollapsingHeader("Tilesets"))
            {
                foreach (Texture2D texture in Tilesets)
                {
                    ImGui.Text($"{texture.Id}");
                    rlImGui.Image(texture);
                }
            }

            ImGui.End();
        }
    }
}
