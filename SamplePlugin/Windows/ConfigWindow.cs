using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Windows;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using ImGuiNET;

namespace SamplePlugin.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;
    private Plugin Plugin;

   
    private string selectedDealer = "Select a dealer"; // Default text

    public ConfigWindow(Plugin plugin) : base("Settings Menu###With a constant ID")
    {
        this.Plugin = plugin;
        this.Configuration = plugin.Configuration;

       

        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(500, 550);
        SizeCondition = ImGuiCond.Always;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        if (Configuration.IsConfigWindowMovable)
        {
            Flags &= ~ImGuiWindowFlags.NoMove;
        }
        else
        {
            Flags |= ImGuiWindowFlags.NoMove;
        }
    }

    public override void Draw()
    {
        ImGui.TextColored(ImGuiColors.TankBlue, "Settings Menu");
        ImGui.Dummy(new Vector2(0, 40));
        ImGui.SetNextItemWidth(250f);
       


        ImGui.Dummy(new Vector2(0, 20));

        

        DealerMembers();
        Buttons();
    }

    public void Buttons()
    {
        

        ImGui.SetCursorPos(new Vector2(190, 520));
        if (ImGui.Button("Clear Leaderboard"))
        {
            foreach (var player in Plugin.MainWindow.gameState.Players.Values)
            {
                player.Winnings = 0;
            }
            Plugin.Chat.SendMessage("/p Leaderboard has been cleared!");
        }

        ImGui.PopStyleColor();
    }

   

    public void DealerMembers()
    {
        ImGui.Separator();
        ImGui.TextColored(ImGuiColors.TankBlue, "Dealer Selection");
        
        var playerNames = new List<string>();
        
        if (Svc.Party.Length > 0)
        {
            foreach (var member in Svc.Party)
            {
                playerNames.Add(member.Name.TextValue);
            }
            
            if (ImGui.BeginCombo("Select Dealer", selectedDealer))
            {
                foreach (string name in playerNames)
                {
                    bool isSelected = (selectedDealer == name);
                    if (ImGui.Selectable(name, isSelected))
                    {
                        selectedDealer = name;
                        Configuration.DealerName = name; // Save to configuration
                        Configuration.Save();
                    }

                    if (isSelected)
                    {
                        ImGui.SetItemDefaultFocus();
                    }
                }
                ImGui.EndCombo();
            }
        }
        else
        {
            ImGui.TextColored(ImGuiColors.DalamudRed, "No party members found");
        }
    }
}
