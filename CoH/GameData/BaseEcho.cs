using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CoH.GameData;

/// <summary>
/// "Unalive" echo.<br/>
/// Basically a blueprint to create an echo when starting a wild battle.
/// </summary>
[Serializable]
public unsafe struct BaseEcho
{
    public string Name;
    public byte Cost; // 0-4 inclusive. ((cost * 10) + 80)
    public fixed ushort BaseSkills[5];
    public fixed ushort ItemDropTable[4];
    public ushort EchoDexIndex;
    public EchoStyleArray Styles;

    public static bool operator ==(BaseEcho a, BaseEcho b) => a.Name == b.Name;
    public static bool operator !=(BaseEcho a, BaseEcho b) => a.Name != b.Name;
}

[InlineArray(4)]
public struct EchoStyleArray
{
    private EchoStyle _element0;
}

[Serializable]
public unsafe struct EchoStyle
{
    public StyleType Type;
    public byte Element1, Element2;
    public fixed byte BaseStats[6];
    public fixed byte Abilities[2];
    public fixed ushort StyleSkills[11];
    public ushort Level100Skill;
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