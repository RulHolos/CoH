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

namespace CoH.GameData;

public struct BaseEchoData()
{
    public ushort EchoId { get; set; } = 0;
    /// <summary>
    /// If EchoDexId is equal to -1, then it's invisible in the dex. Numbers are skipped if they're not linear.<br/>
    /// Meaning, if an echo is Id 4 and the next is Id 6, then there won't be an echo 5 unless specified and after 4 it'll jump to 6.
    /// </summary>
    public int EchoDexId { get; set; } = -1;
    /// <summary>
    /// Echo file name in the same directory with or without the xml extension.
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
public unsafe struct BaseEcho : GUIDrawable
{
    public ushort Id;
    public string Name;
    public string DexName;
    public byte Cost; // 0-4 inclusive. ((cost * 10) + 80)
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
            {
                EchoStyle style = Styles[i];
                if (ImGui.TreeNode($"{Enum.GetName(style.Type)} {Name}##Style{i}"))
                {
                    ImGui.Text($"Elements = {Enum.GetName(style.Element1)} ; {Enum.GetName(style.Element2)}");

                    if (ImGui.TreeNode($"Stats##Style{i}Echo{Id}"))
                    {
                        ImGui.Text($"HP = {style.BaseStats[0]}");
                        ImGui.Text($"FoAtk = {style.BaseStats[1]}");
                        ImGui.Text($"FoDef = {style.BaseStats[2]}");
                        ImGui.Text($"SpAtk = {style.BaseStats[3]}");
                        ImGui.Text($"SpDef = {style.BaseStats[4]}");
                        ImGui.Text($"Speed = {style.BaseStats[5]}");

                        ImGui.TreePop();
                    }

                    ImGui.TreePop();
                }
            }

            ImGui.TreePop();
        }

        ImGui.PopID();
    }
}

[InlineArray(4)]
public struct EchoStyleArray
{
    private EchoStyle _element0;
}

[Serializable]
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