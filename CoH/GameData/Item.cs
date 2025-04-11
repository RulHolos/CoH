using CoH.Game.Views;
using CsvHelper;
using CsvHelper.Configuration;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoH.GameData;

public enum ItemCategory
{
    Tools,
    Heal,
    Hold1,
    Hold2,
    Skills,
    Battle,
    Key,
}

public struct Item() : GUIDrawable
{
    public uint ItemId { get; set; } = 0;
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public uint Quantity { get; set; } = 1;
    public uint Maximum { get; set; } = 999;
    public ItemCategory Category { get; set; } = ItemCategory.Tools;
    public uint EffectId { get; set; } = 0;
    public uint SellPrice { get; set; } = 100;
    public bool CanBeDiscarded { get; set; } = true;

    public void RenderGUI(float deltaTime)
    {
        ImGui.PushID($"##Item{ItemId}");

        if (ImGui.TreeNode($"[ID {ItemId}] - {Name}"))
        {
            ImGui.Text($"Quantity = {Quantity}/{Maximum}");
            ImGui.Text($"Category = {Enum.GetName(Category)}");
            ImGui.Text($"EffectId = {EffectId}");
            ImGui.Text($"SellPrice = {SellPrice}");
            ImGui.Text($"Can Be Discarded = {CanBeDiscarded}");
            ImGui.TextWrapped(Description);

            ImGui.TreePop();
        }

        ImGui.PopID();
    }

    public override string ToString() => $"Item [ID {ItemId}] [Eff {EffectId}] {Name} - {Quantity}/{Maximum} - {Enum.GetName(typeof(ItemCategory), Category)}.";
}

public class ItemCsvMap : ClassMap<Item>
{
    public ItemCsvMap()
    {
        AutoMap(CultureInfo.InvariantCulture);
        Map(m => m.Quantity).Ignore();
    }
}