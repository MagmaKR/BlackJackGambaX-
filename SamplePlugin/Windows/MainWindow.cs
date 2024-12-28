using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ECommons.Automation;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using ImGuiNET;
using static SamplePlugin.Plugin;

namespace SamplePlugin.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin Plugin;
    private Dictionary<string, int> playerBets = new Dictionary<string, int>();
    private Dictionary<string, List<int>> playerCardValues = new Dictionary<string, List<int>>();
    private BetManager betManager;
    private string dealerName;

    private const int MinBet = 20000;
    private const int MaxBet = 500000;

    public MainWindow(Plugin plugin)
        : base("BlackJack Gamba Manager##With a hidden ID", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(200, 150),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Plugin = plugin;
        dealerName = Plugin.dealerName; // Initialize dealer name
    }

    public void Dispose() { }

    public override void Draw()
    {
        // Header Section
        ImGui.TextColored(ImGuiColors.TankBlue, "Barr-Berry-Nyans Gamba Plugin");
        float spacing = ImGui.GetScrollMaxY() == 0 ? 100f : 100f;
        ImGui.SameLine(ImGui.GetWindowWidth() - spacing);

        if (ImGui.Button("Show Settings"))
        {
            Plugin.ToggleConfigUI();
        }

        // Main Content
        ImGui.BeginChild("MainContent", new Vector2(0, -100), true);

        // Leaderboard Section
        ImGui.BeginChild("LeaderboardSection", new Vector2(200, 400), true);
        ImGui.Text("Leaderboard:");


        ImGui.EndChild();
        ImGui.SameLine();

        // Player Bets Section
        ImGui.BeginChild("PlayersSection", new Vector2(700, 0), true);
        ImGui.TextColored(ImGuiColors.TankBlue, "Party Members");

        foreach (var member in Svc.Party)
        {
            if (member.Name.TextValue != dealerName)
            {
                ImGui.PushID(member.Name.TextValue);
                // Player Frame
                ImGui.BeginChild($"Player_{member.Name.TextValue}", new Vector2(700, 100), true);
                ImGui.Text(member.Name.TextValue);

                if (!playerBets.ContainsKey(member.Name.TextValue))
                {
                    playerBets[member.Name.TextValue] = 0;
                }

                if (!playerCardValues.ContainsKey(member.Name.TextValue))
                {
                    playerCardValues[member.Name.TextValue] = new List<int>();
                }

                int bet = playerBets[member.Name.TextValue];
                ImGui.SameLine();
                ImGui.PushItemWidth(200);

                string formatBet = Plugin.BetManager.FormatBet(bet);
                if (ImGui.InputInt($"Bet: {formatBet}", ref bet, 500000))
                {
                    playerBets[member.Name.TextValue] = bet;
                }

                ImGui.PopItemWidth();
                ImGui.SameLine();

                if (ImGui.Button("Bet", new Vector2(60, 30)))
                {
                    Plugin.Chat.SendMessage($"/p {member.Name.TextValue} bet amount is {formatBet}");
                }

                // Display Player's Cards
                var cardValues = playerCardValues[member.Name.TextValue];
                ImGui.Text($"{member.Name.TextValue} Cards: {string.Join(", ", cardValues)} Total: {GetTotal(cardValues)}");

                if (ImGui.Button("Hit", new Vector2(80, 30)))
                {
                    int diceResult = GetDiceResult();
                    int diceResult2 = GetDiceResult();

                    Console.WriteLine($"Hit result for {member.Name.TextValue}: {diceResult}");
                    playerCardValues[member.Name.TextValue].Add(diceResult);
                    playerCardValues[member.Name.TextValue].Add(diceResult2);

                    Plugin.Chat.SendMessage($"/p Random! (1-13) {diceResult}");
                    Plugin.Chat.SendMessage($"/p Random! (1-13) {diceResult2}");

                    Plugin.Chat.SendMessage($"/p {member.Name.TextValue}'s Total: {GetTotal(cardValues)} ");

                    if (GetTotal(playerCardValues[member.Name.TextValue]) >= 21)
                    {
                        Plugin.Chat.SendMessage($"{member.Name.TextValue} has gone over 21! Total: {GetTotal(playerCardValues[member.Name.TextValue])}");
                        playerCardValues[member.Name.TextValue].Clear();
                    }
                }

                ImGui.SameLine();
                if (ImGui.Button("Stand", new Vector2(60, 30)))
                {
                    Plugin.Chat.SendMessage($"/p Player {member.Name.TextValue} is standing for this round");
                }

                ImGui.EndChild();
                ImGui.PopID();
            }
        }

        ImGui.EndChild();
        ImGui.EndChild();

        // Dealer Section
        ImGui.BeginChild("DealerSection", new Vector2(0, 90), true);
        ImGui.PushStyleColor(ImGuiCol.Button, ImGuiColors.TankBlue);

        ImGui.Text("Dealer Section:");
        ImGui.SameLine();
        ImGui.Text($"Dealer: {dealerName}");

        int diceResultDealer = GetDiceResult();
        if (ImGui.Button("Dealer card amount", new Vector2(250, 30)))
        {
            Plugin.Chat.SendMessage($"/p Random! (1-13) {diceResultDealer}");
        }
        ImGui.SameLine();
        if (ImGui.Button("Reveal second card", new Vector2(150, 30)))
        {
            Plugin.Chat.SendMessage($"/p Random! (1-13) {diceResultDealer}");
        }
        ImGui.SameLine();
        ImGui.Button("Give winnings", new Vector2(150, 30));
        ImGui.SameLine();
        ImGui.Button("Show rules", new Vector2(150, 30));
        ImGui.SameLine();
        if (ImGui.Button("Clear", new Vector2(150, 30)))
        {
            ClearPlayerData();
        }

        ImGui.EndChild();
        ImGui.EndChild();
    }

    private int GetDiceResult()
    {
        return new Random().Next(1, 13);
    }

    private int GetTotal(List<int> cardValues)
    {
        int total = 0;
        foreach (var value in cardValues)
        {
            total += value;
        }
        return total;
    }

    private void ClearPlayerData()
    {
        foreach (var member in Svc.Party)
        {
            if (member.Name.TextValue != dealerName)
            {
                playerCardValues[member.Name.TextValue].Clear();
            }
        }
    }
}
