using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoH.GameData;

public enum SkillTarget
{
    Self,
    Opponent,
    Both,
    Terrain
}

[Serializable]
public unsafe struct Skilldata
{
    public string Name;
    public byte Element;
    public byte Power;
    public byte Accuracy;
    public byte Sp;
    public sbyte Priority;
    public byte Type;
    public ushort EffectId;
    public byte EffectChance;
    public SkillTarget EffectTarget; // 0 self; 1 opponent; 2 both; 3 terrain;
}
