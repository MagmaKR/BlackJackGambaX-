using System.Collections.Generic;
using System;
using System.Linq;

public class BetManager
{
    public Dictionary<string, int> playerBets = new Dictionary<string, int>();
    public Dictionary<string, int> leaderboard = new Dictionary<string, int>();
    public List<int> dealerCards = new List<int>();




    public void ResolveRound(Dictionary<string, List<int>> playerCards)
    {
        int dealerTotal = dealerCards.Sum();

        foreach (var player in playerCards)
        {
            string playerName = player.Key;
            int playerTotal = player.Value.Sum();

            if (playerTotal > 21) // Bust
            {
                leaderboard[playerName] = 0;
            }
            else if (playerTotal == 21 && player.Value.Count == 2) // Natural Blackjack
            {
                leaderboard[playerName] += (int)(playerBets[playerName] * 2.5);
            }
            else if (playerTotal > dealerTotal || dealerTotal > 21) // Win
            {
                leaderboard[playerName] += playerBets[playerName] * 2;
            }
            else if (playerTotal == dealerTotal) // Draw
            {
                leaderboard[playerName] += playerBets[playerName];
            }
            else // Loss
            {
                leaderboard[playerName] = 0;
            }
        }
    }


}
