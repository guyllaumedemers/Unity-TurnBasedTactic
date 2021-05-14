using System;
using UnityEngine;
using System.Reflection;
using ScriptableObjects.Abilities;
using ScriptableObjects.Units;

public class ActionMenu : MonoBehaviour
{
    #region SINGLETON MONOBEHAVIOUR
    private static ActionMenu instance;
    private ActionMenu() { }
    public static ActionMenu Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ActionMenu>(true);
                if (instance == null)
                {
                    Debug.LogError("Warning, ActionMenu singleton could not find ActionMenu in scene");
                    GameObject go = new GameObject();
                    instance = go.AddComponent<ActionMenu>();
                }
            }
            return instance;
        }
    }
    #endregion

    public delegate void ActionSelectedEvent(MethodInfo methodInfo, int index);
    public ActionSelectedEvent actionSelectedEvent;

    public delegate void MoveSelectedEvent();
    public MoveSelectedEvent moveSelectedEvent;

    OverlayProperties overlay;

    /// <summary>
    /// We need to add a distance check to prevent the player from attacking when outside its range
    /// </summary>
    public void AttackButton()
    {
        Unit unit = BattleTurnManager.Instance.packageInfo.UnitSelected;
        Tuple<OverlayProperties, Ability> tuple = GetOverlay(overlay, unit, unit.unitData.abilities[0], false);
        GridManager.DrawRangeOverlay(OverlayType.Cross, tuple.Item2.range, unit.positionGrid, tuple.Item1);
        InvokeSelectedEvent("Attack", default);
    }

    public void CastSpellButton(int index)
    {
        Unit unit = BattleTurnManager.Instance.packageInfo.UnitSelected;
        if (unit == null) return;
        
        Tuple<OverlayProperties, Ability> tuple = GetOverlay(overlay, unit, unit.unitData.abilities[index], false); // how to i know which one is green

        switch (tuple.Item2)
        {
            case AoE aoe:
                GridManager.DrawRealtimeOverlay(OverlayType.Diamond, aoe.area, tuple.Item1);
                break;
            case AoEHeal heal:
                GridManager.DrawRealtimeOverlay(OverlayType.Diamond, heal.area, tuple.Item1);
                break;
            case Buff buff:
                GridManager.DrawRangeOverlay(OverlayType.Self, buff.range, unit.positionGrid, tuple.Item1);
                break;
            default:
                GridManager.DrawRangeOverlay(OverlayType.Cross, tuple.Item2.range, unit.positionGrid, tuple.Item1);
                break;
        }
        InvokeSelectedEvent("CastSpell", index);
    }

    public void MoveButton()
    {
        GridManager.ShowPathfinding();
        moveSelectedEvent.Invoke();
        InvokeSelectedEvent("Move", default);
    }

    private void InvokeSelectedEvent(string name, int index)
    {
        actionSelectedEvent?.Invoke(typeof(ActionSystem).GetMethod(name), index);
    }

    private Tuple<OverlayProperties, Ability> GetOverlay(OverlayProperties properties, Unit unit, Ability ab, bool ability)
    {
        ability = false;
        if (unit.unitData.stats.Ap >= ab.apCost)
            ability = true;
        properties = SetProperties(properties, ability, ab.canCastSelfOnly, ab.canCastOnFriendlies, ab.canDamageFriendlies);
        return Tuple.Create(properties, ab);
    }

    private OverlayProperties SetProperties(OverlayProperties properties, bool ability, params bool[] conditions)
    {
        properties.canUseAbility = ability;
        properties.canSelfCastOnly = conditions[0];
        properties.canCastOnFriendlies = conditions[1];
        properties.canDamageFriendlies = conditions[2];
        return properties;
    }
}
