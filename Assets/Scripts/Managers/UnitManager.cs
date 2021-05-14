using System.Collections.Generic;
using System.IO;
using ScriptableObjects;
using ScriptableObjects.Units;
using Sirenix.Serialization;
using UnityEngine;

public class UnitManager : Flow
{
    #region REGULAR SINGLETON
    private static UnitManager instance;
    private UnitManager() { }
    public static UnitManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new UnitManager();
            }
            return instance;
        }
    }
    #endregion

    //Magic Value on Y -- Needed to offset unit a bit so that it fits perfectly on the first tile it spawns.
    //No issues after first move, offset is not necessary to be accessed or modified by anyone, so that's why the magic value was kept that way.
    private const float SPRITE_OFFSET_MAGIC_VALUE = 0.125f;

    public Dictionary<Vector3Int, Unit> unitDictionnary;
    //private Dictionary<Vector3Int, Unit> unitPrefabs;
    private Unit genericUnitPrefab;

    public delegate void AIUnitSelectedEvent(Unit unit);
    public AIUnitSelectedEvent aiUnitSelectedEvent;

    public void PreInitialize()
    {
        genericUnitPrefab = Resources.Load<Unit>("Prefabs/Units/GenericUnit");
        unitDictionnary = new Dictionary<Vector3Int, Unit>();
    }

    public void Initialize()
    {
        RegisterEvent();
    }

    public void Refresh()
    {
    }

    /// <summary>
    /// Retrieve all the Active Units of Type T from the dictionnary and return true if the List.Count is <= 0 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public bool HasCompleteTurn<T>() where T : class
    {
        List<T> activeUnits = GetActiveUnitsFromDictionnary<T>();
        if (activeUnits is List<AIEnemy>)
            return activeUnits.Count <= 0 || CompleteAITurn();
        return activeUnits.Count <= 0;
    }

    /// <summary>
    /// Check if the Dictionnary contains Units of type T, return true if there is none
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public bool HasNoUnitLeft<T>() where T : class
    {
        List<T> units = new List<T>();
        foreach (Vector3Int pos in unitDictionnary.Keys)
        {
            if (unitDictionnary[pos] is T && unitDictionnary[pos].unitData.stats.Hp > 0)
            {
                units.Add(unitDictionnary[pos] as T);
            }
        }
        return units.Count <= 0;
    }

    public void SetTargetState(Vector3Int tileTarget)
    {
        if (!unitDictionnary.ContainsKey(tileTarget))
            return;
        unitDictionnary[tileTarget].isNotAffectedByCondition = false;
    }

    /// <summary>
    /// Reset all Units AP in dictionnary
    /// </summary>
    public void ResetAP()
    {
        if (unitDictionnary == null)
            return;
        foreach (Vector3Int pos in unitDictionnary.Keys)
        {
            unitDictionnary[pos].unitData.stats.Ap = unitDictionnary[pos].unitData.stats.maxAp;
        }
    }

    /// <summary>
    /// Update Dictionnary
    /// Remove the Unit with Old position entry, add the new Unit with the actual position and AP value
    /// </summary>
    /// <param name="newUnit"></param>
    /// <param name="oldTilePos"></param>
    /// <param name="newTilePos"></param>
    public void UpdateUnitEntryInDictionnary(Unit newUnit, Vector3Int oldTilePos, Vector3Int newTilePos)
    {
        if (unitDictionnary == null)
            return;
        unitDictionnary.Remove(oldTilePos);                 // need to be removed first, what if the unit does not move but cast a spell
        unitDictionnary.Add(newTilePos, newUnit);           // we wouldnt be able to set its new position since he never moved
    }

    /// <summary>
    /// Remove all units that have hp <= 0
    /// </summary>
    public void RemoveInactiveUnits()
    {
        if (unitDictionnary == null)
            return;
        List<Vector3Int> entriesToRemove = new List<Vector3Int>();
        foreach (Vector3Int entry in unitDictionnary.Keys)
        {
            if (unitDictionnary[entry].unitData.stats.Hp <= 0)
                entriesToRemove.Add(entry);
        }
        foreach (Vector3Int key in entriesToRemove)
        {
            unitDictionnary.Remove(key);
        }
    }

    /// <summary>
    /// Select the Active Units from the Dictionnary. Need to specify its type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public List<T> GetActiveUnitsFromDictionnary<T>() where T : class
    {
        if (unitDictionnary == null)
            return null;
        List<T> units = new List<T>();
        foreach (Vector3Int pos in unitDictionnary.Keys)
        {
            if (unitDictionnary[pos] is T && unitDictionnary[pos].unitData.stats.Ap > 0)
            {
                units.Add(unitDictionnary[pos] as T);
            }
        }
        return units;
    }

    /// <summary>
    /// Check the Unit Type and return true if the Type T passed is equals to it
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="unit"></param>
    /// <returns></returns>
    public bool CheckUnitType<T>(Unit unit) where T : class
    {
        return unit.GetType().Equals(typeof(T));
    }

    /// <summary>
    /// Get All Units according to the Type giving
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public List<T> GetAllUnitsType<T>() where T : class
    {
        if (unitDictionnary == null)
            return null;
        List<T> units = new List<T>();
        foreach (Vector3Int pos in unitDictionnary.Keys)
        {
            if (unitDictionnary[pos] is T)
            {
                units.Add(unitDictionnary[pos] as T);
            }
        }
        return units;
    }

    public bool PlayerIsWinning()
    {
        if (GetAllUnitsType<AIEnemy>().Count > GetAllUnitsType<PlayerUnit>().Count)
            return false;
        return true;
    }

    public void SelectWhichEnemyAIGoesFirst()
    {
        List<AIEnemy> activeEnemies = GetActiveUnitsFromDictionnary<AIEnemy>();
        if (activeEnemies.Count <= 0)
            return;
        AIEnemy unitWithMaxHp = activeEnemies[0];
        foreach (AIEnemy enemy in activeEnemies)
        {
            if (!enemy.isNotAffectedByCondition)
                continue;
            if (Globals.GV.PONDERATION * (unitWithMaxHp.unitData.stats.Hp) + (1 - Globals.GV.PONDERATION) * (unitWithMaxHp.unitData.stats.Ap) <
                Globals.GV.PONDERATION * (enemy.unitData.stats.Hp) + (1 - Globals.GV.PONDERATION) * (enemy.unitData.stats.Ap))
            {
                unitWithMaxHp = enemy;
            }
            //check own state and  Hp and which Enemy Player can do the most damage
        }
        unitWithMaxHp.IsItMyTurn = true;
        aiUnitSelectedEvent.Invoke(unitWithMaxHp);
    }

    bool CompleteAITurn()
    {
        bool completeTurn = true;
        foreach (var key in unitDictionnary.Keys)
        {
            if (unitDictionnary[key] is AIEnemy)
            {
                if (!(unitDictionnary[key] as AIEnemy).wantToCompleteTurn)
                {
                    completeTurn = false;
                }
            }
        }
        List<AIEnemy> activeUnits = GetActiveUnitsFromDictionnary<AIEnemy>();

        if (completeTurn || activeUnits.Count <= 0)
        {
            foreach (var unit in unitDictionnary.Values)
            {
                if (unit is AIEnemy)
                {
                    (unit as AIEnemy).wantToCompleteTurn = false;
                }
            }
        }
        return completeTurn;
    }

    /// <summary>
    /// This function create units according to the UnitData that is passed.
    /// 
    /// Every unit is built using the "GenericUnit" prefab.
    /// </summary>
    /// <param name="unitData"></param>
    /// <returns></returns>
    public Unit CreateUnit(UnitData unitData)
    {
        Unit genericUnitClone = GameObject.Instantiate(genericUnitPrefab);
        UnitData unitDataClone = unitData.Clone();

        if (unitDataClone.GetType() == typeof(EnemyUnitData))
        {
            genericUnitClone.positionGrid = GridManager.GetRandomEnemySpawnPosition();
        }
        else
        {
            Component.Destroy(genericUnitClone.GetComponent<Panda.BehaviourTree>());
            Component.Destroy(genericUnitClone.GetComponent<AIEnemy>());
            genericUnitClone = genericUnitClone.gameObject.AddComponent<PlayerUnit>();
            genericUnitClone.positionGrid = GridManager.GetRandomPlayerSpawnPosition();
        }

        genericUnitClone.unitData = unitDataClone;
        genericUnitClone.spriteRenderer = genericUnitClone.GetComponent<SpriteRenderer>();

        genericUnitClone.animationManager = genericUnitClone.gameObject.AddComponent<AnimationManager>();
        genericUnitClone.animationManager.animator = genericUnitClone.gameObject.AddComponent<Animator>();
        genericUnitClone.animationManager.animator.runtimeAnimatorController = unitDataClone.AnimatorController;

        Vector3 spawnPosition = GridManager.GetTileToCellFromWorld(genericUnitClone.positionGrid);

        genericUnitClone.transform.position = new Vector3(spawnPosition.x, spawnPosition.y - SPRITE_OFFSET_MAGIC_VALUE, 0);

        genericUnitClone.gameObject.name = genericUnitClone.unitData.Name;

        genericUnitClone.blocker.BlockAtCurrentPosition();

        unitDictionnary.Add(genericUnitClone.positionGrid, genericUnitClone);

        return genericUnitClone;
    }

    public void RegisterEvent()
    {
        BattleTurnManager.Instance.enemySelectedEvent += SelectWhichEnemyAIGoesFirst;
    }
}

