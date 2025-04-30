using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CoH.Game.Views;

/// <summary>
/// Base class for views. (Main Menu, map, battle, ...)
/// </summary>
public abstract class View : GUIDrawable, AssetConsumer
{
    public View? PreviousView { get; private set; }
    public View? NextView { get; private set; }
    public int Timer { get; set; }
    public DialogManager DialogManager { get; private set; }

    public bool Lock { get; set; } = false;

#if DEBUG
    public bool ShowGUI = true;
#else
    public bool ShowGUI = false;
#endif

    public virtual ILogger Logger { get; set; }

    /// <summary>
    /// Your DON'T WANT to call <see cref="Load"/> in this constructor.<br/>
    /// Creates a View instance with a Logger tag.
    /// </summary>
    public View(string tag)
    {
        DialogManager = new();
        Logger = CreateLogger(tag);
    }

    /// <summary>
    /// Your DON'T WANT to call <see cref="Load"/> in this constructor.
    /// </summary>
    public View()
        : this("")
    {

    }

    /// <summary>
    /// Initializes the view.
    /// </summary>
    public virtual void Load()
    {
        MainWindow.CurrentView = this;
    }

    /// <summary>
    /// Unloads the contents of the view. (Images, music, ...)
    /// </summary>
    public virtual void Unload()
    {

    }

    public abstract void Frame(float deltaTime);
    public abstract void Render(float deltaTime);
    public abstract void RenderGUI(float deltaTime);

    public virtual void BeforeFrame(float deltaTime)
    {
        
    }

    public virtual void AfterFrame(float deltaTime)
    {
        Timer++;
    }

    public void SetNextView(View nextView)
    {
        NextView = nextView;
    }

    public void GoToNextView(View? view = null)
    {
        if (NextView == null && view == null)
        {
            Logger.Error("Unable to switch view, there is no Next View.");
            return;
        }

        try
        {
            Unload();
            if (view != null)
                view.Load();
            else
                NextView?.Load();
        }
        catch (Exception e)
        {
            Logger.Error($"Unable to switch views because of the following reason:\n{e}");
        }
        finally
        {
            NextView?.Unload();
        }
    }

    protected ILogger CreateLogger(string tag)
    {
        return Log.ForContext("Tag", tag);
    }
}

public interface GUIDrawable
{
    public void RenderGUI(float deltaTime);
}

public interface AssetConsumer
{
    public void Load();
    public void Unload();
}