using CoH.Game.Views;
using CsvHelper.Configuration;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CoH.GameData;

public enum SkillTarget
{
    Self,
    Opponent,
    Both,
    Terrain
}

public enum SkillType
{
    Skill,
    Item,
    Switch,
}

public enum Element
{
    Void, Fire, Water, Nature, Earth, Steel, Wind, Electric,
    Light, Dark, Nether, Poison, Fighting, Illusion, Sound, Warped
}

[Serializable]
public unsafe struct SkillData() : GUIDrawable
{
    public uint Id { get; set; } = 0;
    public string Name { get; set; } = string.Empty;
    public Element Element { get; set; } = Element.Void;
    public byte Power { get; set; } = 50;
    public byte Accuracy { get; set; } = 100;
    public byte Sp { get; set; } = 20;
    public sbyte Priority { get; set; } = 0;
    public SkillType Type { get; set; } = SkillType.Skill;
    public ushort EffectId { get; set; } = 0;
    public byte EffectChance { get; set; } = 100;
    public SkillTarget EffectTarget { get; set; } = SkillTarget.Opponent;

    public void RenderGUI(float deltaTime)
    {
        ImGui.PushID($"##SkillData{Name}");

        if (ImGui.TreeNode($"[ID {Id}] - {Name}"))
        {
            ImGui.Text($"Element = {Enum.GetName(Element)}");
            ImGui.Text($"Power = {Power}");
            ImGui.Text($"Accuracy = {Accuracy}");
            ImGui.Text($"Sp = {Sp}");
            ImGui.Text($"Priority = {Priority}");
            ImGui.Text($"Type = {Enum.GetName(Type)}");
            ImGui.Text($"EffectId = {EffectId}");
            ImGui.Text($"EffectChance = {EffectChance}");
            ImGui.Text($"EffectTarget = {Enum.GetName(EffectTarget)}");

            ImGui.TreePop();
        }

        ImGui.PopID();
    }
}

public class SkillDataMap : ClassMap<SkillData>
{
    public SkillDataMap()
    {
        AutoMap(CultureInfo.InvariantCulture);
    }
}