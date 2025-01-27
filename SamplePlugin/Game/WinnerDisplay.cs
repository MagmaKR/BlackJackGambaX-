using System.Numerics;
using Dalamud.Interface.Colors;
using ImGuiNET;
using SamplePlugin.Windows;

namespace SamplePlugin.Game;

public class WinnerDisplay
{
    private Plugin Plugin;
    private bool showWindow;
    private string winnerName = string.Empty;
    private float windowTimer;
    private const float WINDOW_DURATION = 4.0f;

    public WinnerDisplay(Plugin plugin)
    {
        this.Plugin = plugin;
    }

    public void ShowWinner(string winner)
    {
        winnerName = winner;
        showWindow = true;
        windowTimer = 0f;
    }

   

    public void Draw()
    {
        if (!showWindow) return;

        var viewport = ImGui.GetMainViewport();
        var center = viewport.GetCenter();
        ImGui.SetNextWindowPos(center, ImGuiCond.Always, new Vector2(0.5f, 0.5f));

        ImGuiWindowFlags flags = ImGuiWindowFlags.NoMove |
                                ImGuiWindowFlags.NoResize |
                                ImGuiWindowFlags.AlwaysAutoResize |
                                ImGuiWindowFlags.NoCollapse;

        if (ImGui.Begin("Winner Announcement##WinnerPopup", ref showWindow, flags))
        {
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.TankBlue);
            ImGui.SetWindowFontScale(2.0f);
            ImGui.Text($"{winnerName} won the round!");
            ImGui.SetWindowFontScale(1.0f);
            ImGui.PopStyleColor();

            windowTimer += ImGui.GetIO().DeltaTime;
            if (windowTimer >= WINDOW_DURATION)
            {
                showWindow = false;
                windowTimer = 0f;
            }
        }
        ImGui.End();
    }
}
