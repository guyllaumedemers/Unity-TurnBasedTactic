using System;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System.Collections;
using System.Linq;
using Globals;
using ScriptableObjects;
using ScriptableObjects.Abilities;
using ScriptableObjects.Items;
using ScriptableObjects.Units;
using ScriptableObjects.Stats;

//The SingleNodeBlocker is used by AStar to allow a GameObject to block a PathNode
[RequireComponent(typeof(SingleNodeBlocker))]
public class Unit : MonoBehaviour
{
    [Header("Required Components")]
    public Vector3Int positionGrid;
    public Vector3Int lastpositionGrid;
    public UnitData unitData;
    public AnimationManager animationManager;
    public bool isNotAffectedByCondition;
    public bool Immunity { get; set; }
    //public delegate void MoveEvent(bool hasCompletedMove);
    //public MoveEvent moveEvent;

    [Header("AStar Components")]
    [HideInInspector] public BlockManager blockManager;
    [HideInInspector] public SingleNodeBlocker blocker;
    public GraphNode targetNode;
    public BlockManager.TraversalProvider traversalProvider;

    [HideInInspector] public SpriteRenderer spriteRenderer;

    public List<BuffDuration> buffDurations = new List<BuffDuration>();
    public Dictionary<string, int> abilityCooldowns = new Dictionary<string, int>();

    public int maxUnitHp;
    public int maxUnitAp;
    
    protected virtual void Awake()
    {
        isNotAffectedByCondition = true;

        blockManager = GameObject.Find("AStarManager").GetComponent<BlockManager>();
        blocker = GetComponent<SingleNodeBlocker>();
        traversalProvider = new BlockManager.TraversalProvider(blockManager, BlockManager.BlockMode.AllExceptSelector, new List<SingleNodeBlocker>() { blocker });
    }

    protected virtual void Start()
    {
        if ((this is AIEnemy) && (unitData as EnemyUnitData).type.Equals("Boss"))
            transform.localRotation = Quaternion.Euler(0, 180, 0);
    }

    /// <summary>
    /// Update unit information
    /// </summary>
    protected virtual void UpdateUnitInfo() { }

    /// <summary>
    /// Use buff
    /// </summary>
    /// <param name="ability"></param>
    public virtual void UseBuff(Ability ability) { }

    /// <summary>
    /// Use ability
    /// </summary>
    /// <param name="ability"></param>
    public virtual void UseAbility(Ability ability) { }

    /// <summary>
    /// Adds mods from consumable to player stats
    /// </summary>
    /// <param name="consumable">consumable item</param>
    public void UseConsumable(Consumable consumable)
    {
        if (consumable == null) return;
        ModStats(consumable.mods.stats);
    }

