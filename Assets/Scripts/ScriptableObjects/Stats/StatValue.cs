using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ScriptableObjects.Stats
{
    [Serializable]
    public struct StatValue : IEquatable<StatValue>
    {
        [HideInInspector]
        public StatType type;

        [Range(sbyte.MinValue, sbyte.MaxValue), LabelWidth(70), LabelText("$type")]
        public int value;

        public StatValue(StatType type, int value)
        {
            this.type = type;
            this.value = value;
        }
        
        public StatValue(StatType type)
        {
            this.type = type;
            value = 0;
        }

        public bool Equals(StatValue other) => type == other.type && Math.Abs(value - other.value) < 0f;
    }
}
