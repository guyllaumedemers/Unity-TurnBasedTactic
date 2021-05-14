using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ScriptableObjects.Units;
using Globals;

public class PlayerHUD : MonoBehaviour
{
    #region SINGLETON MONOBEHAVIOUR
    private static PlayerHUD instance;
    private PlayerHUD() { }
    public static PlayerHUD Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<PlayerHUD>();
                if (instance == null)
                {
                    GameObject go = new GameObject();
                    instance = go.AddComponent<PlayerHUD>();
                }
            }
            return instance;
        }
    }
    #endregion

    #region MEMBER VALUES
    #region UI COMPONENTS
    [Header("Requiered Components PlayerHUD")]
    RectTransform bottomLeftPanel;
    RectTransform bottomRightPanel;
    RectTransform topLeftPanel;
    RectTransform spellSelectionRect;
    RectTransform mouseRect;
    RectTransform buttonsRectParent;
    RectTransform characterRect;
    RectTransform actionRect;
    RectTransform inventoryRect;
    Button[] buttons;
    #endregion

    //TWEENING
    CanvasGroup[] canvasGroupSpellAtIndex;
    CanvasGroup canvasGroupMouse;

    #region PLAYER HUD VALUES
    [Header("Unit Informations")]
    Image characterIcon;
    TextMeshProUGUI characterName;
    TextMeshProUGUI characterStats_hp;
    TextMeshProUGUI characterStats_ap;
    Slider characterStatsHPSlider;
    Slider characterStatsAPSlider;
    #endregion

    #region UI STATE VALUES
    [Header("UI Tweening Behaviour")]
    bool hasButtonHUDLayoutMove;
    bool hasHUDLayoutMove;
    bool hasInventoryLayoutScale;
    bool hasFXTransitionTerminate;
    #endregion
    #endregion

    #region LOAD PLAYER INFORMATIONS
    /// <summary>
    /// Display Unit info loaded from the selected player 
    /// </summary>
    /// <param name="playerUnit"></param>
    private void DisplayUnitInfo(PlayerUnit playerUnit)
    {
        ///// set sliders max values
        characterStatsHPSlider.maxValue =  playerUnit.maxUnitHp;
        characterStatsAPSlider.maxValue = playerUnit.maxUnitAp;
        ///// set character icon
        characterIcon.sprite = playerUnit.unitData.sprite;
        ///// set aspect ratio of image
        characterIcon.preserveAspect = true;
        ///// set character name
        characterName.text = playerUnit.unitData.Name;
        ///// set character stats
        characterStats_hp.text = playerUnit.unitData.stats.Hp + $"/{playerUnit.maxUnitHp}";
        characterStats_ap.text = playerUnit.unitData.stats.Ap + $"/{playerUnit.maxUnitAp}";
        ///// set slider values
        characterStatsHPSlider.value = playerUnit.unitData.stats.Hp;
        characterStatsAPSlider.value = playerUnit.unitData.stats.Ap;
        SwitchCanvasGroupInteractable(false);
    }

    /// <summary>
    /// Spell size is static. First index is base attack so we do not need to load it. Keep i = 1
    /// </summary>
    /// <param name="playerUnit"></param>
    private void UpdateSpellList(PlayerUnit playerUnit)
    {
        for (int i = 1; i < playerUnit.unitData.abilities.Length; i++)
        {
            Button btn = buttons[i - 1];
            string spellName = playerUnit.unitData.abilities[i].Name;

            //var spell = playerUnit.abilityCooldowns.FirstOrDefault(ab => ab.name.Equals(playerUnit.unitData.abilities[i].Name));
            //if (spell != null)
            if(playerUnit.abilityCooldowns.ContainsKey(playerUnit.unitData.abilities[i].Name))
            {
                spellName += $" ({playerUnit.abilityCooldowns[playerUnit.unitData.abilities[i].Name]})";
                DisableButton(btn);
            }
            else if (playerUnit.unitData.stats.Ap < playerUnit.unitData.abilities[i].apCost)
                DisableButton(btn);
            else
                EnableButton(btn);

            btn.GetComponentInChildren<TextMeshProUGUI>().text = spellName;
        }
    }

    private void EnableButton(Button button)
    {
        button.interactable = true;
        button.GetComponent<ButtonEventTrigger>().disableOnPointerDown = false;
    }

    private void DisableButton(Button button)
    {
        button.interactable = false;
        button.GetComponent<ButtonEventTrigger>().disableOnPointerDown = true;
    }

    /// <summary>
    /// Updating the HUD info from the player by calling again the displayUnitInfo
    /// ONLY for clarity use
    /// </summary>
    /// <param name="playerUnit"></param>
    public void UpdateUnitInfo(PlayerUnit playerUnit)
    {
        DisplayUnitInfo(playerUnit);
        UpdateSpellList(playerUnit);
    }
    #endregion

    #region TWEENING

    public void HUDInteraction()
    {
        StartCoroutine(Tweening.MoveTab(characterRect.gameObject, true, characterRect.position.x, hasHUDLayoutMove ?
            bottomLeftPanel.position.x - Mathf.Abs(Utilities.WorldSpaceAnchors(bottomLeftPanel)[0].x) : bottomLeftPanel.position.x, GV.shortAnimationTime));
        StartCoroutine(Tweening.MoveTab(actionRect.gameObject, false, actionRect.position.y, hasHUDLayoutMove ?
            bottomRightPanel.position.y - Mathf.Abs(Utilities.WorldSpaceAnchors(bottomRightPanel)[0].y) : bottomRightPanel.position.y, GV.shortAnimationTime));
        /// this condition is only valid when the spell menu is open and the player press escape
        if (canvasGroupSpellAtIndex[0].alpha >= 1 && !hasFXTransitionTerminate)
        {
            StartCoroutine(StairCaseEffect(canvasGroupSpellAtIndex, buttons, buttonsRectParent));
        }
        /// handle the hiding of the inventory when it is active and we press escape
        if (inventoryRect.localScale.x == 1)
        {
            InventoryTabInteraction(inventoryRect.gameObject);
        }
        /// handle the hiding of the message box
        if (InventoryFullDlg.Instance.gameObject.activeSelf || DestroyItemDlg.Instance.gameObject.activeSelf)
        {
            InventoryFullDlg.Instance.gameObject.SetActive(false);
            DestroyItemDlg.Instance.gameObject.SetActive(false);
        }
        StartCoroutine(Tweening.FadeCanvasGroup(canvasGroupMouse, canvasGroupMouse.alpha, hasHUDLayoutMove ? 0 : 1, GV.shortAnimationTime));
        hasHUDLayoutMove = !hasHUDLayoutMove;
    }

    /// <summary>
    /// SpellTab cannot have more than 1 param as it is called from a button Event
    /// </summary>
    /// <param name="uiTab"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    public void SpellTabInteraction()
    {
        SwitchCanvasGroupInteractable(!canvasGroupSpellAtIndex[0].interactable);
        if (!hasFXTransitionTerminate)
        {
            StartCoroutine(StairCaseEffect(canvasGroupSpellAtIndex, buttons, buttonsRectParent));
        }
    }

    private void SwitchCanvasGroupInteractable(bool value)
    {
        foreach (CanvasGroup group in canvasGroupSpellAtIndex)
        {
            group.interactable = value;
        }
    }

    private IEnumerator StairCaseEffect(CanvasGroup[] canvasGroup, Button[] buttons, RectTransform buttonsRectParent)
    {
        hasFXTransitionTerminate = true;
        int index = 0;
        while (index < buttons.Length)
        {
            StartCoroutine(Tweening.FadeCanvasGroup(canvasGroup[index], canvasGroup[index].alpha, hasButtonHUDLayoutMove ? 0 : 1, GV.shortAnimationTime));
            StartCoroutine(Tweening.MoveTab(buttons[index].gameObject, true, buttons[index].transform.position.x, hasButtonHUDLayoutMove
                ? buttonsRectParent.position.x - Utilities.WorldSpaceAnchors(buttonsRectParent)[0].x : buttonsRectParent.position.x, GV.shortAnimationTime));
            ++index;
            yield return new WaitForSeconds(GV.shortAnimationTime);
        }
        hasButtonHUDLayoutMove = !hasButtonHUDLayoutMove;
        hasFXTransitionTerminate = !hasFXTransitionTerminate;
    }

    /// <summary>
    /// InventoryTab is scaling from 0 to 1
    /// </summary>
    /// <param name="uiTab"></param>
    public void InventoryTabInteraction(GameObject uiTabShow)
    {
        StartCoroutine(Tweening.ScaleTab(uiTabShow, uiTabShow.transform.localScale.x, hasInventoryLayoutScale ? 0 : 1, GV.shortAnimationTime));
        hasInventoryLayoutScale = !hasInventoryLayoutScale;
    }
    #endregion

    #region INITIALIZATION FUNCTIONS

    public void CachingUIComponents()
    {
        RetrieveTagComponents();
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        bottomLeftPanel = GameObject.FindGameObjectWithTag(GV.bottomLeftPanelTag).GetComponent<RectTransform>();
        bottomRightPanel = GameObject.FindGameObjectWithTag(GV.bottomRightPanelTag).GetComponent<RectTransform>();
        topLeftPanel = GameObject.FindGameObjectWithTag(GV.topLeftPanelTag).GetComponent<RectTransform>();
        spellSelectionRect = GameObject.FindGameObjectWithTag(GV.spellMenuTag).GetComponent<RectTransform>();
        mouseRect = GameObject.FindGameObjectWithTag(GV.mousePanelTag).GetComponent<RectTransform>();
        // Main Panels
        actionRect = bottomRightPanel.GetChild(0).GetComponent<RectTransform>();
        characterRect = bottomLeftPanel.GetChild(0).GetComponent<RectTransform>();
        // Sub Panels
        buttonsRectParent = spellSelectionRect.GetComponent<RectTransform>();
        buttons = spellSelectionRect.GetComponentsInChildren<Button>();
        // Alpha panels
        canvasGroupSpellAtIndex = spellSelectionRect.GetComponentsInChildren<CanvasGroup>();
        canvasGroupMouse = mouseRect.GetComponent<CanvasGroup>();
        // inventory
        inventoryRect = topLeftPanel.GetChild(0).GetComponent<RectTransform>();
    }

    private void RetrieveTagComponents()
    {
        characterIcon = GameObject.FindGameObjectWithTag(GV.characterIconTag).GetComponent<Image>();
        characterName = GameObject.FindGameObjectWithTag(GV.characterNameTag).GetComponent<TextMeshProUGUI>();
        characterStats_hp = GameObject.FindGameObjectWithTag(GV.characterStatsHPTag).GetComponent<TextMeshProUGUI>();
        characterStats_ap = GameObject.FindGameObjectWithTag(GV.characterStatsAPTag).GetComponent<TextMeshProUGUI>();
        characterStatsHPSlider = GameObject.FindGameObjectWithTag(GV.characterStatsHPSliderTag).GetComponent<Slider>();
        characterStatsAPSlider = GameObject.FindGameObjectWithTag(GV.characterStatsAPSliderTag).GetComponent<Slider>();
    }

    private void InitializeAllBools()
    {
        hasButtonHUDLayoutMove = false;
        hasHUDLayoutMove = false;
        hasInventoryLayoutScale = false;
        hasFXTransitionTerminate = false;
    }

    public void InitializeHUD()
    {
        RegisterEvent();
        InitializeAllBools();
        SetCanvasAlphaChannel();            // initalize canvas values to alpha 0
        SetCanvasInitialPosition();         // initialize canvas position to be outside the screen
    }

    private void SetCanvasAlphaChannel()
    {
        // Set Mouse Cursor canvas alpha 0
        canvasGroupMouse.alpha = 0;
        // Set Alpha to 0 in order to fade in
        foreach (CanvasGroup c in canvasGroupSpellAtIndex)
        {
            c.alpha = 0;
        }
    }

    private void SetCanvasInitialPosition()
    {
        Vector3 initialCharacterRect = characterRect.position;
        initialCharacterRect.x = bottomLeftPanel.position.x - Mathf.Abs(Utilities.WorldSpaceAnchors(bottomLeftPanel)[0].x);
        characterRect.position = initialCharacterRect;

        Vector3 initialActionRect = actionRect.position;
        initialActionRect.y = bottomRightPanel.position.y - Mathf.Abs(Utilities.WorldSpaceAnchors(bottomRightPanel)[0].y);
        actionRect.position = initialActionRect;

        // Delta Translate the Initial Button position in order to have to proper starting position for the Staircase Animation
        foreach (Button button in buttons)
        {
            button.transform.position = new Vector3(button.transform.position.x - Mathf.Abs(Utilities.WorldSpaceAnchors(buttonsRectParent)[0].x),
                button.transform.position.y, button.transform.position.z);
        }

        inventoryRect.localScale = Vector3.zero;
    }

    private void RegisterEvent()
    {
        BattleTurnManager.Instance.turnEvent += HUDInteraction;
    }
    #endregion
}