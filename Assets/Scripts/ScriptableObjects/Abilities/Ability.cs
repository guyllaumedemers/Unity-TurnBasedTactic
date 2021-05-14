using Globals;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ScriptableObjects.Abilities
{
    public class Ability : SerializedScriptableObject
    {
        [BoxGroup("Info"), HorizontalGroup("Info/Split", 55, LabelWidth = 70)]
        [HideLabel, PreviewField(55, ObjectFieldAlignment.Left)]
        public Texture icon;

        [PreviewField(45, ObjectFieldAlignment.Left), VerticalGroup("Info/Split/Right")]
        public Sprite sprite;

        [VerticalGroup("Info/Split/Right")]
        public string Name;

        [BoxGroup("Variables")]
        [PropertyRange(1, GV.MAXAp)]
        public int apCost = 1;

        [BoxGroup("Variables")]
        [PropertyRange(0, GV.MAXMultiplier)]
        public float multiplier;

        [BoxGroup("Variables")]
        [PropertyRange(0, GV.MAXDmg)]
        public float damage;

        [BoxGroup("Variables")]
        [PropertyRange(0, GV.MAXRange)]
        public int range;

        [BoxGroup("Variables")]
        [PropertyRange(0, GV.MAXDuration)]
        public int duration;

        [BoxGroup("Variables")]
        [PropertyRange(0, GV.MAXDuration)]
        public int cooldown;

        [DisableIf("canCastOnFriendlies")]
        [BoxGroup("Variables")]
        public bool canCastSelfOnly = false;

        [DisableIf("canCastSelfOnly")]
        [BoxGroup("Variables")]
        public bool canCastOnFriendlies = false;

        // This bool has no effect in the spell logic itself, if "canCastOnFriendlies" is enabled, it will damage/heal according to the spell itself
        // This is used only for the overlay to show the "warning" tile if the spell is going to damage an ally
        [DisableIf("canCastSelfOnly")]
        [EnableIf("canCastOnFriendlies")]
        [BoxGroup("Variables")]
        public bool canDamageFriendlies = false;

        [BoxGroup("Variables")]
        public GameObject abilityEffect;

        protected bool canUseAbility = false;

        #region UNIT MANAGEMENT
        [BoxGroup("Variables")]
        public AudioClip clip;

        [BoxGroup("Variables")]
        public AnimationManager animationManager;
        #endregion
        
        public virtual string GetAbilityString()
        {
            string str = "";

            if (apCost > 0) str += $"ApCost: {apCost}".AddColor(Color.cyan) + "\n";
            if (damage > 0) str += $"Dmg: {damage}".AddColor(Color.red) + "\n";
            if (range > 0) str += $"Range: {range}".AddColor(Color.green) + "\n";
            if (duration > 0) str += $"Duration: {duration}\n";
            if (cooldown > 0) str += $"Cooldown: {cooldown}\n";

            return str;
        }

        #region SHARED
        public void ApplyDMG(Vector3Int targetPosInt, int attackerDmg = 1)
        {
            if (UnitManager.Instance.unitDictionnary.ContainsKey(targetPosInt))
            {
                int totalDamage = ((int)damage + attackerDmg).ZeroCheck();
                UnitManager.Instance.unitDictionnary[targetPosInt].TakeDamage(totalDamage);
            }
        }

        /// <summary>
        /// Use Ability Trigger the AudioClip, Animator VFX and remove the AP from the player
        /// </summary>
        /// <param name="host"></param>
        /// <param name="targetPosInt"></param>
        public virtual void UseAbility(Unit host, Vector3Int targetPosInt)
        {
            canUseAbility = CheckIfAbilityCanBeUsed(host, targetPosInt);

            if (canUseAbility || host is AIEnemy)
            {
                host.RemoveAP(apCost);
                host.UseAbility(this);

                if (AudioManager.Instance.sources[0] != null && clip != null)
                    InitializeRoutingAndPlay();

                host.SetSpriteOrientation(targetPosInt);
            }
        }
        #endregion

        protected virtual bool CheckIfAbilityCanBeUsed(Unit host, Vector3Int targetPosInt)
        {
            Unit target = null;

            if (UnitManager.Instance.unitDictionnary.ContainsKey(targetPosInt))
                target = UnitManager.Instance.unitDictionnary[targetPosInt];

            if (target != null && host.unitData.stats.Ap >= apCost && target.unitData.stats.Hp > 0)
            {
                if (canCastSelfOnly && UnitManager.Instance.unitDictionnary[targetPosInt].unitData == host.unitData)
                    return true;
                else if (canCastOnFriendlies)
                    return true;
                else if (!canCastOnFriendlies && UnitManager.Instance.unitDictionnary[targetPosInt].unitData.GetType() != host.GetType())
                    return true;
            }
            else if (host.unitData.stats.Ap < apCost && !UnitManager.Instance.CheckUnitType<AIEnemy>(host))
            {
                DisplayMessageBox.Instance.DisplayMessageBoxFunc();
            }
            return false;
        }

        /// <summary>
        /// Set the animation requirements and Trigger the VFX according to the spell name. Animation must have the same name as the Ability
        /// </summary>
        public void SetAnimationClip(Unit host, string name)
        {
            host.animationManager.TriggerAnimation(name);
        }

        /// <summary>
        /// Play Sound
        /// </summary>
        public void InitializeRoutingAndPlay()
        {
            AudioManager.Instance.InitializeSettingsAndRouting(AudioManager.Instance.sources[GV.channelID_sfx], clip, GV.actionFX, false, false);
            AudioManager.Instance.sources[GV.channelID_sfx].Play();
        }
    }
}