using CoH.GameData;
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

    public Battle()
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
}
