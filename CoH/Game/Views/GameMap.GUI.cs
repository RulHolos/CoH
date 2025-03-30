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

    public override void RenderGUI(float deltaTime)
    {
        if (!ShowGUI)
            return;

        if (ImGui.BeginMainMenuBar())
        {
            ImGui.BeginMenu($"{Raylib.GetFPS()}FPS", false);

            if (ImGui.BeginMenu("Tools"))
            {
                ImGui.MenuItem("Textures", string.Empty, ref RenderTextureWin);

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

        if (ImGui.Begin("Loaded Textures"))
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
