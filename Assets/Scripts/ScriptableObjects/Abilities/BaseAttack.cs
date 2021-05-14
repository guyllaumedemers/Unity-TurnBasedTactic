using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjects.Abilities
{
    public class BaseAttack : Ability
    {
        /// <summary>
        /// Base Attack can only affect the opposite type
        /// </summary>
        /// <param name="host"></param>
        /// <param name="targetPosInt"></param>
        public override void UseAbility(Unit host, Vector3Int targetPosInt)
        {
            base.UseAbility(host, targetPosInt);

            if (canUseAbility || host is AIEnemy)
            {
                SetAnimationClip(host, AnimationManager.attackTrigger);
                ApplyDMG(targetPosInt, host.unitData.stats.Dmg);
            }
        }
    }
}
