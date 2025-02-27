using ECommons.DalamudServices;
using System.Collections.Generic;
using System;
using SamplePlugin.Windows;
using Lumina.Excel.Sheets;
using SamplePlugin;
using Dalamud.Interface.Windowing;
using System.Net.Http.Headers;

public partial class CardManagement
{

    private Dictionary<string, List<int>> playerCardValues = new Dictionary<string, List<int>>();
    private string? dealerName;
    private readonly bool aceMessage = false;
    Plugin?  plugin;
    
    public int GetDiceResult()
    {
        return new Random().Next(1, 13);
    }

    public int GetTotal(List<int> cardValues)
    {
        int total = 0;
        int aceCount = 0;
        bool aceMessage = false;

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
                aceMessage = true;
            }
            else
            {
                total += 1;
            }
        }
        //Display the ace message 
        if (aceMessage)
        {
            //initiate main window class 
            var window = new MainWindow(plugin);
            string currentMember = window.playerName;
            //revert the ace as a 1 
            int altAceTotal = total - 10; //coverts the ace from 11 to 1 
            plugin.Chat.SendMessage($"{currentMember}, you have rolled an ace, Your result could have been Ace as a 1: {altAceTotal} or ace as an 11: {total}");
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
