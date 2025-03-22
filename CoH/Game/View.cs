﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoH.Game;

/// <summary>
/// Base class for views. (Main Menu, map, battle, ...)
/// </summary>
public abstract class View
{
    public View? PreviousView { get; private set; }
    public View? NextView { get; private set; }
    public ulong Timer { get; set; }

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
    public abstract void RenderGUI(float deltaTime);

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
            Console.Error.WriteLine("Unable to switch view, there is no Next View.");
            return;
        }
        try
        {
            this.Unload();
            if (view != null)
                view.Load();
            else
                NextView?.Load();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Unable to switch views because of the following reason:\n{e}");
        }
    }
}
