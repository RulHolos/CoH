using CoH.GameData;
using ImGuiNET;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoH.Game.Views.Battles;

public enum BattleType
{
    Tutorial,
    Wild,
    Trainer,
}

public enum BattleState
{
    WaitingForInput,
    ResolvingAction,
    Switching,
    Victory,
    Defeat,
}

public enum AnimationState
{
    Idle,
    Attacking,
    TakingDamage,
    Fainting,
}

public static class StatusEffect
{
    public const byte None = 0;
}

public partial class Battle : View
{
    public override ILogger Logger { get; set; }

    private BattleType battleType;
    private BattleState battleState;

    private Tuple<AliveEcho, AliveEcho>? terrainEchoes;

    public Battle()
        : this(BattleType.Wild)
    {

    }

    public Battle(BattleType battleType)
        : base()
    {
        Logger = Log.ForContext("Tag", "Battle");
    }

    public override void Frame(float deltaTime)
    {
        
    }

    public override void Render(float deltaTime)
    {
        
    }

    public override void RenderGUI(float deltaTime)
    {
        
    }

    private void ResolveAction(ref AliveEcho attacker, ref AliveEcho defender, SkillData skill)
    {
        switch (skill.Type)
        {
            case SkillType.Skill:
                break;
            case SkillType.Item:
                break;
            case SkillType.Switch:
                break;
        }
    }

    private void AdjustSkill()
    {
        // Adjust skill accuracy, priority, etc... based on the lua file and current conditions of the fight.
    }

    private void RawDamage(ref AliveEcho attacker, ref AliveEcho defender, SkillData skill)
    {
        //int atkStat = () ? attacker: ;
        //int defStat;

        float leveldmg = ((2 * attacker.GetLevel()) / 5) + 2;
        //float powerAd = leveldmg * skill.Power * ();
    }
}
