using Globals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DisplayWinOrLose : MonoBehaviour
{
    #region SINGLETON MONOBEHAVIOUR
    private static DisplayWinOrLose instance;
    private DisplayWinOrLose() { }
    public static DisplayWinOrLose Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<DisplayWinOrLose>();
                if (instance == null)
                {
                    GameObject go = new GameObject();
                    instance = go.AddComponent<DisplayWinOrLose>();
                }
            }
            return instance;
        }
    }
    #endregion

    [Header("Required Components")]
    public GameObject[] panels;
    CanvasGroup canvasGroup;
    bool hasFade;

    private void RegisterEvent()
    {
        BattleTurnManager.Instance.displayWinnerEvent += DisplayUI;
    }

    public void CachingUIComponents()
    {
        canvasGroup = GetComponentInChildren<CanvasGroup>();
        //panels = GameObject.FindGameObjectsWithTag(GV.winloseTag);
    }

    public void InitializeUIValues()
    {
        RegisterEvent();
        hasFade = false;
        canvasGroup.alpha = 0;
        foreach (GameObject go in panels)
        {
            go.SetActive(false);
        }
    }

    public void DisplayUI(bool playerWin)
    {
        panels[playerWin ? 0 : 1].SetActive(true);
        StartCoroutine(Tweening.FadeCanvasGroup(canvasGroup, canvasGroup.alpha, hasFade ? 0 : 1, GV.fadeAnimationTime));
        hasFade = !hasFade;

        StartCoroutine(SceneLoader.Instance.LoadNextScene(playerWin));
    }
}
