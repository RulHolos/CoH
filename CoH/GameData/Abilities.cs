using CoH.Assets.DataSheets;
using CoH.Game.Views;
using CsvHelper.Configuration;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoH.GameData;

[Serializable]
public struct Ability() : GUIDrawable
{
    public int Id { get; set; } = 0;
    public string Name { get; set; } = "None";
    public string Description { get; set; } = "None";

    public void RenderGUI(float deltaTime)
    {
        ImGui.PushID($"##Ability{Id}");

        if (ImGui.TreeNode($"[ID {Id}] - {Name}"))
        {
            ImGui.Text($"Lua exists = {File.Exists(Path.Combine(MainWindow.PathToResources, "Abilities", Path.ChangeExtension(Name, "lua")))}");
            ImGui.TextWrapped($"Description = {Description}");

            ImGui.TreePop();
        }

        ImGui.PopID();
    }

    public static string GetAbilityNameFromId(int Id) => DataSheetsHandler.Abilities.Find(x => x.Id == Id).Name;
}


public class AbilityMap : ClassMap<Ability>
{
    public AbilityMap()
    {
        AutoMap(CultureInfo.InvariantCulture);
    }
}