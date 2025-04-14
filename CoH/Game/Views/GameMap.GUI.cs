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

                ImGui.EndMenu();
            }

            ImGui.EndMainMenuBar();
        }

        TextureWin();
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
