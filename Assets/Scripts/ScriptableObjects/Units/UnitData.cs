using System.Linq;
using Globals;
using ScriptableObjects.Abilities;
using ScriptableObjects.Stats;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace ScriptableObjects.Units
{
    public abstract class UnitData : SerializedScriptableObject
    {
        [BoxGroup("Info"), HorizontalGroup("Info/Split", 55, LabelWidth = 70)]
        [HideLabel, PreviewField(55, ObjectFieldAlignment.Left)]
        public Texture icon;
        
        [PreviewField(45, ObjectFieldAlignment.Left), VerticalGroup("Info/Split/Right")]
        public Sprite sprite;
        
        [VerticalGroup("Info/Split/Right")]
        public string Name;

        [VerticalGroup("Info/Split/Right")]
        public RuntimeAnimatorController AnimatorController;

        [TabGroup("Stats"), HideLabel]
        public BaseStats stats = new BaseStats();

        [TabGroup("Abilities")]
        [InfoBox("Max Abilities reached", InfoMessageType.Warning, "@abilities.Length >= GV.MAXAbilities")]
        [ListDrawerSettings(Expanded = true)]
        public Ability[] abilities = new Ability[GV.MAXAbilities];

        private void OnValidate()
        {
            if (!abilities.IsNullOrEmpty() && abilities.Length > 0)
            {
                if (!abilities.Last().SafeIsUnityNull() && abilities.Length <= GV.MAXAbilities)
                    abilities = abilities.Distinct().ToArray();
                else
                    abilities = abilities.Take(abilities.Length - 1).ToArray();
            }
        }
        public Ability GetAbilityFromString(string abilityName) => 
            abilities.FirstOrDefault(a => a.Name.ToLower().Equals(abilityName.ToLower()));
    }
}