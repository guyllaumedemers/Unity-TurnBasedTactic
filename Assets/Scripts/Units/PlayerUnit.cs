using System.Linq;
using ScriptableObjects.Abilities;
using ScriptableObjects.Units;
using UnityEngine;

public class PlayerUnit : Unit
{
    private new void Start()
    {
        InitializeSpriteOrientation();
        BattleTurnManager.Instance.turnEvent += OnTurnCompleted;
        
        // Apply Equipment buffs
        if (((PlayerUnitData) unitData).equipment.mainHand != null)
            SetItemMods(((PlayerUnitData) unitData).equipment.mainHand.mods.stats);
        if((((PlayerUnitData) unitData).equipment.body != null))
            SetItemMods(((PlayerUnitData) unitData).equipment.body.mods.stats);
        
        maxUnitHp = unitData.stats.maxStat;
        maxUnitAp = unitData.stats.maxAp;
    }

    /// <summary>
    /// Set the orientation of the sprite onStart
    /// </summary>
    private void InitializeSpriteOrientation()
    {
        transform.localRotation = Quaternion.Euler(0, 180, 0);
    }
    
    /// <summary>
    /// Invoked when a turn is completed
    /// </summary>
    private void OnTurnCompleted()
    {
        foreach (var bd in buffDurations.ToList())
        {
            bd.duration--;
            if (bd.duration <= 0)
            {
                ModStats(bd.mods, -1);
                buffDurations.Remove(bd);
            }
        }

        foreach (var kp in abilityCooldowns.ToList())
        {
            abilityCooldowns[kp.Key]--;
            if (abilityCooldowns[kp.Key] <= 0)
                abilityCooldowns.Remove(kp.Key);
        }
    }

    protected override void UpdateUnitInfo()
    {
        PlayerHUD.Instance.UpdateUnitInfo(this);
    }
    
    /// <summary>
    /// Add ability cooldown to unit ability cooldown dictionary
    /// </summary>
    /// <param name="ability"></param>
    public override void UseAbility(Ability ability)
    {
        if(ability.cooldown > 0 && !abilityCooldowns.ContainsKey(ability.Name))
            abilityCooldowns.Add(ability.Name, ability.cooldown);
    }
    
    /// <summary>
    /// Add buff duration and ability cooldown to unit dictionary and applies buff modifications
    /// </summary>
    /// <param name="ability"></param>
    public override void UseBuff(Ability ability)
    {
        if (buffDurations.Count(ab => ab.name == ability.Name) <= 0)
        {
            buffDurations.Add(new BuffDuration(ability.Name, ability.duration,((Buff)ability).mods.stats));
            ModStats(((Buff)ability).mods.stats);
        }
    }
}
