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
using SamplePlugin;
using static SamplePlugin.Plugin;
public partial class PlayersInPartyInfo
{
    private Plugin Plugin;
    private Dictionary<string, int> playerBets = new Dictionary<string, int>();
    private Dictionary<string, List<int>> playerCardValues = new Dictionary<string, List<int>>();
   
    private string? dealerName;

    public void DisplayPartyMemebersChild()
    {
        CardManagement card = new CardManagement();

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
                ImGui.Text($"{member.Name.TextValue} Cards: {string.Join(", ", cardValues)} Total: {card.GetTotal(cardValues)}");

                if (ImGui.Button("Hit", new Vector2(80, 30)))
                {
                    int diceResult = card.GetDiceResult();
                    int diceResult2 = card.GetDiceResult();

                    Console.WriteLine($"Hit result for {member.Name.TextValue}: {diceResult}");
                    playerCardValues[member.Name.TextValue].Add(diceResult);
                    playerCardValues[member.Name.TextValue].Add(diceResult2);

                    Plugin.Chat.SendMessage($"/p Random! (1-13) {diceResult}");
                    Plugin.Chat.SendMessage($"/p Random! (1-13) {diceResult2}");

                    Plugin.Chat.SendMessage($"/p {member.Name.TextValue}'s Total: {card.GetTotal(cardValues)} ");

                    if (card.GetTotal(playerCardValues[member.Name.TextValue]) >= 21)
                    {
                        Plugin.Chat.SendMessage($"/p{member.Name.TextValue} has gone over 21! Total: {card.GetTotal(playerCardValues[member.Name.TextValue])}");
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
    }
}
