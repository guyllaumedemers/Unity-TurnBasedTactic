using ScriptableObjects.Abilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ScriptableObjects
{
    /// <summary>
    /// Unit cannot die for duration (damage doesnt get applied)
    /// </summary>
    public class Immunity : Buff
    {
        [BoxGroup("Variables")]
        public bool immune;
        
        public override string GetAbilityString() => $"{base.GetAbilityString()} Immunity\n";
        
        public override void UseAbility(Unit host, Vector3Int targetPosInt)
        {
            base.UseAbility(host, targetPosInt);
            
            if (abilityEffect != null)
            {
                SetAnimationClip(host, AnimationManager.spellTrigger);
                GameObject go = Instantiate(abilityEffect, GridManager.GetTileToCellFromWorld(targetPosInt), Quaternion.identity);
                Destroy(go, 1f);
            }
            if (canUseAbility)
                host.Immunity = immune;
        }
    }
}