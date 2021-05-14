using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjects.Abilities
{
    public class Line : Ability
    {
        public override string GetAbilityString()
        {
            string str = "Type: " + "Line\n" + base.GetAbilityString();

            return str;
        }
        public override void UseAbility(Unit host, Vector3Int targetPosInt)
        {
            base.UseAbility(host, targetPosInt);

            if (canUseAbility || host is AIEnemy)
            {
                SetAnimationClip(host, AnimationManager.spellTrigger);

                OverlayProperties properties = new OverlayProperties();
                properties.canUseAbility = host.unitData.stats.Ap > apCost;
                properties.canSelfCastOnly = canCastSelfOnly;
                properties.canCastOnFriendlies = canCastOnFriendlies;
                properties.canDamageFriendlies = canDamageFriendlies;

                Tuple<OverlayProperties, Ability> tuple = new Tuple<OverlayProperties, Ability>(properties, this);
                //if (host is AIEnemy)
                GridManager.ClearTiles();
                GridManager.DrawRangeOverlay(OverlayType.Cross, range - 1, targetPosInt, tuple.Item1, host);
                List<Unit> units = GridManager.GetUnitsInRange();

                if (host is AIEnemy)
                    GridManager.RangeOverlayTiles.Clear();

                List<Unit> ToRemove = new List<Unit>();

                foreach (var unit in units)
                {
                    if (host.positionGrid.x == targetPosInt.x)
                    {
                        if (unit.positionGrid.y == targetPosInt.y)
                        {
                            ToRemove.Add(unit);
                        }
                    }
                    else if (host.positionGrid.y == targetPosInt.y)
                    {
                        if (unit.positionGrid.x == targetPosInt.x)
                        {
                            ToRemove.Add(unit);
                        }
                    }
                }

                foreach (var item in ToRemove)
                    units.Remove(item);

                if (UnitManager.Instance.unitDictionnary.ContainsKey(targetPosInt))
                    units.Add(UnitManager.Instance.unitDictionnary[targetPosInt]);

                if (units.Contains(host))
                    units.Remove(host);

                AreaDamage(units);

                if (abilityEffect != null)
                {
                    GameObject go = Instantiate(abilityEffect, GridManager.GetTileToCellFromWorld(host.positionGrid), Quaternion.identity);
                    go.AddComponent<MoveLineAbility>().end = targetPosInt;
                }
            }
        }

        protected void AreaDamage(List<Unit> targets)
        {
            foreach (Unit unit in targets)
            {
                ApplyDMG(unit.positionGrid);
            }
        }
    }
}