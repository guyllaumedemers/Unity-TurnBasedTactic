using System.Collections;
using Globals;
using ScriptableObjects.Units;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonEventTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
    public bool disableOnPointerDown;
    public bool disableTooltip;

    [Header("Required Components")]
    RectTransform mousePreview;
    RectTransform buttonRect;

    [Header("Tags")]
    readonly string mouseTag = "MouseCursor";

    [Header("Constant")]
    readonly float buttonScalingFactor = 1.2f;
    readonly float time = 100f;
    readonly float mouseCursorOffset = 0.5f;

    bool hasMouseLayoutMove;
    bool isHoovering;

    CanvasGroup buttonCanvas;
    private TextMeshProUGUI _buttonTMP;
    
    private void Awake()
    {
        InitializeRequiredComponents();
        buttonCanvas = GetComponent<CanvasGroup>();
        if (gameObject.GetComponent<Button>()?.GetComponentInChildren<TextMeshProUGUI>() != null)
            _buttonTMP = gameObject.GetComponent<Button>().GetComponentInChildren<TextMeshProUGUI>();
    }

    /// <summary>
    /// Action Buttons dont have canvas since they lerp out, spell buttons have canvas for fade animation
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buttonCanvas == null || buttonCanvas.alpha == 1)
        {
            isHoovering = true;
            AudioManager.Instance.InitializeRoutingAndPlay(AudioManager.Instance.genericClips, GV.channelID_ui, 0, GV.uiFX, false);
            Tweening.ButtonHoovering(gameObject, buttonScalingFactor, time);
            StartCoroutine(MouseAnimation(mousePreview, buttonRect));
        }
        
        if(!disableTooltip)
            ShowTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHoovering = false;
        Tweening.ButtonFocusReset(gameObject);
        
        if(!disableTooltip)
            OverlayTooltip.Instance.DisableTooltip();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        AudioManager.Instance.InitializeRoutingAndPlay(AudioManager.Instance.genericClips, GV.channelID_ui, 1, GV.uiFX, false);
        Tweening.ButtonFocusReset(gameObject);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(!disableOnPointerDown)
            Tweening.ButtonClick(gameObject, buttonScalingFactor, time);
        
        if(!disableTooltip)
            OverlayTooltip.Instance.DisableTooltip();
    }

    private void InitializeRequiredComponents()
    {
        mousePreview = GameObject.FindGameObjectWithTag(mouseTag).GetComponent<RectTransform>();
        buttonRect = gameObject.GetComponent<RectTransform>();
    }

    /// <summary>
    /// MouseAnimation goes back and forth when hoovering a button and stay still when onExit. It is important to retrieve the buttonRect position in order to Align properly
    /// and offset at half the deltaSize of the button
    /// </summary>
    /// <param name="uiMouseCursor"></param>
    /// <param name="mouseRectCanvas"></param>
    /// <param name="buttonRect"></param>
    /// <returns></returns>
    private IEnumerator MouseAnimation(RectTransform uiMouseCursor, RectTransform buttonRect)
    {
        Vector3[] corners = Utilities.WorldSpaceAnchors(buttonRect);
        uiMouseCursor.position = corners[1];
        while (isHoovering)
        {
            StartCoroutine(Tweening.MoveTab(uiMouseCursor.gameObject, true, uiMouseCursor.position.x, hasMouseLayoutMove
                ? corners[1].x : corners[1].x - mouseCursorOffset,
                GV.shortAnimationTimeAlt));
            if (!isHoovering)                       // use to instantly break the coroutine when user exit hoovering state => looks more smooth after than before the inner coroutine
                yield break;
            hasMouseLayoutMove = !hasMouseLayoutMove;
            yield return new WaitForSeconds(GV.shortAnimationTimeAlt);
        }
    }

    private void ShowTooltip()
    {
        if (_buttonTMP == null || BattleTurnManager.Instance.packageInfo.UnitSelected == null) return;
        
        var ability = _buttonTMP.text.ToLower().Equals("attack") ? 
            BattleTurnManager.Instance.packageInfo.UnitSelected.unitData.abilities[0] : 
            BattleTurnManager.Instance.packageInfo.UnitSelected.unitData.GetAbilityFromString(_buttonTMP.text);
        
        if (ability != null)
        {
            string str = $"{ability.Name}\n\n" +
                         $"{ability.GetAbilityString()}\n" +
                         "Left click to use";
            
            OverlayTooltip.Instance.EnableTooltip(1f, buttonRect, new Vector2(0f, 2f), str); 
        }
    }
}