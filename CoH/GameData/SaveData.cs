using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CoH.GameData;

public struct SaveData
{
    public string TrainerName;
    public uint TrainerId;
    public bool Gender; // True = Female; False = Male
    public uint Money;
    public Vector2 PositionOnMap;
    public sbyte FacingDir; // 0 = Left, 1 = Right, 2 = Top, 3 = Bottom
    public Party PartyEchoes; // No more than 6.
}

[InlineArray(6)]
public struct Party
{
    private AliveEcho _element0;
}

/// <summary>
/// Responsible for handling the save file.
/// </summary>
public static class SaveFile
{
    public static void Save()
    {

    }

    public static void Load()
    {

    }
}
