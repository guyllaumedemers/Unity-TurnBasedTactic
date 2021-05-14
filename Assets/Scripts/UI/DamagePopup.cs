using Globals;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    private TextMeshPro damageText;
    private static GameObject damagePrefab;
    private float distanceToTravel = 4;
    private Vector3 startPos;

    private void Awake()
    {
        damageText = GetComponent<TextMeshPro>();
    }

    public static DamagePopup Create(int damage, Vector3 position)
    {
        damagePrefab = Instantiate(GameAsset.Instance.damageHitInfo, position + new Vector3(0.5f, 2f, 0), Quaternion.identity);
        DamagePopup dmg = damagePrefab.GetComponent<DamagePopup>();
        dmg.Setup(damage);
        return dmg;
    }

    public void Animate()
    {
        startPos = damagePrefab.transform.position;
        StartCoroutine(Tweening.FadeTextOpacity(damageText, damageText.alpha, 0, GV.fadeAnimationTime));
        StartCoroutine(Tweening.MoveTab(damagePrefab, false, damagePrefab.transform.position.y, startPos.y - distanceToTravel, GV.moveAnimationTime));
    }

    private void Setup(int damage)
    {
        damageText.text = $"-{damage}HP";
    }
}
