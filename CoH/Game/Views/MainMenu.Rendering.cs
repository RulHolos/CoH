using CoH.Game.Ext;
using DotTiled;
using ImGuiNET;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoH.Game.Views;

public partial class MainMenu
{
    private int widgetIndex = 0;
    private Widget CurrentWidget => widgets[widgetIndex];
    private readonly CircularList<Widget> widgets = [];

    private class Widget(int index, bool enabled, Action? callback = null)
    {
        // Index determines the bound of the image.
        // When loading the image, index will devide the image in the height of the image devided by Index. Width is always maximum.
        public int Index = index;
        public bool Disable = !enabled;
        public float ShakeTimer = 0;
        public float SelectTimer = 0;
        public Action? Callback = callback;
    }

    public override void Render(float deltaTime)
    {

    }

    public override void RenderGUI(float deltaTime)
    {
        if (!ShowGUI)
            return;

        if (ImGui.BeginMainMenuBar())
        {
            ImGui.BeginMenu($"MainMenu [{widgets.Count} Options] - {Raylib.GetFPS()}FPS", false);

            if (ImGui.BeginMenu("Tools"))
            {
                ImGui.EndMenu();
            }

            ImGui.EndMainMenuBar();
        }
    }
}
