using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AnimationManager : MonoBehaviour
{
    [SerializeField][Header("Required Components")]
    public Animator animator;

    [HideInInspector] public const string idleTrigger = "Idle";
    [HideInInspector] public const string attackTrigger = "Attack";
    [HideInInspector] public const string spellTrigger = "CastSpell";
    [HideInInspector] public const string moveTrigger = "Move";
    [HideInInspector] public const string deathTrigger = "Death";
    [HideInInspector] public const string aoeTrigger = "AOE";
    // Boss Only
    [HideInInspector] public const string summonTrigger = "Summon";
    [HideInInspector] public const string summonAppearTrigger = "SummonAppear";
    [HideInInspector] public const string summonIdleTrigger = "SummonIdle";

    public AnimationManager(Animator animator)
    {
        this.animator = animator;
    }

    /// <summary>
    /// can Animate ANY animation that is boolean base and return its bool status in case we want to store the active state of the animation 
    /// </summary>
    /// <param name="status"></param>
    /// <param name="animation"></param>
    /// <returns></returns>
    public bool Animate(bool status, string animation)
    {
        animator.SetBool(animation, status);
        return status;
    }

    public void TriggerAnimation(string name)
    {
        animator.SetTrigger(name);
    }
}
