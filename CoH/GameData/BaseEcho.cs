using CoH.Game.Views;
using CsvHelper.Configuration;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace CoH.GameData;

public struct BaseEchoData()
{
    /// <summary>
    /// Id of the echo, it's also the number used to calculate the position on the image sheet for the chibi echo (in menu icons)
    /// </summary>
    public ushort EchoId { get; set; } = 0;
    /// <summary>
    /// If EchoDexId is equal to -1, then it's invisible in the dex. Numbers are skipped if they're not linear.<br/>
    /// Meaning, if an echo is Id 4 and the next is Id 6, then there won't be an echo 5 unless specified and after 4 it'll jump to 6.
    /// </summary>
    public int EchoDexId { get; set; } = -1;
    /// <summary>
    /// Echo file name in the same directory with or without the xml extension.<br/>
    /// This is also the name of the file of the image of the Echo.
    /// </summary>
    public string FileName { get; set; } = "Echo";
}

public class BaseEchoDataMap : ClassMap<BaseEchoData>
{
    public BaseEchoDataMap()
    {
        AutoMap(CultureInfo.InvariantCulture);
    }
}

/// <summary>
/// "Unalive" echo.<br/>
/// Basically a blueprint to create an echo when starting a wild battle.
/// </summary>
[Serializable]
public unsafe struct BaseEcho(ushort id) : GUIDrawable
{
    public ushort Id = id;
    public string Name = "New Echo";
    public string DexName = "New Echo Full Name";
    public byte Cost; // 0-3 inclusive. ((cost * 10) + 80). Also dictates the level rank. Higher cost means slower leveling.
    public fixed ushort BaseSkills[5];
    public fixed ushort ItemDropTable[4];
    public int EchoDexIndex;
    public EchoStyleArray Styles;

    public void RenderGUI(float deltaTime)
    {
        ImGui.PushID($"##Echo{Id}");

        if (ImGui.TreeNode($"[ID {Id}/{EchoDexIndex}] - {Name} - {DexName}"))
        {
            ImGui.Text($"Cost = {Cost} / {(Cost * 10) + 80}");
            
            if (ImGui.TreeNode($"Base Skills##BaseSkills{Id}"))
            {
                for (int i = 0; i < 5; i++)
                    ImGui.Text($"{BaseSkills[i]}");

                ImGui.TreePop();
            }

            for (int i = 0; i < 4; i++)
                Styles[i].RenderGUI(deltaTime, i, Name, Id);

            ImGui.TreePop();
        }

        ImGui.PopID();
    }

    public int GetRealCost() => (Cost * 10) + 80;
}

[InlineArray(4)]
public struct EchoStyleArray
{
    private EchoStyle _element0;
}

[Serializable]
/// Doesn't implement the interface because the method signature is different.
public unsafe struct EchoStyle()
{
    public StyleType Type = StyleType.Normal;
    public Element Element1, Element2 = Element.Void;
    public fixed byte BaseStats[6];
    public fixed byte Abilities[2];
    public fixed ushort StyleSkills[11];
    public ushort Level100Skill = 0;
    public fixed byte SkillCardBitfield[16];
    public fixed ushort Level70Skills[8];
    public StyleMeta Meta = StyleMeta.None;

    public void RenderGUI(float deltaTime, int i, string Name, int Id)
    {
        if (ImGui.TreeNode($"{Enum.GetName(Type)} {Name}##Style{i}"))
        {
            ImGui.Text($"Elements = {Enum.GetName(Element1)} ; {Enum.GetName(Element2)}");

            if (ImGui.TreeNode($"Stats##Style{i}Echo{Id}"))
            {
                ImGui.Text($"HP = {BaseStats[0]}");
                ImGui.Text($"FoAtk = {BaseStats[1]}");
                ImGui.Text($"FoDef = {BaseStats[2]}");
                ImGui.Text($"SpAtk = {BaseStats[3]}");
                ImGui.Text($"SpDef = {BaseStats[4]}");
                ImGui.Text($"Speed = {BaseStats[5]}");

                ImGui.TreePop();
            }

            ImGui.Text($"Abilities: {Ability.GetAbilityNameFromId(Abilities[0])} ; {Ability.GetAbilityNameFromId(Abilities[1])}");

            if (ImGui.TreeNode($"Skills##Style{i}Echo{Id}"))
            {
                for (int j = 0; j < 11; j++)
                    ImGui.Text($"{StyleSkills[j]}");

                ImGui.TreePop();
            }

            ImGui.Text($"Level 100 Skill = {Level100Skill}");

            ImGui.Text($"Meta = {GetMetaNames()}");

            ImGui.TreePop();
        }
    }

    public string GetMetaNames()
    {
        var result = new List<string>();
        var flagValue = Convert.ToInt64(Meta);

        foreach (StyleMeta value in Enum.GetValues(typeof(StyleMeta)))
        {
            long longValue = Convert.ToInt64(value);
            if (longValue != 0 && (flagValue & longValue) == longValue)
                result.Add(value.ToString());
        }

        if (result.Count == 0)
            return "None";
        return string.Join(" | ", result);
    }
}

public enum StyleType
{
    Normal,
    Power,
    Defense,
    Speed,
    Assist,
    Extra
}

[Flags]
public enum StyleMeta
{
    None,
    HasOtherForms, // Has multiple forms (Can change forms with abilities and certain actions or triggers)
}