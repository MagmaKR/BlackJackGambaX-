using System;
using System.Collections;
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
    public static string vALUEHIT = string.Empty;
    private static string HitText = string.Empty;
    public static string StandValue = string.Empty;
    public static string DoubleDownValue = string.Empty;
    public static string rulesEmote = string.Empty;
    public static string BetEmote = string.Empty;
    public static string Natural21 = string.Empty;
    public static string Bust = string.Empty;
    private static string savedText1, savedText2, savedText3, savedText4, savedText5, savedText6, savedText7, savedText8 = "";
    public Plugin plugin;
    public string DealerName;
    private string selectedPlayer = "Select a player";

    public ConfigWindow(Plugin plugin) : base("Settings Menu###With a constant ID")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(500, 550);
        SizeCondition = ImGuiCond.Always;

        Configuration = plugin.Configuration;
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
        {
            ImGui.InputTextWithHint("Hit emote", "emote done after player hits ", ref vALUEHIT, 20);
        }

        ImGui.SetNextItemWidth(250f);
        {
            ImGui.InputText("Additional text for hit", ref HitText, 40);
        }

        ImGui.Dummy(new Vector2(0, 20));

        ImGui.SetNextItemWidth(250f);
        {
            ImGui.InputTextWithHint("Stand emote", "emote done after player stands", ref StandValue, 40);
        }
        ImGui.Dummy(new Vector2(0, 20));
        ImGui.SetNextItemWidth(250f);
        {
            ImGui.InputTextWithHint("Doubledown emote", "emote done after player double downs ", ref DoubleDownValue, 50);
        }
        ImGui.Dummy(new Vector2(0, 20));
        ImGui.SetNextItemWidth(250f);
        {
            ImGui.InputTextWithHint("Rules emote ", "emote done after player shows rules ", ref rulesEmote, 50);
        }

        ImGui.Dummy(new Vector2(0, 20));
        ImGui.SetNextItemWidth(250f);
        {
            ImGui.InputTextWithHint("Bet emote", "emote done after player Bets ", ref BetEmote, 50);
        }

        ImGui.Dummy(new Vector2(0, 20));
        ImGui.SetNextItemWidth(250f);
        {
            ImGui.InputTextWithHint("Natural 21 emote", "emote done after player hits a natural 21", ref Natural21, 50);
        }

        ImGui.Dummy(new Vector2(0, 20));
        ImGui.SetNextItemWidth(250f);
        {
            ImGui.InputTextWithHint("Bust emote", "emote done after player busts", ref Bust, 50);
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
            savedText1 = vALUEHIT;
            savedText2 = HitText;
            savedText3 = StandValue;
            savedText4 = DoubleDownValue;
            savedText5 = rulesEmote;
            savedText6 = BetEmote;
            savedText7 = Natural21;
            savedText7 = Bust;
        }

        ImGui.SetCursorPos(new Vector2(70, 520));
        if (ImGui.Button("Reset"))
        {
            vALUEHIT = string.Empty;
            HitText = string.Empty;
            StandValue = string.Empty;
            DoubleDownValue = string.Empty;
            rulesEmote = string.Empty;
            BetEmote = string.Empty;
            Natural21 = string.Empty;
        }

        ImGui.SetCursorPos(new Vector2(130, 520));
        if (ImGui.Button("Load"))
        {
            vALUEHIT = savedText1;
            HitText = savedText2;
            StandValue = savedText3;
            DoubleDownValue = savedText4;
            rulesEmote = savedText5;
            BetEmote = savedText6;
            Natural21 = savedText7;
            Bust = savedText8;
        }
        ImGui.PopStyleColor();
    }

    public void DealerMembers()
    {
        var playerNames = new ArrayList();

        if (Svc.Party.Length > 0)
        {
            ImGui.Text("Party members ");

            foreach (var members in Svc.Party)
            {
                playerNames.Add(members.Name.TextValue);
            }
        }
        else
        {
            ImGui.Text("No party Members found");
        }

        if (ImGui.BeginCombo("Dealer select", "select a dealer"))
        {
            foreach (string name in playerNames)
            {
                bool isSelected = (selectedPlayer == name);

                if (ImGui.Selectable(name, isSelected))
                {
                    selectedPlayer = name;
                    Plugin.dealerName = selectedPlayer; // Update dealer name
                }
                else
                {
                    ImGui.Text("no players have been found");
                }

                if (isSelected)
                {
                    ImGui.SetItemDefaultFocus();
                }
            }
            ImGui.EndCombo();
        }
    }
}
