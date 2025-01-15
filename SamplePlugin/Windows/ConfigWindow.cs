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

    private string valueHit = string.Empty;
    private string hitText = string.Empty;
    private string standValue = string.Empty;
    private string doubleDownValue = string.Empty;
    private string rulesEmote = string.Empty;
    private string betEmote = string.Empty;
    private string natural21 = string.Empty;
    private string bust = string.Empty;
    private string selectedDealer = "Select a dealer"; // Default text

    public ConfigWindow(Plugin plugin) : base("Settings Menu###With a constant ID")
    {
        this.Plugin = plugin;
        this.Configuration = plugin.Configuration;

        valueHit = Configuration.ValueHit;
        hitText = Configuration.HitText;
        standValue = Configuration.StandValue;
        doubleDownValue = Configuration.DoubleDownValue;
        rulesEmote = Configuration.RulesEmote;
        betEmote = Configuration.BetEmote;
        natural21 = Configuration.Natural21Emote;
        bust = Configuration.BustEmote;

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
        if (ImGui.InputTextWithHint("Hit emote", "emote done after player hits ", ref valueHit, 20))
        {
            Configuration.ValueHit = valueHit;
            Configuration.Save();
        }

        ImGui.SetNextItemWidth(250f);
        if (ImGui.InputText("Additional text for hit", ref hitText, 40))
        {
            Configuration.HitText = hitText;
            Configuration.Save();
        }

        ImGui.Dummy(new Vector2(0, 20));

        ImGui.SetNextItemWidth(250f);
        if (ImGui.InputTextWithHint("Stand emote", "emote done after player stands", ref standValue, 40))
        {
            Configuration.StandValue = standValue;
            Configuration.Save();
        }
        ImGui.Dummy(new Vector2(0, 20));
        ImGui.SetNextItemWidth(250f);
        if (ImGui.InputTextWithHint("Doubledown emote", "emote done after player double downs ", ref doubleDownValue, 50))
        {
            Configuration.DoubleDownValue = doubleDownValue;
            Configuration.Save();
        }
        ImGui.Dummy(new Vector2(0, 20));
        ImGui.SetNextItemWidth(250f);
        if (ImGui.InputTextWithHint("Rules emote ", "emote done after player shows rules ", ref rulesEmote, 50))
        {
            Configuration.RulesEmote = rulesEmote;
            Configuration.Save();
        }

        ImGui.Dummy(new Vector2(0, 20));
        ImGui.SetNextItemWidth(250f);
        if (ImGui.InputTextWithHint("Bet emote", "emote done after player Bets ", ref betEmote, 50))
        {
            Configuration.BetEmote = betEmote;
            Configuration.Save();
        }

        ImGui.Dummy(new Vector2(0, 20));
        ImGui.SetNextItemWidth(250f);
        if (ImGui.InputTextWithHint("Natural 21 emote", "emote done after player hits a natural 21", ref natural21, 50))
        {
            Configuration.Natural21Emote = natural21;
            Configuration.Save();
        }

        ImGui.Dummy(new Vector2(0, 20));
        ImGui.SetNextItemWidth(250f);
        if (ImGui.InputTextWithHint("Bust emote", "emote done after player busts", ref bust, 50))
        {
            Configuration.BustEmote = bust;
            Configuration.Save();
        }

        DealerMembers();
        Buttons();
    }

    public void Buttons()
    {
        ImGui.PushStyleColor(ImGuiCol.Button, ImGuiColors.TankBlue);
        ImGui.SetCursorPos(new Vector2(10, 520));
        if (ImGui.Button("Save"))
        {
            Configuration.ValueHit = valueHit;
            Configuration.HitText = hitText;
            Configuration.StandValue = standValue;
            Configuration.DoubleDownValue = doubleDownValue;
            Configuration.RulesEmote = rulesEmote;
            Configuration.BetEmote = betEmote;
            Configuration.Natural21Emote = natural21;
            Configuration.BustEmote = bust;
            
            Configuration.SaveCurrentValues();
            Configuration.Save();
        }

        ImGui.SetCursorPos(new Vector2(70, 520));
        if (ImGui.Button("Reset"))
        {
            valueHit = string.Empty;
            hitText = string.Empty;
            standValue = string.Empty;
            doubleDownValue = string.Empty;
            rulesEmote = string.Empty;
            betEmote = string.Empty;
            natural21 = string.Empty;
            bust = string.Empty;

            Configuration.ValueHit = string.Empty;
            Configuration.HitText = string.Empty;
            Configuration.StandValue = string.Empty;
            Configuration.DoubleDownValue = string.Empty;
            Configuration.RulesEmote = string.Empty;
            Configuration.BetEmote = string.Empty;
            Configuration.Natural21Emote = string.Empty;
            Configuration.BustEmote = string.Empty;
            Configuration.Save();
        }

        ImGui.SetCursorPos(new Vector2(130, 520));
        if (ImGui.Button("Load"))
        {
            Configuration.LoadSavedValues();
            
            valueHit = Configuration.ValueHit;
            hitText = Configuration.HitText;
            standValue = Configuration.StandValue;
            doubleDownValue = Configuration.DoubleDownValue;
            rulesEmote = Configuration.RulesEmote;
            betEmote = Configuration.BetEmote;
            natural21 = Configuration.Natural21Emote;
            bust = Configuration.BustEmote;
        }

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

    public void DisplayHITMessage()
    {
        Plugin.Chat.SendMessage($"{valueHit}");
        Plugin.Chat.SendMessage($"{hitText}");
    }

    public void DisplayStandEmote()
    {
        Plugin.Chat.SendMessage($"{standValue}");
    }

    public void DisplayDoubleDownEmote()
    {
        Plugin.Chat.SendMessage($"{doubleDownValue}");
    }

    public void DisplayRulesEmote()
    {
        Plugin.Chat.SendMessage($"{rulesEmote}");
    }

    public void DisplayBetEmote()
    {
        Plugin.Chat.SendMessage($"{betEmote}");
    }

    public void DisplayNat21Emote()
    {
        Plugin.Chat.SendMessage($"{natural21}");
    }

    public void DisplayBustEmote()
    {
        Plugin.Chat.SendMessage($"{bust}");
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
