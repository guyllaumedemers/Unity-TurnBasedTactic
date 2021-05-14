using System.Collections.Generic;
using ScriptableObjects.Stats;

public class BuffDuration
{
    public readonly string name;
    public readonly List<StatValue> mods;
    public int duration;
    
    public BuffDuration(string abilityName, int abilityDuration, List<StatValue> modsStats)
    {
        name = abilityName;
        mods = modsStats;
        duration = abilityDuration;
    }
}