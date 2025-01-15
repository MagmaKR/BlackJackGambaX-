using System.Collections.Generic;

namespace SamplePlugin.Game;

public class GameState
{
    public Dictionary<string, PlayerState> Players { get; set; } = new Dictionary<string, PlayerState>();
    public List<int> DealerCards { get; set; } = new List<int>();
    public bool ShowRules { get; set; }
    public bool ShowRules2 { get; set; }
    
    public PlayerState GetOrCreatePlayerState(string playerName)
    {
        if (!Players.ContainsKey(playerName))
        {
            Players[playerName] = new PlayerState();
        }
        return Players[playerName];
    }

    public void Reset()
    {
        foreach (var player in Players.Values)
        {
            player.Reset();
        }
        DealerCards.Clear();
    }

    public bool HasPlayer(string playerName) => Players.ContainsKey(playerName);
} 
