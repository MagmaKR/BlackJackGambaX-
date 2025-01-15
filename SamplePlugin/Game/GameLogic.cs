using System.Collections.Generic;

namespace SamplePlugin.Game;

public class GameLogic
{
    public static double CalculateWinnings(PlayerState player, int dealerTotal)
    {
        int playerTotal = GetTotal(player.CardValues);
        
        if (playerTotal == 21 && player.CardValues.Count == 2)
        {
            return player.Bet * 2.5; // Natural BlackJack
        }
        else if (playerTotal <= 21 && (playerTotal > dealerTotal || dealerTotal > 21))
        {
            return player.Bet * 2; // Regular win
        }
        else if (playerTotal <= 21 && playerTotal == dealerTotal)
        {
            return player.Bet; // Push (tie)
        }
        
        return 0; // Loss or bust
    }

    public static int GetTotal(List<int> cards)
    {
        int total = 0;
        int aces = 0;

        foreach (int card in cards)
        {
            if (card == 1)
            {
                aces++;
                total += 11;
            }
            else
            {
                total += card > 10 ? 10 : card;
            }
        }

        while (total > 21 && aces > 0)
        {
            total -= 10;
            aces--;
        }

        return total;
    }
} 
