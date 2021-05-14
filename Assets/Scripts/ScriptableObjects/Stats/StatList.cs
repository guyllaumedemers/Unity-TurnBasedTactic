using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif

namespace ScriptableObjects.Stats
{
    [Serializable]
    public class StatList
    {
        [SerializeField]
        [ValueDropdown("CustomAddStatsButton", IsUniqueList = true, DrawDropdownForListElements = false, DropdownTitle = "Modify Stats")]
        [ListDrawerSettings(DraggableItems = false, Expanded = true)]
        public List<StatValue> stats = new List<StatValue>();

        public override string ToString()
        {
            return stats.Where(stat => stat.value != 0)
                .Aggregate("", (current, stat) => current + stat.type switch
                {
                    StatType.Ap => $"{stat.type} : {GetPosNegString(stat.value)}{stat.value}".AddColor(Color.cyan) + "\n",
                    StatType.Hp => $"{stat.type} : {GetPosNegString(stat.value)}{stat.value}".AddColor(Color.green) + "\n",
                    StatType.Dmg => $"{stat.type} : {GetPosNegString(stat.value)}{stat.value}".AddColor(Color.red) + "\n",
                    StatType.Def => $"{stat.type} : {GetPosNegString(stat.value)}{stat.value}".AddColor(Color.magenta) + "\n",
                    _ => string.Empty
                });
        }
    
        private string GetPosNegString(int value) => value > 0 ? "+" : "-";
        
        public StatValue this[int index]
        {
            get => stats[index];
            set => stats[index] = value;
        }

        public int Count => stats.Count;

        public int this[StatType type]
        {
            get
            {
                for (int i = 0; i < stats.Count; i++)
                {
                    if (stats[i].type == type)
                        return stats[i].value;
                }

                return 0;
            }
            set
            {
                for (int i = 0; i < stats.Count; i++)
                {
                    if (stats[i].type == type)
                    {
                        var val = stats[i];
                        val.value = value;
                        stats[i] = val;
                        return;
                    }
                }

                stats.Add(new StatValue(type, value));
            }
        }

        private IEnumerable CustomAddStatsButton()
        {
            return Enum.GetValues(typeof(StatType))
                .Cast<StatType>()
                .Except(stats.Select(x => x.type))
                .Select(x => new StatValue(x))
                .AppendWith(stats)
                .Select(x => new ValueDropdownItem(x.type.ToString(), x));
        }
    }

#if UNITY_EDITOR
    internal class StatListValueDrawer : OdinValueDrawer<StatList>
    {
        protected override void DrawPropertyLayout(GUIContent label) => Property.Children[0].Draw(label);
    }
#endif
}
