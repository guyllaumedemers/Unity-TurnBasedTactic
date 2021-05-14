using System.Linq;
using ScriptableObjects.Stats;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ScriptableObjects.Items
{
    public class Consumable : Item
    {
        [VerticalGroup(BoxGroupName + "/Split/Right"), PropertyRange(0, 20)]
        public int cooldown;
        
        [VerticalGroup(BoxGroupName + "/Split/Right"), PropertyRange(0, 20)]
        public int duration;
        
        [BoxGroup("Modifiers")]
        public StatList mods;

        public override string GetModsString()
        {
            return mods.stats.Where(stat => stat.value != 0)
                .Aggregate("", (current, stat) => current + stat.type switch
                {
                    StatType.Ap => $"{stat.type} : {stat.value}".AddColor(Color.blue) + "\n",
                    StatType.Hp => $"{stat.type} : {stat.value}".AddColor(Color.green) + "\n",
                    StatType.Dmg => $"{stat.type} : {stat.value}".AddColor(Color.red) + "\n",
                    StatType.Def => $"{stat.type} : {stat.value}".AddColor(Color.magenta) + "\n",
                    _ => string.Empty
                });
        }
    }
}