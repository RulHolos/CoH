using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoH.GameData;

[Serializable]
public unsafe struct AliveEcho
{
    public uint TrainerId;
    public uint SecretId;
    public string TrainerName;
    public ushort CaughtLocation;
    public DateTime CaughtTime;
    public string EchoNickname;
    public ushort EchoId;
    public StyleType EchoStyle;
    public byte AbilityIndex;
    public byte Mark;
    public fixed byte Ivs[3];
    public uint Experience;
    public ushort Happi;
    public ushort pp;
    public byte CostumeIndex;
    public fixed byte Evs[6];
    public ushort HeldItemId;
    public fixed ushort Skills[4];
    public bool HeartMark;
    public ushort HP; // Current HP, not max.
    public fixed byte SkillPoints[4]; // Remaining uses for skills.
    public fixed byte StatusEffect[2];
}
