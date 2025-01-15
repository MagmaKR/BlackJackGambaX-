using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using ImGuiNET;
using static SamplePlugin.Plugin;
using SamplePlugin.Game;

namespace SamplePlugin.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin Plugin;
    public GameState gameState;
    private WinnerDisplay winnerDisplay;
    private CardManagement cardManager;

    public MainWindow(Plugin plugin)
        : base("BlackJack Gamba Manager##With a hidden ID", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        Plugin = plugin;
        gameState = new GameState();
        winnerDisplay = new WinnerDisplay();
        cardManager = new CardManagement();

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(200, 150),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public void Dispose() { }

    public override void Draw()
    {
        // Header Section
        ImGui.TextColored(ImGuiColors.TankBlue, "Barr-Berry-Nyans Gamba Plugin");
        if (ImGui.Button("Show Settings"))
        {
            Plugin.ToggleConfigUI();
        }

        // Main Content
        ImGui.BeginChild("MainContent", new Vector2(0, -100), true);

        // Leaderboard Section
        ImGui.BeginChild("LeaderboardSection", new Vector2(200, 400), true);
        ImGui.Text("Leaderboard:");
        foreach (var player in gameState.Players)
        {
            if (player.Key != Plugin.Configuration.DealerName)
            {
                string winningsFormatted = FormatWinnings(player.Value.Winnings);
                ImGui.Text($"{player.Key}: {winningsFormatted}");
            }
        }
        ImGui.EndChild();
        ImGui.SameLine();

        // Player Bets Section
        PlayersInPartyInfo playersInPartyInfo = new PlayersInPartyInfo();
        CardManagement card = new CardManagement();

        ImGui.BeginChild("PlayersSection", new Vector2(700, 0), true);
        ImGui.TextColored(ImGuiColors.TankBlue, "Party Members");

        foreach (var member in Svc.Party)
        {
            if (member.Name.TextValue != Plugin.Configuration.DealerName)
            {
                string playerName = member.Name.TextValue;
                ImGui.PushID(playerName);
                ImGui.BeginChild($"Player_{playerName}", new Vector2(700, 100), true);
                ImGui.Text(playerName);

                // Initialize player state if it doesn't exist
                if (!gameState.Players.ContainsKey(playerName))
                {
                    gameState.Players[playerName] = new PlayerState();
                }

                var playerState = gameState.Players[playerName];
                int bet = playerState.Bet;
                ImGui.SameLine();
                ImGui.PushItemWidth(200);

                string formatBet = FormatWinnings(bet);
                if (ImGui.InputInt($"Bet: {formatBet}", ref bet, 500000))
                    playerState.Bet = bet;

                ImGui.PopItemWidth();
                ImGui.SameLine();

                if (ImGui.Button("Bet", new Vector2(60, 30)))
                {
                    Plugin.Chat.SendMessage($"/p {playerName} bet amount is {formatBet}");
                    if (!string.IsNullOrEmpty(Plugin.Configuration.BetEmote))
                        Plugin.Chat.SendMessage($"/p {Plugin.Configuration.BetEmote}");
                }

                // Display Player's Cards
                var cardValues = playerState.CardValues;
                ImGui.Text($"{playerName} Cards: {string.Join(", ", cardValues)} Total: {card.GetTotal(cardValues)}");

                // Set button color based on standing state
                if (playerState.IsStanding)
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.5f, 0.5f, 0.5f, 1.0f)); // Gray
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
                }

                // First Hit Button
                if (playerState.IsStanding || playerState.HasUsedFirstHit)
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.5f, 0.5f, 0.5f, 1.0f)); // Gray
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
                }

                if (ImGui.Button("Hit", new Vector2(80, 30)) && !playerState.IsStanding && !playerState.HasDoubledDown && !playerState.HasUsedFirstHit)
                {
                    List<int> currentHand = playerState.IsPlayingSecondHand ? 
                        playerState.SecondHand : playerState.CardValues;

                    // Initial two-card draw
                    int diceResult = card.GetDiceResult();
                    int diceResult2 = card.GetDiceResult();

                    // Display hit emotes
                    if (!string.IsNullOrEmpty(Plugin.Configuration.ValueHit))
                        Plugin.Chat.SendMessage($"/p {Plugin.Configuration.ValueHit}");
                    if (!string.IsNullOrEmpty(Plugin.Configuration.HitText))
                        Plugin.Chat.SendMessage($"/p {Plugin.Configuration.HitText}");

                    currentHand.Add(diceResult);
                    currentHand.Add(diceResult2);

                    // Check for split possibility (only on initial draw)
                    if (diceResult == diceResult2 && !playerState.IsPlayingFirstHand && !playerState.IsPlayingSecondHand)
                    {
                        playerState.CanSplit = true;
                    }

                    // Enable double down on first two cards only
                    playerState.CanDoubleDown = true;
                    playerState.HitButton2Enabled = true;
                    playerState.HasUsedFirstHit = true; // Mark first hit as used

                    Plugin.Chat.SendMessage($"/p Random! (1-13) {diceResult}");
                    Plugin.Chat.SendMessage($"/p Random! (1-13) {diceResult2}");
                    Plugin.Chat.SendMessage($"/p {playerName}'s Total: {card.GetTotal(currentHand)}");

                    // Check for Natural 21 or bust
                    if (card.GetTotal(currentHand) > 21)
                    {
                        Plugin.Chat.SendMessage($"/p {playerName}'s hand busted with {card.GetTotal(currentHand)}");
                        if (!string.IsNullOrEmpty(Plugin.Configuration.BustEmote))
                            Plugin.Chat.SendMessage($"/p {Plugin.Configuration.BustEmote}");
                        playerState.IsStanding = true;
                    }
                    else if (card.GetTotal(currentHand) == 21)
                    {
                        Plugin.Chat.SendMessage($"/p {playerName} got a Natural 21!");
                        if (!string.IsNullOrEmpty(Plugin.Configuration.Natural21Emote))
                            Plugin.Chat.SendMessage($"/p {Plugin.Configuration.Natural21Emote}");
                        playerState.IsStanding = true;
                    }
                }

                if (playerState.IsStanding || playerState.HasUsedFirstHit)
                {
                    ImGui.PopStyleColor(3);
                }

                ImGui.SameLine();

                // Hit Button 2
                if (playerState.IsStanding)
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
                }
                
                if (ImGui.Button("Hit 2", new Vector2(80, 30)) && playerState.HitButton2Enabled && !playerState.IsStanding && !playerState.HasDoubledDown)
                {
                    int diceResult = card.GetDiceResult();
                    playerState.CardValues.Add(diceResult);
                    
                    Plugin.Chat.SendMessage($"/p Random! (1-13) {diceResult}");
                    Plugin.Chat.SendMessage($"/p {playerName}'s Total: {card.GetTotal(playerState.CardValues)}");

                    if (card.GetTotal(playerState.CardValues) > 21)
                    {
                        Plugin.Chat.SendMessage($"{playerName} has busted: Total is {card.GetTotal(playerState.CardValues)}");
                        if (!string.IsNullOrEmpty(Plugin.Configuration.BustEmote))
                            Plugin.Chat.SendMessage($"/p {Plugin.Configuration.BustEmote}");
                        playerState.IsStanding = true;
                    }
                    
                    playerState.HitButton2Enabled = true; // Disable after use
                }

                ImGui.SameLine();

                // Stand Button
                if (ImGui.Button("Stand", new Vector2(60, 30)) && !playerState.IsStanding)
                {
                    Plugin.Chat.SendMessage($"/p Player {playerName} is standing for this round");
                    if (!string.IsNullOrEmpty(Plugin.Configuration.StandValue))
                        Plugin.Chat.SendMessage($"/p {Plugin.Configuration.StandValue}");
                    playerState.IsStanding = true;
                }

                // Double Down Button
                if (playerState.IsStanding || !playerState.CanDoubleDown)
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
                }
                 ImGui.SameLine();
                if (ImGui.Button("DD", new Vector2(60, 30)) && playerState.CanDoubleDown && !playerState.IsStanding)
                {
                    // Double the bet
                    playerState.Bet *= 2;
                    Plugin.Chat.SendMessage($"/p {playerName} doubled down! New bet: {FormatWinnings(playerState.Bet)}");
                    if (!string.IsNullOrEmpty(Plugin.Configuration.DoubleDownValue))
                        Plugin.Chat.SendMessage($"/p {Plugin.Configuration.DoubleDownValue}");

                    // Draw exactly one card
                    int diceResult = card.GetDiceResult();
                    playerState.CardValues.Add(diceResult);
                    Plugin.Chat.SendMessage($"/p Random! (1-13) {diceResult}");
                    Plugin.Chat.SendMessage($"/p {playerName}'s Total: {card.GetTotal(playerState.CardValues)}");

                    // After double down, player automatically stands
                    playerState.IsStanding = true;
                    playerState.HasDoubledDown = true;
                    playerState.CanDoubleDown = false;
                    playerState.HitButton2Enabled = false; // Disable Hit 2 as well

                    if (card.GetTotal(playerState.CardValues) > 21)
                    {
                        Plugin.Chat.SendMessage($"/p {playerName} has busted: Total is {card.GetTotal(playerState.CardValues)}");
                        if (!string.IsNullOrEmpty(Plugin.Configuration.BustEmote))
                            Plugin.Chat.SendMessage($"/p {Plugin.Configuration.BustEmote}");
                    }
                }

                if (playerState.IsStanding || !playerState.CanDoubleDown)
                {
                    ImGui.PopStyleColor(3);
                }

                ImGui.SameLine();

                // Split Button
                if (playerState.IsStanding || !playerState.CanSplit)
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
                }
                 ImGui.SameLine();
                if (ImGui.Button("Split", new Vector2(60, 30)) && playerState.CanSplit && !playerState.IsStanding)
                {
                    // Create two hands from the split
                    var firstCard = playerState.CardValues[0];
                    var secondCard = playerState.CardValues[1];

                    // Check if splitting aces
                    playerState.HasSplitAces = (firstCard == 1 && secondCard == 1);

                    // Clear original hand and setup both hands
                    playerState.CardValues.Clear();
                    playerState.CardValues.Add(firstCard);
                    
                    playerState.SecondHand.Clear();
                    playerState.SecondHand.Add(secondCard);

                    // Set playing state
                    playerState.IsPlayingFirstHand = true;
                    playerState.IsPlayingSecondHand = false;

                    // Double the bet (one bet per hand)
                    playerState.Bet *= 2;

                    Plugin.Chat.SendMessage($"/p {playerName} split their pair! New bet per hand: {FormatWinnings(playerState.Bet / 2)}");
                    Plugin.Chat.SendMessage($"/p Playing first hand...");
                    
                    playerState.CanSplit = false;
                }

                if (playerState.IsStanding || !playerState.CanSplit)
                {
                    ImGui.PopStyleColor(3);
                }

                // Pop the style colors if buttons were disabled
                if (playerState.IsStanding)
                {
                    ImGui.PopStyleColor(3);
                    ImGui.PopStyleColor(3);
                }

                ImGui.EndChild();
                ImGui.PopID();
            }
        }

        ImGui.EndChild();
        ImGui.EndChild();

        // Dealer Section
        ImGui.BeginChild("DealerSection", new Vector2(0, 150), true);
        ImGui.PushStyleColor(ImGuiCol.Button, ImGuiColors.TankBlue);

        ImGui.Text("Dealer Section:");
        ImGui.SameLine();
        
        string dealerName = string.IsNullOrEmpty(Plugin.Configuration.DealerName) 
            ? "No dealer selected" 
            : Plugin.Configuration.DealerName;
        
        ImGui.Text($"Dealer: {dealerName}");

        // Only show dealer controls if a dealer is selected
        if (!string.IsNullOrEmpty(Plugin.Configuration.DealerName))
        {
            ImGui.Text($"Dealer's Cards: {string.Join(", ", gameState.DealerCards)} Total: {card.GetTotal(gameState.DealerCards)}");

            if (ImGui.Button("Hit", new Vector2(150, 30)))
            {
                int dealerCard = card.GetDiceResult();
                gameState.DealerCards.Add(dealerCard);

                Plugin.Chat.SendMessage($"/p Dealer drew a card: {dealerCard}");
                Plugin.Chat.SendMessage($"/p Dealer's Total: {card.GetTotal(gameState.DealerCards)}");
            }
        }
        else
        {
            ImGui.TextColored(ImGuiColors.DalamudRed, "Please select a dealer in the settings menu");
        }

        ImGui.SameLine();
        if (ImGui.Button("Clear", new Vector2(150, 30)))
        {
            foreach (var player in gameState.Players.Values)
            {
                // Reset everything except Winnings
                player.Bet = 0;
                player.CardValues.Clear();
                player.HitButton2Enabled = false;
                player.IsStanding = false;
                player.CanDoubleDown = false;
                player.CanSplit = false;
                player.HasDoubledDown = false;
                player.IsPlayingFirstHand = false;
                player.IsPlayingSecondHand = false;
                player.SecondHand.Clear();
                player.HasSplitAces = false;
                player.HasUsedFirstHit = false;
            }
            gameState.DealerCards.Clear();
            Plugin.Chat.SendMessage("/p Cleared all player and dealer data for the next round.");
        }

       var ruleList = new Rules();

        ImGui.SameLine();
        if (ImGui.Button("Rules", new Vector2(150, 30)))
        {
            
            if (!string.IsNullOrEmpty(Plugin.Configuration.RulesEmote))
                Plugin.Chat.SendMessage($"/p {Plugin.Configuration.RulesEmote}");
        }

        if (gameState.ShowRules)
        {
           

        }

        ImGui.SameLine();
        if (ImGui.Button("Rules 2", new Vector2(150, 30)))
        {
            gameState.ShowRules2 = !gameState.ShowRules2;
        }

        if (gameState.ShowRules2)
        {
            
        }

        ImGui.SameLine();
        if (ImGui.Button("Give winnings", new Vector2(150, 30)))
        {
            double highestWinnings = 0;
            string roundWinner = string.Empty;

            foreach (var member in Svc.Party)
            {
                if (member.Name.TextValue != Plugin.Configuration.DealerName)
                {
                    string playerName = member.Name.TextValue;
                    
                    // Check if player has cards and bets before calculating
                    if (gameState.Players.ContainsKey(playerName) && gameState.Players[playerName].Bet > 0)
                    {
                        int playerTotal = card.GetTotal(gameState.Players[playerName].CardValues);
                        int dealerTotal = card.GetTotal(gameState.DealerCards);
                        int playerBet = gameState.Players[playerName].Bet;

                        double winnings = 0;
                        double netChange = 0; // Track the actual change to player's balance

                        // Calculate winnings based on game outcome
                        if (playerTotal == 21 && gameState.Players[playerName].CardValues.Count == 2)
                        {
                            winnings = playerBet * 2.5;
                            netChange = winnings - playerBet; // Player gets original bet plus 1.5x
                            Plugin.Chat.SendMessage($"/p {playerName} won {FormatWinnings(netChange)} on a natural blackJack!");
                            if (!string.IsNullOrEmpty(Plugin.Configuration.Natural21Emote))
                                Plugin.Chat.SendMessage($"/p {Plugin.Configuration.Natural21Emote}");
                        }
                        else if (playerTotal <= 21 && (playerTotal > dealerTotal || dealerTotal > 21))
                        {
                            winnings = playerBet * 2;
                            netChange = winnings - playerBet; // Player gets original bet plus 1x
                            Plugin.Chat.SendMessage($"/p {playerName} won {FormatWinnings(netChange)} by beating the dealer!");
                        }
                        else if (playerTotal <= 21 && playerTotal == dealerTotal)
                        {
                            winnings = playerBet;
                            netChange = 0; // Player gets back original bet, no change to balance
                            Plugin.Chat.SendMessage($"/p {playerName} tied with the dealer. Bet returned: {FormatWinnings(winnings)}");
                        }
                        else
                        {
                            winnings = 0;
                            netChange = -playerBet; // Player loses their bet
                            Plugin.Chat.SendMessage($"/p {playerName} lost their bet of {FormatWinnings(netChange)}");
                        }

                        // Track highest winner based on net change
                        if (netChange > highestWinnings)
                        {
                            highestWinnings = netChange;
                            roundWinner = playerName;
                        }

                        // Update player winnings with net change
                        gameState.Players[playerName].Winnings += netChange;
                        gameState.Players[playerName].CardValues.Clear();
                        gameState.Players[playerName].Bet = 0;
                    }
                }
            }

            // Show winner window if there was a winner
            if (!string.IsNullOrEmpty(roundWinner))
            {
                winnerDisplay.ShowWinner(roundWinner);
                Plugin.Chat.SendMessage($"/p 🎉 Congratulations to {roundWinner} for winning this round! 🎉");
            }
        }

        ImGui.PopStyleColor();
        ImGui.EndChild();
        ImGui.EndChild();

        winnerDisplay.Draw();
    }

    private string FormatWinnings(double winnings)
    {
        string prefix = winnings < 0 ? "-" : "";
        double absWinnings = Math.Abs(winnings);
        
        if (absWinnings >= 1_000_000)
            return $"{prefix}{absWinnings / 1_000_000:F1}M";
        else if (absWinnings >= 1_000)
            return $"{prefix}{absWinnings / 1_000:F1}k";
        else
            return $"{prefix}{absWinnings:F0}";
    }

    
}
