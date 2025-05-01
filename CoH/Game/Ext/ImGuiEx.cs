using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoH.Game.Ext;

public static class ImGuiEx
{
    public static void ComboByte(ref byte value, string[] options)
    {
        int currentCost = value;
        if (ImGui.Combo("Cost", ref currentCost, options, options.Length))
            value = (byte)currentCost;
    }
}
