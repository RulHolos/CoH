using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CoH.Assets.DataSheets;
using CoH.Game.Ext;
using CoH.GameData;
using ImGuiNET;
using Serilog.Core;

namespace CoH.Editor;

public partial class GameEditor
{
    private bool ShowEchoes = false;
    private ushort? SelectedEchoId = null;

    private void RenderEchoes()
    {
        if (!ShowEchoes)
            return;

        if (!ImGui.Begin("Echoes", ref ShowEchoes, ImGuiWindowFlags.MenuBar))
        {
            ImGui.End();
            return;
        }

        Echo_RenderMenu();

        Echo_RenderList();
        ImGui.SameLine();
        Echo_RenderEditor();

        ImGui.End();
    }

    private void Echo_RenderMenu()
    {
        // TODO
        if (ImGui.BeginMenuBar())
        {
            ImGui.EndMenuBar();
        }
    }

    private void Echo_RenderList()
    {
        ImGui.BeginGroup();
        {
            if (ImGui.BeginChild("echoes_list", new Vector2(240, 0) - new Vector2(26) with { X = 0}, ImGuiChildFlags.Borders))
            {
                for (ushort i = 0; i < DataSheetsHandler.Echoes.Count; i++)
                {
                    ImGui.PushID($"Echo-{i}");

                    if (ImGui.Selectable($"#{DataSheetsHandler.Echoes[i].Id:000}> {DataSheetsHandler.Echoes[i].Name}##Echo_{i}", SelectedEchoId == i))
                    {
                        SelectedEchoId = i;
                    }

                    ImGui.PopID();
                }

                ImGui.EndChild();
            }

            if (ImGui.Button("New Echo"))
            {
                DataSheetsHandler.Echoes.Add(new BaseEcho((ushort)DataSheetsHandler.Echoes.Count));
            }
        }
        ImGui.EndGroup();
    }

    private void Echo_RenderEditor()
    {
        if (SelectedEchoId == null)
            return;

        var listSpan = CollectionsMarshal.AsSpan(DataSheetsHandler.Echoes);
        ref BaseEcho echo = ref listSpan[(ushort)SelectedEchoId];

        ImGui.BeginGroup();
        {
            if (ImGui.BeginChild("echo_editor", Vector2.Zero - new Vector2(26) with { X = 0 }, ImGuiChildFlags.Borders))
            {
                // ### Base Values ### //
                ImGui.Columns(3, "echo_editor_base", false);
                ImGui.InputText("Name", ref echo.Name, 1024);
                ImGui.InputText("Dex Name", ref echo.DexName, 1024);
                ImGui.NextColumn();
                ImGuiEx.ComboByte(ref echo.Cost, ["0", "1", "2", "3"]);
                if (ImGui.InputInt("Dex Index", ref echo.EchoDexIndex, 1, 5, ImGuiInputTextFlags.CharsDecimal))
                    echo.EchoDexIndex = Math.Clamp(echo.EchoDexIndex, -1, int.MaxValue);
                ImGui.NextColumn();
                ImGui.Columns(1);

                // ### Styles ### //
                ImGui.SeparatorText("Styles");

                ImGui.EndChild();
            }

            if (ImGui.Button("Save Echo"))
                Echo_SaveEcho();
        }
        ImGui.EndGroup();
    }

    private void Echo_SaveEcho()
    {
        if (SelectedEchoId == null)
            return;

        ushort echoId = (ushort)SelectedEchoId;
        BaseEcho echo = DataSheetsHandler.Echoes[echoId];
        string PathToEchoes = Path.Combine(MainWindow.PathToResources, "Echoes");

        if (!DataSheetsHandler.EchoesEntry.Any(x => x.EchoId == SelectedEchoId))
        {
            DataSheetsHandler.EchoesEntry.Add(new()
            {
                EchoId = echoId,
                EchoDexId = echoId,
                FileName = DataSheetsHandler.Echoes[echoId].Name, // Same filename as name by default.
            });
        }

        var tmp = DataSheetsHandler.EchoesEntry[(ushort)SelectedEchoId];
        tmp.EchoDexId = echo.EchoDexIndex;
        DataSheetsHandler.EchoesEntry[(ushort)SelectedEchoId] = tmp;

        if (!DataSheetsHandler.SaveCsv<BaseEchoData, BaseEchoDataMap>("Echoes", DataSheetsHandler.EchoesEntry))
        {
            Logger.Error($"Couldn't save echoes data.");
            return;
        }

        BaseEchoParser.WriteToXml(echo, Path.Combine(PathToEchoes, $"{Path.ChangeExtension(echo.Name, ".xml")}"));
    }
}
