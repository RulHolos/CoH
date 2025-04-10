using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoH.Game.Ext;

/// <summary>
/// Represents a generic circular list.<br/>
/// Taken from <see href="https://stackoverflow.com/a/71235552"/>
/// </summary>
public class CircularList<T> : List<T>
{
    public int Index;

    public T Current()
    {
        return this[Index];
    }

    public T Previous()
    {
        Index--;
        if (Index < 0)
            Index = Count - 1;

        return this[Index];
    }

    public T PreviousSkip(Func<int, bool> skipCommand)
    {
        Previous();
        if (skipCommand(Index))
            Previous();

        return this[Index];
    }

    public T Next()
    {
        Index++;
        Index %= Count;

        return this[Index];
    }

    public T NextSkip(Func<int, bool> skipCommand)
    {
        Next();
        if (skipCommand(Index))
            Next();

        return this[Index];
    }

    public void Reset()
    {
        Index = 0;
    }

    public void MoveToEnd()
    {
        Index = Count - 1;
    }
}
