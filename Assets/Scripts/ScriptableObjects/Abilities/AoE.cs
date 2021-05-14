using Globals;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjects.Abilities
{
    public class AoE : Ability
    {
        [BoxGroup("Variables"), PropertyRange(0, GV.MAXArea)]
        public int area = 1;

        public override string GetAbilityString()
        {
            string str = "Type: " + "AOE\n" + base.GetAbilityString();
            if (area > 0)
                str += $"Area: {area}";
            return str;
        }

        public override void UseAbility(Unit host, Vector3Int targetPosInt)
        {
            base.UseAbility(host, targetPosInt);

            if (canUseAbility || host is AIEnemy)
            {
                if (host is AIEnemy)
                {
                    OverlayProperties properties = new OverlayProperties();
                    properties.canUseAbility = host.unitData.stats.Ap > apCost;
                    properties.canSelfCastOnly = canCastSelfOnly;
                    properties.canCastOnFriendlies = canCastOnFriendlies;
                    properties.canDamageFriendlies = canDamageFriendlies;
                    Tuple<OverlayProperties, Ability> tuple = new Tuple<OverlayProperties, Ability>(properties, this);
                    GridManager.DrawRangeOverlay(OverlayType.Diamond, area, targetPosInt, tuple.Item1, host);
                }

                if (abilityEffect != null)
                {
                    GameObject go = Instantiate(abilityEffect, GridManager.GetTileToCellFromWorld(targetPosInt), Quaternion.identity);
                    Destroy(go, 1f);
                }
                SetAnimationClip(host, AnimationManager.aoeTrigger);
                AreaDamage(GridManager.GetUnitsInRange());
                if (host is AIEnemy)
                    GridManager.RangeOverlayTiles.Clear();
            }
        }

        protected override bool CheckIfAbilityCanBeUsed(Unit host, Vector3Int targetPosInt)
        {
            if (host.unitData.stats.Ap >= apCost)
            {
                if (canCastSelfOnly && UnitManager.Instance.unitDictionnary[targetPosInt].unitData == host.unitData)
                    return true;
                else if (canCastOnFriendlies)
                    return true;
                else if (!canCastOnFriendlies && UnitManager.Instance.unitDictionnary[targetPosInt].unitData.GetType() != host.GetType())
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Apply damage to all Units inside the Area of Effect
        /// </summary>
        /// <param name="targets"></param>
        protected void AreaDamage(List<Unit> targets)
        {
            foreach (Unit unit in targets)
            {
                ApplyDMG(unit.positionGrid);
            }
        }
    }
}