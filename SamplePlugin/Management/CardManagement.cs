using ECommons.DalamudServices;
using System.Collections.Generic;
using System;

public partial class CardManagement
{

    private Dictionary<string, List<int>> playerCardValues = new Dictionary<string, List<int>>();
    private string? dealerName;
    public int GetDiceResult()
    {
        return new Random().Next(1, 13);
    }

    public int GetTotal(List<int> cardValues)
    {
        int total = 0;
        int aceCount = 0;

        // First count aces and non-aces separately
        foreach (var value in cardValues)
        {
            if (value == 1) // Ace
            {
                aceCount++;
            }
            else
            {
                total += value;
            }
        }

        // Add aces, counting them as 11 when possible
        for (int i = 0; i < aceCount; i++)
        {
            if (total + 11 <= 21)
            {
                total += 11;
            }
            else
            {
                total += 1;
            }
        }

        return total;
    }

    public string GetHandDescription(List<int> cardValues)
    {
        var cards = new List<string>();
        foreach (var value in cardValues)
        {
            if (value == 1)
            {
                cards.Add("A");
            }
            else
            {
                cards.Add(value.ToString());
            }
        }
        return string.Join(", ", cards);
    }

    public void ClearPlayerData()
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
