using Globals;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisplayActiveTurn : MonoBehaviour
{
    #region SINGLETON MONOBEHAVIOUR
    private static DisplayActiveTurn instance;
    private DisplayActiveTurn() { }
    public static DisplayActiveTurn Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<DisplayActiveTurn>();
                if (instance == null)
                {
                    GameObject go = new GameObject();
                    instance = go.AddComponent<DisplayActiveTurn>();
                }
            }
            return instance;
        }
    }
    #endregion

    #region UI COMPONENTS
    [Header("RecTransform")]
    public RectTransform leftSidePanel;
    public RectTransform rightSidePanel;
    public RectTransform turnDisplayPanel;
    RectTransform playerPanel;
    RectTransform enemyPanel;

    [Header("Text Component")]
    TextMeshProUGUI textTurnDisplay;
    public TextMeshProUGUI[] dialogues;

    //Tweening
    CanvasGroup textDisplayCanvasGroup;
    Image backgroundShade;
    // Animator
    Animator dialogueAnimator;
    #endregion

    #region UI MEMBER VALUES
    [Header("UI States")]
    bool hasTabMoved;
    bool hasOpacityFaded;
    bool hasTabFaded;

    [Header("UI values")]
    const float direction = 1.0f;
    const float uiOffset = 2.0f;
    const float maxOpacity = 0.55f;
    const float minOpacity = 0f;
    #endregion

    //Properties
    public TextMeshProUGUI[] GetTextArray { get; set; }
    public int TrackTurnCount { get; set; }
    //Events
    public delegate void UIAnimationEvent();
    public UIAnimationEvent animationEvent;

    /// <summary>
    /// Load the text Information to display depending on the active turn and display the proper tab
    /// </summary>
    public IEnumerator UpdateTurnDisplay(bool activeTurn)
    {
        InputManager.Instance.canUseInputs = false;
        //InitializeUIValues();
        LoadDisplay(activeTurn);
        yield return StartCoroutine(ShowTabs(activeTurn));                              // wait for this coroutine to end before exiting the function
        InputManager.Instance.canUseInputs = true;                                      // Enable Inputs during animation
        animationEvent.Invoke();
    }

    /// <summary>
    /// Load Text according to the Active Turn
    /// </summary>
    private void LoadDisplay(bool activeTurn)
    {
        _ = (activeTurn) ? textTurnDisplay.text = GV.player : textTurnDisplay.text = GV.enemy;
    }

    /// <summary>
    /// Tab Animations
    /// </summary>
    private IEnumerator ShowTabs(bool activeTurn)
    {
        UpdateDialogueText();
        StartCoroutine(ShowBG());
        StartCoroutine(DisplayTurnText());
        yield return
            _ = (activeTurn) ?
                StartCoroutine(ShowActiveTurn(playerPanel, leftSidePanel, -direction)) :
                StartCoroutine(ShowActiveTurn(enemyPanel, rightSidePanel, direction));
    }

    #region TWEENING
    /// <summary>
    /// Move Animation for the Active panel, moving in and out of the screen
    /// </summary>
    /// <param name="panel"></param>
    /// <param name="parent"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    private IEnumerator ShowActiveTurn(RectTransform panel, RectTransform parent, float direction)
    {
        yield return StartCoroutine(Tweening.MoveTab(panel.gameObject, true, panel.position.x, hasTabMoved ?
            parent.position.x + (Mathf.Abs(Utilities.WorldSpaceAnchors(parent)[direction < 0 ? 0 : 3].x) * uiOffset * direction) : parent.position.x, GV.moveAnimationTime));
        hasTabMoved = !hasTabMoved;
        yield return new WaitForSeconds(GV.moveAnimationTime);
        yield return StartCoroutine(Tweening.MoveTab(panel.gameObject, true, panel.position.x, hasTabMoved ?
            parent.position.x + (Mathf.Abs(Utilities.WorldSpaceAnchors(parent)[direction < 0 ? 0 : 3].x) * uiOffset * direction) : parent.position.x, GV.moveAnimationTime));
        hasTabMoved = !hasTabMoved;
    }

    /// <summary>
    /// ShowBG only affect the alpha channel of the Image giving a smooth dark background to the Turn Display Panel
    /// </summary>
    /// <returns></returns>
    private IEnumerator ShowBG()
    {
        yield return StartCoroutine(Tweening.OpacityFade(backgroundShade, backgroundShade.color, backgroundShade.color.a, hasOpacityFaded ?
            maxOpacity : minOpacity, GV.fadeAnimationTime));
        hasOpacityFaded = !hasOpacityFaded;
        yield return new WaitForSeconds(GV.moveAnimationTime);
        yield return StartCoroutine(Tweening.OpacityFade(backgroundShade, backgroundShade.color, backgroundShade.color.a, hasOpacityFaded ?
            maxOpacity : minOpacity, GV.fadeAnimationTime));
        hasOpacityFaded = !hasOpacityFaded;
    }

    private IEnumerator DisplayTurnText()
    {
        yield return StartCoroutine(Tweening.FadeCanvasGroup(textDisplayCanvasGroup, textDisplayCanvasGroup.alpha, hasTabFaded ? 1 : 0, GV.fadeAnimationTime));
        hasTabFaded = !hasTabFaded;
        yield return new WaitForSeconds(GV.moveAnimationTime);
        yield return StartCoroutine(Tweening.FadeCanvasGroup(textDisplayCanvasGroup, textDisplayCanvasGroup.alpha, hasTabFaded ? 1 : 0, GV.fadeAnimationTime));
        hasTabFaded = !hasTabFaded;
    }

    private void UpdateDialogueText()
    {
        TriggerNextText();
        ChooseTextOption(OptionSelectionLogic(UnitManager.Instance.PlayerIsWinning()));
    }

    /// <summary>
    /// Trigger Function
    /// </summary>
    /// <param name="animatorIndex"></param>
    private void TriggerNextText()
    {
        if (!BossBattleDialogueManagement() && QuickDeathDialogueManagement())
            return;
        dialogueAnimator.SetTrigger(GV.trigger);
    }

    private bool QuickDeathDialogueManagement()
    {
        if (!dialogueAnimator.GetBool(GV.quickDeathTrigger) && UnitManager.Instance.GetAllUnitsType<AIEnemy>().Count < GV.minUnitOnField)
        {
            dialogueAnimator.SetTrigger(GV.skipdiagTrigger);
            if (TrackTurnCount < GV.minTurnQuickDeath)
            {
                dialogueAnimator.SetBool(GV.quickDeathTrigger, true);
                return true;
            }
        }
        return false;
    }

    private bool BossBattleDialogueManagement()
    {
        return dialogueAnimator.GetBool(GV.bossBattleTrigger);
    }

    /// <summary>
    /// doesnt need a return -1 since not all nodes have this condition. It will only be taken into account when needed
    /// </summary>
    /// <param name="isPlayerWinning"></param>
    /// <returns></returns>
    private int OptionSelectionLogic(bool isPlayerWinning)
    {
        return isPlayerWinning ? 0 : 1;
    }

    /// <summary>
    /// Choose Text Option
    /// </summary>
    /// <param name="animatorIndex"></param>
    /// <param name="value"></param>
    private void ChooseTextOption(int value)
    {
        if (value < 0)
            return;
        dialogueAnimator.SetInteger(GV.options, value);
    }

    #endregion

    #region INITIALIZATION FUNCTIONS

    public void CachingUIComponents()
    {
        //  Childs
        playerPanel = leftSidePanel.GetChild(0).GetComponent<RectTransform>();
        enemyPanel = rightSidePanel.GetChild(0).GetComponent<RectTransform>();
        // Text Components
        textTurnDisplay = turnDisplayPanel.GetComponentInChildren<TextMeshProUGUI>();
        // Dialogues
        GetTextArray = new TextMeshProUGUI[dialogues.Length];
        for (int i = 0; i < dialogues.Length; i++)
        {
            GetTextArray[i] = dialogues[i];
        }
        // Background Shade Component
        backgroundShade = leftSidePanel.parent.GetComponent<Image>();
        // Text Display Canvas Group
        textDisplayCanvasGroup = turnDisplayPanel.GetComponent<CanvasGroup>();
        // animators
        dialogueAnimator = GetComponentInChildren<Animator>();
    }

    /// <summary>
    /// Set the Panels to be outside the screen onStart
    /// </summary>
    private void InitilalizePanelsPosition()
    {
        Vector3 initializePlayerPanel = playerPanel.position;
        initializePlayerPanel.x = leftSidePanel.position.x - Mathf.Abs(Utilities.WorldSpaceAnchors(leftSidePanel)[0].x) * uiOffset; // Justify because dialogue box maxanchor.x = 1.5
        playerPanel.position = initializePlayerPanel;

        Vector3 initializeEnemyPanel = enemyPanel.position;
        initializeEnemyPanel.x = rightSidePanel.position.x + Mathf.Abs(Utilities.WorldSpaceAnchors(rightSidePanel)[3].x) * uiOffset;
        enemyPanel.position = initializeEnemyPanel;
    }

    /// <summary>
    /// Single function call that initialize every UI values for the ActiveTurn
    /// </summary>
    public void InitializeUIValues()
    {
        RegisterEvent();
        InitilalizePanelsPosition();
        dialogueAnimator.SetBool(GV.bossBattleTrigger, PlayerPrefs.GetInt(GV.bossBattleState) == 1 ? true : false);
        TrackTurnCount = 0;
        hasTabMoved = false;
        hasOpacityFaded = true;
        hasTabFaded = true;
        textDisplayCanvasGroup.alpha = 0;
        backgroundShade.color = new Color(0, 0, 0, 0);
    }

    private void RegisterEvent()
    {
        BattleTurnManager.Instance.trackingTurnEvent += () => { TrackTurnCount += 1; };
        BattleTurnManager.Instance.displayWinnerEvent += (b) => { PlayerPrefs.SetInt(GV.bossBattleState, Convert.ToInt32(b)); };
    }
    #endregion
}
