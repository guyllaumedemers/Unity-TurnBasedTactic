using ScriptableObjects.Stats;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ScriptableObjects.Abilities
{
    public class Buff : Ability
    {
        [Space(10)]
        [BoxGroup("Variables")]
        public StatList mods;
        
        public override string GetAbilityString() => $"{base.GetAbilityString()}\nBUFF:\n{mods}";
        
        public override void UseAbility(Unit host, Vector3Int targetPosInt)
        {
            base.UseAbility(host, targetPosInt);

            if (canUseAbility || host is AIEnemy)
            {
                SetAnimationClip(host, AnimationManager.spellTrigger);
                if (abilityEffect != null)
                {
                    GameObject go = Instantiate(abilityEffect, GridManager.GetTileToCellFromWorld(targetPosInt), Quaternion.identity);
                    Destroy(go, 1f);
                }

                host.UseBuff(this);
            }           
        }
    }
}