using Globals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIComponentEndTurn : MonoBehaviour
{
    #region SINGLETON MONOBEHAVIOUR
    private static UIComponentEndTurn instance;
    private UIComponentEndTurn() { }
    public static UIComponentEndTurn Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<UIComponentEndTurn>();
                if (instance == null)
                {
                    GameObject go = new GameObject();
                    instance = go.AddComponent<UIComponentEndTurn>();
                }
            }
            return instance;
        }
    }
    #endregion

    [Header("Required Components")]
    RectTransform buttonPanel;
    bool hasUIScaled;

    public void InitializeUIValues()
    {
        RegisterEvent();
        hasUIScaled = false;
        buttonPanel.localScale = Vector3.zero;
    }

    public void CachingUIComponents()
    {
        buttonPanel = GameObject.FindGameObjectWithTag(GV.buttonEndTurnTag).GetComponent<RectTransform>();
    }

    private void RegisterEvent()
    {
        BattleTurnManager.Instance.turnEvent += DisplayButton;
    }

    public void DisplayButton()
    {
        StartCoroutine(Tweening.ScaleTab(buttonPanel.gameObject, buttonPanel.localScale.x, hasUIScaled ? 0 : 1, GV.shortAnimationTime));
        hasUIScaled = !hasUIScaled;
    }
}
