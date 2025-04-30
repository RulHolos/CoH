using DotTiled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CoH.Game;

/// <summary>
/// Base class for Game Objects (players, trainers, events, ...)
/// </summary>
public abstract class GameObject
{
    public virtual void Load() { }
    public virtual void Unload() { }

    public abstract void Frame(float dt);
    public abstract void Render(float dt);
    public virtual void RenderGUI(float dt) { }
}

public struct MapObject
{
    public int Id { get; set; }
    public Vector2 Position { get; set; }
    public string Image { get; set; }
    public ObjMovementType Movement { get; set; }
    public string Event { get; set; }
    public bool SavePosition { get; set; } // If false, the position returns to its default one on map loading. (Used for random roaming)
}

public enum ObjMovementType
{
    FacingUp,
    FacingDown,
    FacingLeft,
    FacingRight,
    RandomRoaming,
}