    public void SetSpriteOrientation(Vector3Int tileTarget)
    {
        if ((this is AIEnemy) && !(unitData as EnemyUnitData).type.Equals("Boss") || (this is PlayerUnit))
        {

            if (tileTarget.x < positionGrid.x || tileTarget.y > positionGrid.y)
            {
                transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
            else if (tileTarget.x > positionGrid.x || tileTarget.y < positionGrid.y)
            {
                transform.localRotation = Quaternion.Euler(0, 180, 0);
            }

        }
        else
        {
            if (tileTarget.x < positionGrid.x || tileTarget.y > positionGrid.y)
            {
                transform.localRotation = Quaternion.Euler(0, 180, 0);
            }
            else if (tileTarget.x > positionGrid.x || tileTarget.y < positionGrid.y)
            {
                transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
        }

    }

    /// <summary>
    /// AP Removal for the Move Action is handle by event callback inside the AStar
    /// </summary>
    /// <param name="tileTarget"></param>
    public void BeginMove(Vector3Int tileTarget)
    {
        animationManager.Animate(true, AnimationManager.moveTrigger);
        SetSpriteOrientation(tileTarget);
        lastpositionGrid = positionGrid;
    }

    public void EndMove()
    {
        StartCoroutine(UpdateUnitInDictionnary(this, lastpositionGrid));
    }

    /// <summary>
    /// Update the Entry for the Player Selected after he complete his move
    /// </summary>
    /// <param name="newUnit"></param>
    /// <param name="oldposition"></param>
    /// <param name="newposition"></param>
    private IEnumerator UpdateUnitInDictionnary(Unit newUnit, Vector3Int oldposition)
    {
        if (UnitManager.Instance.unitDictionnary.ContainsKey(oldposition))
        {
            while (!GridManager.aStar.moveCompleted)
            {
                if (GridManager.aStar.GraphNodes.Count == 0)
                    break;
                yield return null;
            }
            UnitManager.Instance.UpdateUnitEntryInDictionnary(newUnit, oldposition, positionGrid);
        }
        yield return null;
    }

    /// <summary>
    /// Has to take into account the armor of the target unit, immune state etc...
    /// </summary>
    /// <param name="amount"></param>
    public void TakeDamage(int amount)
    {
        int totalDamage = (amount - unitData.stats.Def).ZeroCheck();
        unitData.stats.Hp -= totalDamage;
        //unitData.stats.Hp = Mathf.Clamp(unitData.stats.Hp, 0, unitData.stats.maxStat);
        
        DamagePopup popup = DamagePopup.Create(totalDamage, GridManager.GetTileToCellFromWorld(positionGrid));
        popup.Animate();
        Destroy(popup.gameObject, GV.moveAnimationTime);
        
        if (unitData.stats.Hp <= 0)
        {
            UnitManager.Instance.unitDictionnary.Remove(positionGrid); 
            StartCoroutine(Die());
        }
    }

    public IEnumerator Die()
    {
        if (UnitManager.Instance.CheckUnitType<AIEnemy>(this))
        {
            InventoryManager.Instance.AddEnemyDrops(((EnemyUnitData)unitData).drops);
        }        
        
        animationManager.TriggerAnimation(AnimationManager.deathTrigger);

        yield return StartCoroutine(CheckIfDeathAnimationCompleted());

        blocker.Unblock();

        Destroy(gameObject, 3f);
    }

    private IEnumerator CheckIfDeathAnimationCompleted()
    {
        while ((animationManager.animator.GetCurrentAnimatorStateInfo(0).normalizedTime) < 1f)
            yield return null;
    }

    /// <summary>
    /// Function that is registered inside the AStar Class and invoke in the MoveAlong function to update the AP
    /// </summary>
    /// <param name="amount"></param>
    public void RemoveAP(int amount) => unitData.stats.Ap -= amount;

    /// <summary>
    /// Set the value of ActionPionts
    /// </summary>
    /// <param name="statValue"></param>
    private void ModAP(int statValue) => unitData.stats.Ap = SetApValue(unitData.stats.Ap += statValue);

    /// <summary>
    /// Set the value of Healthpoints and if zero calls die function
    /// </summary>
    /// <param name="statValue"></param>
    private void ModHp(int statValue)
    {
        unitData.stats.Hp = SetHpValue(unitData.stats.Hp + statValue);
        if (unitData.stats.Hp <= 0) unitData.stats.Hp = 1;
    }

    /// <summary>
    /// Sets the value of Defence
    /// </summary>
    /// <param name="statValue"></param>
    private void ModDef(int statValue) => unitData.stats.Def = SetStatValue(unitData.stats.Def + statValue);

    /// <summary>
    /// Sets the value of Damage
    /// </summary>
    /// <param name="statValue"></param>
    private void ModDmg(int statValue) => unitData.stats.Dmg = SetStatValue(unitData.stats.Dmg + statValue);

    /// <summary>
    /// Sets the value of ActionPoints
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private int SetApValue(int value) => value > maxUnitAp ? maxUnitAp : value.ZeroCheck();

    /// <summary>
    /// Set the value of HP
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private int SetHpValue(int value) => value > maxUnitHp ? maxUnitHp : value.ZeroCheck();
    
    /// <summary>
    /// Sets the value of other stats
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private int SetStatValue(int value) => value > unitData.stats.maxStat ? unitData.stats.maxStat : value.ZeroCheck();

    /// <summary>
    /// Adds item modifications to stats by mult value
    /// </summary>
    /// <param name="stats"></param>
    /// <param name="mult"></param>
    public void SetItemMods(List<StatValue> stats, int mult = 1)
    {
        foreach (var stat in stats)
        {
            var statMult = stat.value * mult;
            switch (stat.type)
            {
                case StatType.Ap:
                    (maxUnitAp += statMult).ZeroCheck();
                    if (unitData.stats.Ap > maxUnitAp) unitData.stats.Ap = maxUnitAp;
                    break;
                case StatType.Hp:
                    (maxUnitHp += statMult).OneCheck();
                    if (unitData.stats.Hp > maxUnitHp) unitData.stats.Hp = maxUnitHp;
                    break;
                case StatType.Dmg:
                    (unitData.stats.Dmg += statMult).ZeroCheck();
                    break;
                case StatType.Def:
                    (unitData.stats.Def += statMult).ZeroCheck();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        UpdateUnitInfo();
    }
    
    /// <summary>
    /// Mofifies StatType with multiplier
    /// </summary>
    /// <param name="stat"></param>
    /// <param name="mult"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private void ModStat(StatValue stat, int mult = 1)
    {
        var statMult = stat.value * mult;
        switch (stat.type)
        {
            case StatType.Ap:
                ModAP(statMult);
                break;
            case StatType.Hp:
                ModHp(statMult);
                break;
            case StatType.Dmg:
                ModDmg(statMult);
                break;
            case StatType.Def:
                ModDef(statMult);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        UpdateUnitInfo();
    }
    
    /// <summary>
    /// Show unit tooltip
    /// </summary>
    public void OnMouseEnter()
    {
        Vector3Int mousepos = GridManager.GetWorldToCellFromMouse(Input.mousePosition);
        if (UnitManager.Instance.unitDictionnary.ContainsKey(mousepos) && BattleTurnManager.Instance.performEvent == null)
        {
            Unit selected = UnitManager.Instance.unitDictionnary[mousepos];
            string hp = "HP";
            string str = $"{selected.unitData.Name.AddColor(Color.cyan)}\n" +
                         $"{hp.AddColor(Color.green)} : {selected.unitData.stats.Hp.ToString().AddColor(Color.red)}";
            OverlayTooltip.Instance.EnableTooltip(0.2f, selected.transform, new Vector2(1.1f, 0.3f), str);
        }
    }

    /// <summary>
    /// Hide unit tooltip
    /// </summary>
    public void OnMouseExit()
    {
        OverlayTooltip.Instance.DisableTooltip();
    }
    
    /// <summary>
    /// Mofifies List of StatType with multiplier
    /// </summary>
    /// <param name="stats">list of StatValue</param>
    /// <param name="mult">mult</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    protected void ModStats(List<StatValue> stats, int mult = 1)
    {
        foreach (var stat in stats)
            ModStat(stat, mult);
    }
}