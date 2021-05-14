using Globals;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjects.Abilities
{
    public class AoEHeal : Ability
    {
        [BoxGroup("Variables"), PropertyRange(0, GV.MAXArea)]
        public int area = 1;

        public override string GetAbilityString()
        {
            string str = "Type: " + "AOEHeal\n" + base.GetAbilityString();
            if(area > 0)
                str += $"Area: {area}";
            return str;
        }
        
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

                if (UnitManager.Instance.unitDictionnary[targetPosInt] is PlayerUnit)
                    Heal(targetPosInt);
                else
                    ApplyDMG(targetPosInt);
            }
        }

        public void Heal(Vector3Int targetPosInt)
        {
            UnitManager.Instance.unitDictionnary[targetPosInt].unitData.stats.Hp += (int)damage;
            UnitManager.Instance.unitDictionnary[targetPosInt].unitData.stats.Hp = Mathf.Clamp(UnitManager.Instance.unitDictionnary[targetPosInt].unitData.stats.Hp, 0, UnitManager.Instance.unitDictionnary[targetPosInt].unitData.stats.maxStat);
        }
    }
}
