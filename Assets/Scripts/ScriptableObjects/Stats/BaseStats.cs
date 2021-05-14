using System;
using Globals;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ScriptableObjects.Stats
{
    [Serializable]
    public class BaseStats
    {
        [BoxGroup("Stats Range"), LabelWidth(60), PropertyRange(1, GV.MAXAp)]
        public ushort maxAp = 10;
        [BoxGroup("Stats Range"), LabelWidth(60), PropertyRange(1, GV.MAXStat)]
        public ushort maxStat = 100;
        
        [HideInInspector]
        public StatList stats = new StatList();
        
        [BoxGroup("Stats"), LabelWidth(60), ProgressBar(0, "maxAp"), ShowInInspector]
        public int Ap
        {
            get => stats[StatType.Ap];
            set => stats[StatType.Ap] = value;
        }
        
        [BoxGroup("Stats"), LabelWidth(60), ProgressBar(0, "maxStat"), ShowInInspector]
        public int Hp
        {
            get => stats[StatType.Hp];
            set => stats[StatType.Hp] = value;
        }

        [BoxGroup("Stats"), LabelWidth(60), ProgressBar(0, "maxStat"), ShowInInspector]
        public int Dmg
        {
            get => stats[StatType.Dmg];
            set => stats[StatType.Dmg] = value;
        }
        
        [BoxGroup("Stats"), LabelWidth(60), ProgressBar(0, "maxStat"), ShowInInspector]
        public int Def
        {
            get => stats[StatType.Def];
            set => stats[StatType.Def] = value;
        }
    }
}
