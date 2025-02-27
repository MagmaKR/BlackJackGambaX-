using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using ImGuiNET;
using static SamplePlugin.Plugin;
using SamplePlugin.Game;
using FFXIVClientStructs.FFXIV.Client.System.Threading;
using System.Threading;
using Lumina.Excel.Sheets;

namespace SamplePlugin.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin Plugin;
    public GameState gameState;
    private WinnerDisplay winnerDisplay;
    private CardManagement cardManager;
    public string playerName { get; set; }

    public MainWindow(Plugin plugin)
        : base("BlackJack Gamba Manager##With a hidden ID", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.Plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
        this.gameState = new GameState();
        this.winnerDisplay = new WinnerDisplay(plugin);
        this.cardManager = new CardManagement();

        // Initialize default window constraints
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(200, 150),
            MaximumSize = new Vector2(1200, 800) // Set a reasonable maximum size
        };
    }

    public void Dispose() { }

    public override void Draw()
    {
        try
        {
            if (Svc.Party == null) return;

            // Header Section
            DrawHeader();

            // Main Content
            if (ImGui.BeginChild("MainContent", new Vector2(0, -100), true))
            {
                DrawMainContent();
                ImGui.EndChild();
            }

            // Dealer Section
            if (ImGui.BeginChild("DealerSection", new Vector2(0, 150), true))
            {
                DrawDealerSection();
                ImGui.EndChild();
            }

            // Draw winner display if needed
            winnerDisplay?.Draw();
        }
        catch (Exception ex)
        {
            Svc.Log.Error($"Error in Draw: {ex.Message}");
        }
    }

    private void DrawMainContent()
    {
        try
        {
            // Leaderboard Section
            if (ImGui.BeginChild("LeaderboardSection", new Vector2(200, 400), true))
            {
                DrawLeaderboard();
                ImGui.EndChild();
            }

            ImGui.SameLine();

            // Players Section
            if (ImGui.BeginChild("PlayersSection", new Vector2(700, 0), true))
            {
                DrawPlayers();
                ImGui.EndChild();
            }
        }
        catch (Exception ex)
        {
            Svc.Log.Error($"Error in DrawMainContent: {ex.Message}");
        }
    }

    private void DrawPlayers()
    {
        ImGui.TextColored(ImGuiColors.TankBlue, "Party Members");

        foreach (var member in Svc.Party)
        {
            if (member?.Name?.TextValue == null || member.Name.TextValue == Plugin.Configuration.DealerName)
                continue;

            playerName = member.Name.TextValue;

            try
            {
                // Initialize player state if needed
                if (!gameState.Players.ContainsKey(playerName))
                {
                    gameState.Players[playerName] = new PlayerState();
                    gameState.Players[playerName].Reset();
                }

                var playerState = gameState.Players[playerName];

                ImGui.PushID(playerName);
                if (ImGui.BeginChild($"Player_{playerName}", new Vector2(700, 100), true))
                {
                    DrawPlayerControls(playerName, playerState);
                    ImGui.EndChild();
                }
                ImGui.PopID();
            }
            catch (Exception ex)
            {
                Svc.Log.Error($"Error processing player {playerName}: {ex.Message}");
            }
        }
    }

    private string FormatWinnings(double winnings)
    {
        string prefix = winnings < 0 ? "-" : "";
        double absWinnings = Math.Abs(winnings);

        //============================================///
        if (absWinnings >= 1_000_000)
            return $"{prefix}{absWinnings / 1_000_000:F1}M";
        else if (absWinnings >= 1_000)
            return $"{prefix}{absWinnings / 1_000:F1}k";
        else
            return $"{prefix}{absWinnings:F0}";
    }

    private void DrawHeader()
    {
        try
        {
            ImGui.TextColored(ImGuiColors.TankBlue, "Barr-Berry-Nyans Gamba Plugin");
            if (ImGui.Button("Show Settings"))
            {
                Plugin.ToggleConfigUI();
            }
        }
        catch (Exception ex)
        {
            Svc.Log.Error($"Error in DrawHeader: {ex.Message}");
        }
    }

    private void DrawLeaderboard()
    {
        try
        {
            ImGui.Text("Leaderboard:");
            foreach (var player in gameState.Players)
            {
                if (player.Key != Plugin.Configuration.DealerName)
                {
                    string winningsFormatted = FormatWinnings(player.Value.Winnings);
                    ImGui.Text($"{player.Key}: {winningsFormatted}");
                }
            }
        }
        catch (Exception ex)
        {
            Svc.Log.Error($"Error in DrawLeaderboard: {ex.Message}");
        }
    }

    private void DrawPlayerControls(string playerName, PlayerState playerState)
    {
        try
        {
            ImGui.Text(playerName);
            var card = new CardManagement();

            // Bet Input and Button
            int bet = playerState.Bet;
            ImGui.SameLine();
            ImGui.PushItemWidth(200);

            string formatBet = FormatWinnings(bet);
            if (ImGui.InputInt($"Bet: {formatBet}", ref bet, 500000))
            {
                playerState.Bet = bet;
            }

            ImGui.PopItemWidth();
            ImGui.SameLine();

            DrawBetButton(playerName, formatBet);

            // Display Player's Cards with new format
            var cardValues = playerState.CardValues;
            string handDescription = card.GetHandDescription(cardValues);
            ImGui.Text($"{playerName} Cards: {handDescription} Total: {card.GetTotal(cardValues)}");

            // Game Buttons
            DrawGameButtons(playerName, playerState, card);
        }
        catch (Exception ex)
        {
            Svc.Log.Error($"Error in DrawPlayerControls for {playerName}: {ex.Message}");
        }
    }

    private void DrawBetButton(string playerName, string formatBet)
    {
        Vector4 defaultButtonColor = new Vector4(0.3f, 0.5f, 0.9f, 1.0f);

        ImGui.PushStyleColor(ImGuiCol.Button, defaultButtonColor);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.4f, 0.6f, 1.0f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.2f, 0.4f, 0.8f, 1.0f));

        if (ImGui.Button("Bet", new Vector2(60, 30)))
        {
            Plugin.Chat.SendMessage($"/p {playerName} bet amount is {formatBet}");
            Plugin.Chat.SendMessage($"/p --------------------------------------");
           

        }

        ImGui.PopStyleColor(3);
    }

    private void DrawGameButtons(string playerName, PlayerState playerState, CardManagement card)
    {
        Vector4 defaultButtonColor = new Vector4(0.3f, 0.5f, 0.9f, 1.0f);

        // Draw Hit Button
        DrawHitButton(playerName, playerState, card, defaultButtonColor);
        ImGui.SameLine();

        // Draw Hit2 Button
        DrawHit2Button(playerName, playerState, card, defaultButtonColor);
        ImGui.SameLine();

        // Draw Stand Button
        DrawStandButton(playerName, playerState, defaultButtonColor);
        ImGui.SameLine();

        // Draw Double Down Button
        DrawDoubleDownButton(playerName, playerState, card, defaultButtonColor);
        ImGui.SameLine();

        // Draw Split Button
        DrawSplitButton(playerName, playerState, defaultButtonColor);
    }

    private void DrawDealerSection()
    {
        try
        {
            ImGui.PushStyleColor(ImGuiCol.Button, ImGuiColors.TankBlue);

            ImGui.Text("Dealer Section:");
            ImGui.SameLine();

            string dealerName = string.IsNullOrEmpty(Plugin.Configuration.DealerName)
                ? "No dealer selected"
                : Plugin.Configuration.DealerName;

            ImGui.Text($"Dealer: {dealerName}");

            if (!string.IsNullOrEmpty(Plugin.Configuration.DealerName))
            {
                DrawDealerControls();
            }
            else
            {
                ImGui.TextColored(ImGuiColors.DalamudRed, "Please select a dealer in the settings menu");
            }

            ImGui.PopStyleColor();
        }
        catch (Exception ex)
        {
            Svc.Log.Error($"Error in DrawDealerSection: {ex.Message}");
        }
    }

    private void DrawDealerControls()
    {
        var card = new CardManagement();
        string handDescription = card.GetHandDescription(gameState.DealerCards);
        ImGui.Text($"Dealer's Cards: {handDescription} Total: {card.GetTotal(gameState.DealerCards)}");

        if (ImGui.Button("Hit", new Vector2(150, 30)))
        {
            int dealerCard = card.GetDiceResult();
            gameState.DealerCards.Add(dealerCard);
            Plugin.Chat.SendMessage($"/p Dealer drew a card: {dealerCard}");
            Plugin.Chat.SendMessage($"/p Dealer's Total: {card.GetTotal(gameState.DealerCards)}");
            Plugin.Chat.SendMessage($"/p ----------------------------------------");
        }

        DrawDealerActionButtons();
    }

    private void DrawDealerActionButtons()
    {
        ImGui.SameLine();
        if (ImGui.Button("Clear", new Vector2(150, 30)))
        {
            ClearAllPlayerStates();
        }

        ImGui.SameLine();
        if (ImGui.Button("Give winnings", new Vector2(150, 30)))
        {
            CalculateAndDistributeWinnings();
        }




        ImGui.SameLine();
        if (ImGui.Button("New round", new Vector2(150, 30)))
        {
            Plugin.Chat.SendMessage("/p A new round has started: Goodluck!");
            Plugin.Chat.SendMessage($"/p ----------------------------------------");
        }
    }

    private void DrawHitButton(string playerName, PlayerState playerState, CardManagement card, Vector4 defaultButtonColor)
    {
        try
        {
            if (playerState.IsStanding || playerState.HasUsedFirstHit)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Button, defaultButtonColor);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.4f, 0.6f, 1.0f, 1.0f));
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.2f, 0.4f, 0.8f, 1.0f));
            }

            if (ImGui.Button("Hit", new Vector2(80, 30)) && !playerState.IsStanding && !playerState.HasDoubledDown && !playerState.HasUsedFirstHit)
            {
                List<int> currentHand = playerState.IsPlayingSecondHand ?
                    playerState.SecondHand : playerState.CardValues;

                int diceResult = card.GetDiceResult();
                int diceResult2 = card.GetDiceResult();

               

                currentHand.Add(diceResult);
                currentHand.Add(diceResult2);

                if (diceResult == diceResult2 && !playerState.IsPlayingFirstHand && !playerState.IsPlayingSecondHand)
                {
                    playerState.CanSplit = true;
                }

                playerState.CanDoubleDown = true;
                playerState.HitButton2Enabled = true;
                playerState.HasUsedFirstHit = true;

                Plugin.Chat.SendMessage($"/p Random! (1-13) {diceResult}");
                Plugin.Chat.SendMessage($"/p Random! (1-13) {diceResult2}");
                Plugin.Chat.SendMessage($"/p {playerName}'s Total: {card.GetTotal(currentHand)}");
                Plugin.Chat.SendMessage("/p ------------------------------------------------------------------");

                if (card.GetTotal(currentHand) > 21)
                {
                    Plugin.Chat.SendMessage($"/p {playerName}'s hand busted with {card.GetTotal(currentHand)}");
                    Plugin.Chat.SendMessage($"/p ----------------------------------------");
                   
                    playerState.IsStanding = true;
                }
                else if (card.GetTotal(currentHand) == 21)
                {
                    Plugin.Chat.SendMessage($"/p {playerName} got a Natural 21!");
                    Plugin.Chat.SendMessage($"/p ----------------------------------------");
                   
                    playerState.IsStanding = true;
                }
            }

            ImGui.PopStyleColor(3);
        }
        catch (Exception ex)
        {
            Svc.Log.Error($"Error in DrawHitButton for {playerName}: {ex.Message}");
        }
    }

    private void DrawHit2Button(string playerName, PlayerState playerState, CardManagement card, Vector4 defaultButtonColor)
    {
        try
        {
            if (playerState.IsStanding || !playerState.HitButton2Enabled)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Button, defaultButtonColor);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.4f, 0.6f, 1.0f, 1.0f));
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.2f, 0.4f, 0.8f, 1.0f));
            }

            if (ImGui.Button("Hit 2", new Vector2(80, 30)) && playerState.HitButton2Enabled && !playerState.IsStanding && !playerState.HasDoubledDown)
            {
                int diceResult = card.GetDiceResult();
                playerState.CardValues.Add(diceResult);

                Plugin.Chat.SendMessage($"/p Random! (1-13) {diceResult}");
                Plugin.Chat.SendMessage($"/p {playerName}'s Total: {card.GetTotal(playerState.CardValues)}");
                Plugin.Chat.SendMessage($"/p ----------------------------------------");

                if (card.GetTotal(playerState.CardValues) > 21)
                {
                    Plugin.Chat.SendMessage($"/p {playerName} has busted: Total is {card.GetTotal(playerState.CardValues)}");
                    Plugin.Chat.SendMessage($"/p ----------------------------------------");
                    
                    playerState.IsStanding = true;
                }
            }

            ImGui.PopStyleColor(3);
        }
        catch (Exception ex)
        {
            Svc.Log.Error($"Error in DrawHit2Button for {playerName}: {ex.Message}");
        }
    }

    private void DrawStandButton(string playerName, PlayerState playerState, Vector4 defaultButtonColor)
    {
        try
        {
            if (playerState.IsStanding)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Button, defaultButtonColor);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.4f, 0.6f, 1.0f, 1.0f));
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.2f, 0.4f, 0.8f, 1.0f));
            }

            if (ImGui.Button("Stand", new Vector2(60, 30)) && !playerState.IsStanding)
            {
                playerState.IsStanding = true;
                playerState.HitButton2Enabled = false;
                Plugin.Chat.SendMessage($"/p Player {playerName} is standing for this round");
                Plugin.Chat.SendMessage($"/p ----------------------------------------");

            }

            ImGui.PopStyleColor(3);
        }
        catch (Exception ex)
        {
            Svc.Log.Error($"Error in DrawStandButton for {playerName}: {ex.Message}");
        }
    }

    private void DrawDoubleDownMessageButton(string playerName, PlayerState playerState, CardManagement card, Vector4 defaultButtonColor)
    {

        
        if (playerState.IsStanding || !playerState.CanDoubleDown)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.Button, defaultButtonColor);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.4f, 0.6f, 1.0f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.2f, 0.4f, 0.8f, 1.0f));
        }

        if (ImGui.Button("DD CM" , new Vector2 (60,30)) && playerState.CanDoubleDown && !playerState.IsStanding)
        {
            playerState.Bet *= 2;
            Plugin.Chat.SendMessage($"/p {playerName} doubled down! New bet: {FormatWinnings(playerState.Bet)}");
            Plugin.Chat.SendMessage($"/p ----------------------------------------");

        }

        ImGui.PopStyleColor(3);
    }

    private void DrawDoubleDownButton(string playerName, PlayerState playerState, CardManagement card, Vector4 defaultButtonColor)
    {
        try
        {
            if (playerState.IsStanding || !playerState.CanDoubleDown)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Button, defaultButtonColor);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.4f, 0.6f, 1.0f, 1.0f));
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.2f, 0.4f, 0.8f, 1.0f));
            }

            if (ImGui.Button("DD", new Vector2(60, 30)) && playerState.CanDoubleDown && !playerState.IsStanding)
            {
                playerState.Bet *= 2;
               

                int diceResult = card.GetDiceResult();
                playerState.CardValues.Add(diceResult);
                Plugin.Chat.SendMessage($"/p Random! (1-13) {diceResult}");
                Plugin.Chat.SendMessage($"/p {playerName}'s Total: {card.GetTotal(playerState.CardValues)}");
                Plugin.Chat.SendMessage($"/p ----------------------------------------");

                playerState.IsStanding = true;
                playerState.HasDoubledDown = true;
                playerState.CanDoubleDown = false;
                playerState.HitButton2Enabled = false;

                if (card.GetTotal(playerState.CardValues) > 21)
                {
                    Plugin.Chat.SendMessage($"/p {playerName} has busted: Total is {card.GetTotal(playerState.CardValues)}");
                    Plugin.Chat.SendMessage($"/p ----------------------------------------");
                   
                }
            }

            ImGui.PopStyleColor(3);
        }
        catch (Exception ex)
        {
            Svc.Log.Error($"Error in DrawDoubleDownButton for {playerName}: {ex.Message}");
        }
    }

    private void DrawSplitButton(string playerName, PlayerState playerState, Vector4 defaultButtonColor)
    {
        try
        {
            if (playerState.IsStanding || !playerState.CanSplit)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Button, defaultButtonColor);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.4f, 0.6f, 1.0f, 1.0f));
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.2f, 0.4f, 0.8f, 1.0f));
            }

            if (ImGui.Button("Split", new Vector2(60, 30)) && playerState.CanSplit && !playerState.IsStanding)
            {
                var firstCard = playerState.CardValues[0];
                var secondCard = playerState.CardValues[1];

                playerState.HasSplitAces = (firstCard == 1 && secondCard == 1);

                playerState.CardValues.Clear();
                playerState.CardValues.Add(firstCard);

                playerState.SecondHand.Clear();
                playerState.SecondHand.Add(secondCard);

                playerState.IsPlayingFirstHand = true;
                playerState.IsPlayingSecondHand = false;

                playerState.Bet *= 2;

                Plugin.Chat.SendMessage($"/p {playerName} split their pair! New bet per hand: {FormatWinnings(playerState.Bet / 2)}");
                Plugin.Chat.SendMessage($"/p Playing first hand...");
                Plugin.Chat.SendMessage($"/p ----------------------------------------");

                playerState.CanSplit = false;
            }

            ImGui.PopStyleColor(3);
        }
        catch (Exception ex)
        {
            Svc.Log.Error($"Error in DrawSplitButton for {playerName}: {ex.Message}");
        }
    }

    private void CalculateAndDistributeWinnings()
    {
        try
        {
            var card = new CardManagement();
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
                    }
                }
            }

            // Show winner window if there was a winner
            if (!string.IsNullOrEmpty(roundWinner))
            {

                //winnerDisplay.ShowWinner(roundWinner);
                Plugin.Chat.SendMessage($"{roundWinner} won the round!");

            }
            else
            {
                Plugin.Chat.SendMessage($"The dealer won the round");
            }


        }
        catch (Exception ex)
        {
            Svc.Log.Error($"Error in CalculateAndDistributeWinnings: {ex.Message}");
        }
    }

    private void ClearAllPlayerStates()
    {
        try
        {
            foreach (var player in gameState.Players.Values)
            {
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
                player.IsInitialized = true;
                player.HasValidBet = false;
            }

            // Clear dealer's cards
            gameState.DealerCards.Clear();

            Plugin.Chat.SendMessage("/p Cleared all player and dealer data for the next round.");
            Plugin.Chat.SendMessage($"/p ----------------------------------------");
        }
        catch (Exception ex)
        {
            Svc.Log.Error($"Error in ClearAllPlayerStates: {ex.Message}");
        }
    }
}
