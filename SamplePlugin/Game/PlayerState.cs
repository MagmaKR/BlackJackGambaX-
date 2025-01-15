using System.Collections.Generic;

namespace SamplePlugin.Game;

public class PlayerState
{
    public int Bet { get; set; }
    public List<int> CardValues { get; set; } = new List<int>();
    public double Winnings { get; set; }
    public bool HitButton2Enabled { get; set; }
    public bool IsStanding { get; set; }
    public bool CanDoubleDown { get; set; }
    public bool CanSplit { get; set; }
    public bool HasDoubledDown { get; set; }
    public bool IsPlayingFirstHand { get; set; }
    public bool IsPlayingSecondHand { get; set; }
    public List<int> SecondHand { get; set; } = new List<int>();
    public bool HasSplitAces { get; set; }
    public bool HasUsedFirstHit { get; set; }

    public PlayerState()
    {
        Reset();
    }

    public void Reset()
    {
        Bet = 0;
        CardValues = new List<int>();
        Winnings = 0;
        HitButton2Enabled = false;
        IsStanding = false;
        CanDoubleDown = false;
        CanSplit = false;
        HasDoubledDown = false;
        IsPlayingFirstHand = false;
        IsPlayingSecondHand = false;
        SecondHand = new List<int>();
        HasSplitAces = false;
        HasUsedFirstHit = false;
    }
} 
