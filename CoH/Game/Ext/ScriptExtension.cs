using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoH.Game.Ext;

public static class ScriptExt
{
    /// <summary>
    /// Registers the given delegates into the Global variable of the script.
    /// </summary>
    /// <param name="script">The script instance</param>
    /// <param name="funcs">A delegate or a list of delegates to register</param>
    /// <exception cref="ArgumentNullException">If one or more delegates are null</exception>
    /// <returns>The instance of <paramref name="script"/> for chaining purposes.</returns>
    public static Script RegisterDelegates(this Script script, params Delegate[] funcs)
    {
        ArgumentNullException.ThrowIfNull(funcs);

        foreach (Delegate func in funcs)
        {
            ArgumentNullException.ThrowIfNull(func);
            script.Globals[func.Method.Name] = func;
        }

        return script;
    }
}