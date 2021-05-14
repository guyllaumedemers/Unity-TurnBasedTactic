using Sirenix.OdinInspector;
using UnityEngine;

namespace ScriptableObjects.Abilities
{
    public class Taunt : Ability
    {
        [Space(10)]
        [BoxGroup("Variables")] 
        public Unit target;

        public override string GetAbilityString()
        {
            string str = "Type: " + "Taunt\n" + base.GetAbilityString();

            return str;
        }

        public override void UseAbility(Unit host, Vector3Int targetPosInt)
        {
            base.UseAbility(host, targetPosInt);

            if (canUseAbility || host is AIEnemy)
            {
                if (abilityEffect != null)
                {
                    SetAnimationClip(host, AnimationManager.spellTrigger);
                    GameObject go = Instantiate(abilityEffect, GridManager.GetTileToCellFromWorld(targetPosInt), Quaternion.identity);
                    Destroy(go, 1f);
                }
                target = GridManager.GetUnitAtTile(targetPosInt);
            }
        }
    }
}