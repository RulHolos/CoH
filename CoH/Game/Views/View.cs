﻿using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoH.Game.Views;

/// <summary>
/// Base class for views. (Main Menu, map, battle, ...)
/// </summary>
public abstract class View
{
    public View? PreviousView { get; private set; }
    public View? NextView { get; private set; }
    public int Timer { get; set; }

    public bool Lock { get; set; } = false;

#if DEBUG
    public bool ShowGUI = true;
#else
    public bool ShowGUI = false;
#endif

    public abstract ILogger Logger { get; set; }

    /// <summary>
    /// Your DON'T WANT to call <see cref="Load"/> in this constructor.
    /// </summary>
    public View()
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
    public virtual void RenderGUI(float deltaTime) { }

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
}