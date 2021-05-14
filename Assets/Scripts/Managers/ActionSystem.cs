using System.Collections.Generic;
using ScriptableObjects.Items;
using ScriptableObjects.Stats;
using UnityEngine;

public static class ActionSystem
{
    /// <summary>
    /// INDEX MUST BE ZERO. BE SURE TO SET THE FIRST ABILITY OF EVERY UNIT IN THE SCRIPTABLE TO BE THE BASIC ATTACK
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="index"></param>
    /// <param name="tileTarget"></param>
    public static void Attack(Unit unit, int index, Vector3Int tileTarget)
    {
        if (GridManager.GetUnitAtTile(tileTarget) != null && UnitManager.Instance.unitDictionnary[tileTarget] is AIEnemy)
            unit.unitData.abilities[0].UseAbility(unit, tileTarget);
    }

    public static void CastSpell(Unit unit, int index, Vector3Int tileTarget)
    {
        unit.unitData.abilities[index].UseAbility(unit, tileTarget);
    }

    public static void UseConsumable(Unit unit, Consumable consumable, Vector3Int tileTarget)
    {
        unit.UseConsumable(consumable);
    }

    public static void Move(Unit unit, int index, Vector3Int tileTarget)
    {
        unit.BeginMove(tileTarget);
        unit.EndMove();
    }
}