using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace SamplePlugin;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool IsConfigWindowMovable { get; set; } = true;
    
    // Add saved emote configuration
    public string SavedValueHit { get; set; } = string.Empty;
    public string SavedHitText { get; set; } = string.Empty;
    public string SavedStandValue { get; set; } = string.Empty;
    public string SavedDoubleDownValue { get; set; } = string.Empty;
    public string SavedRulesEmote { get; set; } = string.Empty;
    public string SavedBetEmote { get; set; } = string.Empty;
    public string SavedNatural21Emote { get; set; } = string.Empty;
    public string SavedBustEmote { get; set; } = string.Empty;

    // Current values (these will be used in real-time)
    public string ValueHit { get; set; } = string.Empty;
    public string HitText { get; set; } = string.Empty;
    public string StandValue { get; set; } = string.Empty;
    public string DoubleDownValue { get; set; } = string.Empty;
    public string RulesEmote { get; set; } = string.Empty;
    public string BetEmote { get; set; } = string.Empty;
    public string Natural21Emote { get; set; } = string.Empty;
    public string BustEmote { get; set; } = string.Empty;
    public string DealerName { get; set; } = string.Empty;

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }

    public void SaveCurrentValues()
    {
        SavedValueHit = ValueHit;
        SavedHitText = HitText;
        SavedStandValue = StandValue;
        SavedDoubleDownValue = DoubleDownValue;
        SavedRulesEmote = RulesEmote;
        SavedBetEmote = BetEmote;
        SavedNatural21Emote = Natural21Emote;
        SavedBustEmote = BustEmote;
        Save();
    }

    public void LoadSavedValues()
    {
        ValueHit = SavedValueHit;
        HitText = SavedHitText;
        StandValue = SavedStandValue;
        DoubleDownValue = SavedDoubleDownValue;
        RulesEmote = SavedRulesEmote;
        BetEmote = SavedBetEmote;
        Natural21Emote = SavedNatural21Emote;
        BustEmote = SavedBustEmote;
    }
}
