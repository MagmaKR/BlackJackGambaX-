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
        foreach (var value in cardValues)
        {
            total += value;
        }
        return total;
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
