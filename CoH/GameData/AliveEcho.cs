using CoH.Game.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoH.GameData;

[Serializable]
public unsafe struct AliveEcho : GUIDrawable
{
    public uint TrainerId; // If 0, then it's a wild puppet;
    public uint SecretId;
    public byte Cost;
    public int EXP;
    public string TrainerName;
    public ushort CaughtLocation;
    public DateTime CaughtTime;
    public string EchoNickname;
    public ushort EchoId;
    public EchoStyle EchoStyle;
    public byte AbilityIndex;
    public byte Mark;
    public fixed byte Ivs[6];
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

    public static AliveEcho GenerateRandom(BaseEcho seedEcho, byte targetLevel)
    {
        Random rnd = new();

        AliveEcho echo = new()
        {
            TrainerId = 0,
            SecretId = (uint)Random.Shared.Next(),
            Cost = seedEcho.Cost,
            EXP = GetMinExpForLevel(targetLevel, seedEcho.Cost),
            EchoNickname = seedEcho.Name,
            EchoId = seedEcho.Id,
            EchoStyle = seedEcho.Styles[rnd.Next(0, 3)],
            AbilityIndex = (byte)rnd.Next(0, 1),
            Mark = 0,
            CostumeIndex = 0,
            HeldItemId = 0,
            HeartMark = false,

        };

        return echo;
    }

    public void RenderGUI(float deltaTime)
    {

    }

    public int GetLevel()
    {
        for (int level = 1; level <= 100; level++)
        {
            int baseMultiplier = (Cost * 10) + 80;

            int requiredExp = GetMinExpForLevel(level, Cost);

            if (EXP < requiredExp)
                return level - 1;
        }

        return 1;
    }

    public static int GetMinExpForLevel(int level, byte cost)
    {
        float baseMultiplier = (cost * 10 + 80) / 100f;

        return cost switch
        {
            0 => (int)((baseMultiplier / 100.0) * (4 * Math.Pow(level, 3) / 5)), // Fast
            1 => (int)((baseMultiplier / 100.0) * Math.Pow(level, 3)), // Medium Fast
            2 => (int)((baseMultiplier / 100.0) * ((6.0 / 5.0) * Math.Pow(level, 3) - 15 * Math.Pow(level, 2) + 100 * level - 140)), // Medium Slow
            3 => (int)((baseMultiplier / 100.0) * (5 * Math.Pow(level, 3) / 4)), // Slow
            _ => throw new InvalidOperationException("Unknown Cost type.")
        };
    }
}
