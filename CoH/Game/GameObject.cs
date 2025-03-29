using DotTiled;
using System;
using System.Collections.Generic;
using System.Linq;
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
}
