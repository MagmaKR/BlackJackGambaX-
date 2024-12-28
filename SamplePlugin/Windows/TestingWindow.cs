using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using static SamplePlugin.Plugin;

namespace SamplePlugin.Windows;

public class TestingWindow : Window, IDisposable
{
    Plugin plugin;
    private Dictionary<string, int> playerBets = new Dictionary<string, int>();
    string PlayerName = clientState.LocalPlayer.Name.TextValue;

    public TestingWindow(Plugin plugin) : base("Testing Window")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;


        Size = new Vector2(1200, 700);
        SizeCondition = ImGuiCond.Always;


    }

    public override void Draw()
    {
        ImGui.TextColored(ImGuiColors.TankBlue, "Barr-Berry-Nyans Gamba Plugin");
        var spacing = ImGui.GetScrollMaxY() == 0 ? 100f : 100f;
        ImGui.SameLine(ImGui.GetWindowWidth() - spacing);
        if (ImGui.Button("Show Settings"))
        {
            plugin.ToggleConfigUI();
        }

        float testingButtonSpacing = spacing + ImGui.CalcTextSize("Show Settings").X + ImGui.GetStyle().ItemSpacing.X + 30f;
        ImGui.SameLine(ImGui.GetWindowWidth() - testingButtonSpacing);

        // "Testing Window" button
        if (ImGui.Button("Testing Window"))
        {
            plugin.ToggleTestWindowUI();
        }
        ImGui.BeginChild("Players", new Vector2(0, -100), true);
        ImGui.TextColored(ImGuiColors.TankBlue, "Party Members");

        // Place leaderboard at the top right within "Players" frame

        ImGui.SameLine(ImGui.GetWindowWidth() - 250); // Position at the right
        // Move the leaderboard down a bit (for example, by 20 units)
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 32);
        {
            ImGui.PushStyleColor(ImGuiCol.ChildBg, ImGuiColors.DalamudGrey);

            ImGui.BeginChild("LeaderboardSection", new Vector2(200, 400), true); // Fixed height of 400

            ImGui.Text("Leaderboard:");

            // Example leaderboard entries
            for (int j = 0; j < 5; j++)
            {
                ImGui.Text($"Player {j + 1}: Score");
            }
            ImGui.PopStyleColor();
            ImGui.EndChild(); // end of leaderbord child 
        }


        // Now add some vertical spacing to ensure player list starts below the leaderboard
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 10);

        ImGui.SetCursorPosY(40);
        // Calculate child frame size based on available space
        var childFrameSize = new Vector2(900, 100); // Adjust width for leaderboard



        for (int i = 0; i < 8; i++)
        {
            ImGui.PushStyleColor(ImGuiCol.ChildBg, ImGuiColors.DalamudGrey);
            // Unique ID for each child
            ImGui.PushID(i);

            // Begin player child frame (bordered)
            ImGui.BeginChild($"Player_{i}", childFrameSize, true, ImGuiWindowFlags.NoScrollbar);
            
            // Player name
            ImGui.Text(PlayerName);

            // Button color logic (consider using a dedicated color variable)
            var buttonColor = i % 2 == 0 ? (ImGuiColors.TankBlue) : (ImGuiColors.ParsedBlue); // Adjust colors as needed
            ImGui.PushStyleColor(ImGuiCol.Button, buttonColor);

            // Betting functionality
            int bet = 0;
            ImGui.SameLine();
            if (ImGui.Button("Bet", new Vector2(60, 30)))
            {
                plugin.Chat.SendMessage($"{plugin.PlayerNameUI.showPlayerNames()} bet amount is {bet}");
            }

            // Hit cards and stand buttons
            ImGui.SameLine();
            if (ImGui.Button("Hit card 1", new Vector2(90, 30)))
            {
                plugin.Chat.SendMessage("Dice roll 1 for player ");
                Thread.Sleep(2000);
                plugin.Chat.SendMessage("/dice party 13");
            }
            ImGui.NewLine();
            if (ImGui.Button("Hit card 2", new Vector2(90, 30)))
            {
                plugin.Chat.SendMessage("/dice party 13");
            }
            ImGui.SameLine();
            bool ButtonHit = false;
            if (ImGui.Button("Stand", new Vector2(60, 30)))
            {
                ButtonHit = !ButtonHit;
            }
            if (ButtonHit)
            {
                ImGui.Text($"Player:{plugin.PlayerNameUI.showPlayerNames()} is standing");
            }

            // Pop button color
            ImGui.PopStyleColor();

            // Split and DD buttons
            ImGui.SameLine();
            ImGui.Button("Split", new Vector2(60, 30));
            ImGui.SameLine();
            ImGui.Button("DD", new Vector2(60, 30));
            ImGui.PopStyleColor();

            // Input field for bet amount
            ImGui.PushItemWidth(200);
            ImGui.NewLine();
            if (ImGui.InputInt($"Bet of player {PlayerName}, ", ref bet, 500000))
            {
                playerBets[PlayerName] = bet; // Might need adjustment for per-player bets
            }
            ImGui.PopItemWidth();

            // Card amount and leaderboard section (optional)
            ImGui.SameLine();
            ImGui.Text("Card amount: cards"); // Replace "cards" with actual card count/info

            ImGui.PopStyleColor();
            // End of player's bordered child
            ImGui.EndChild();

            // Pop unique ID
            ImGui.PopID();

        }



        ImGui.EndChild();


        //============================================================================
        {

            ImGui.BeginChild("Bottom section Dealer", new Vector2(0, 90), true);

            {

                ImGui.PushStyleColor(ImGuiCol.Button, ImGuiColors.TankBlue);
                ImGui.BeginChild("DealerSection", new Vector2(ImGui.GetWindowWidth() * 0.7f - 50f, 0), true);
                ImGui.Text("Dealer Section:");
                ImGui.SameLine();
                //ImGui.Text(plugin.dealerName != "" ? plugin.dealerName : "No Dealer Selected");

                if (ImGui.Button("Dealer's first card ", new Vector2(150, 25)))
                {

                }

                ImGui.SameLine();
                ImGui.Button("Reveal second card", new Vector2(150, 25));
                ImGui.SameLine();


                ImGui.Button("Give winnings", new Vector2(150, 25));

                ImGui.SameLine();
                ImGui.Button("Show rules", new Vector2(150, 25));

                ImGui.NewLine();
                ImGui.Text("Dealer card amount");


                ImGui.EndChild();
            }
            ImGui.SameLine();
            {
                var BerryDinoPath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "C:\\Users\\Keegan\\Source\\Repos\\Dalamud-Template\\Gamba_BlackJackXPlugin\\Images\\BerryDino-removebg-preview.png");
                var BerryDinoImg = Plugin.TextureProvider.GetFromFile(BerryDinoPath).GetWrapOrDefault();

                var BlackJackPath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "C:\\Users\\Keegan\\Source\\Repos\\Dalamud-Template\\Gamba_BlackJackXPlugin\\Images\\blackJack-removebg-preview.png");
                var BlackJackImg = Plugin.TextureProvider.GetFromFile(BlackJackPath).GetWrapOrDefault();
                if (BerryDinoImg != null)
                {
                    ImGui.Image(BerryDinoImg.ImGuiHandle, new Vector2(125, 80));
                    ImGui.SameLine();
                    ImGui.Image(BlackJackImg.ImGuiHandle, new Vector2(200, 100));
                    ImGui.SameLine();
                    ImGui.Text("Created by\n MagmaK");

                }
                else
                {
                    ImGui.Text("Image not found");
                }

            }
            ImGui.EndChild();
        }



    }

    public void Dispose() { }


}
