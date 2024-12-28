using ECommons.Automation;
using FFXIVClientStructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SamplePlugin.Plugin;
using ImGuiNET;
using SamplePlugin;
using Dalamud.Plugin.Services;


public class PlayerNameUI
{

    public String showPlayerNames()
    {
        string PlayerName = clientState.LocalPlayer.Name.TextValue;

        if (clientState.LocalPlayer != null)
        {
            return PlayerName;
        }
        else
        {
            return "Player not found";
        }
    }

}
