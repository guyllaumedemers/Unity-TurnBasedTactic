using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class OverlayTooltip : MonoBehaviour
{
    [ShowInInspector] private float _textPadding = 15f;
    [ShowInInspector] private float _offsetX;
    [ShowInInspector] private float _offsetY;

    public TextMeshProUGUI textGUI;
    private RectTransform _rect;
    
    #region SINGLETON MONOBEHAVIOUR
    private static OverlayTooltip _instance;
    
    private OverlayTooltip() { }
    
    public static OverlayTooltip Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<OverlayTooltip>();
                if (_instance == null)
                {
                    //GameObject go = new GameObject();
                    //instance = go.AddComponent<OverlayTooltip>();
                }
            }

            return _instance;
        }
    }

    #endregion
    
    public void Initialize()
    {
        gameObject.SetActive(false);
        _rect = transform.GetComponent<RectTransform>();
    }

    /// <summary>
    /// Resets tooltip texts and transform position on disable
    /// </summary>
    private void OnDisable()
    {
        textGUI.text = "";
        _offsetX = _offsetY = 0;
        transform.position = Vector3.zero;
    }

    /// <summary>
    /// Enable tooltip display
    /// </summary>
    /// <param name="timeToShow">time delay</param>
    /// <param name="buttonRect">RectTransform of button</param>
    /// <param name="offset">offset of position</param>
    /// <param name="tooltipText">text to display</param>
    public void EnableTooltip(float timeToShow, RectTransform buttonRect, Vector2 offset, 
        string tooltipText = "")
    {
        if(buttonRect == null) return;

        _offsetX = offset.x;
        _offsetY = offset.y;
        textGUI.text = tooltipText;
        
        _rect.sizeDelta = new Vector2(textGUI.preferredWidth + _textPadding * 2f,
                                      textGUI.preferredHeight + _textPadding * 2f);
        
        SetPosition(Utilities.WorldSpaceAnchors(buttonRect)[3]);
        
        Invoke(nameof(DisplayInvoke), timeToShow);
    }

    public void EnableTooltip(float timeToShow, Transform buttonRect, Vector2 offset,
        string tooltipText = "")
    {
        if (buttonRect == null) return;

        _offsetX = offset.x;
        _offsetY = offset.y;
        textGUI.text = tooltipText;

        _rect.sizeDelta = new Vector2(textGUI.preferredWidth + _textPadding * 2f,
                                      textGUI.preferredHeight + _textPadding * 2f);

        SetPosition(buttonRect.position);

        Invoke(nameof(DisplayInvoke), timeToShow);
    }

    /// <summary>
    /// Disables the tooltip and coroutine
    /// </summary>
    public void DisableTooltip()
    {
        CancelInvoke();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Set the position of the tooltip transform
    /// </summary>
    /// <param name="point">position</param>
    private void SetPosition(Vector3 point)
    {
        point.x += _offsetX;
        point.y += _offsetY;
        transform.position = point;
    }

    /// <summary>
    /// Invoke method to set the gameobject active
    /// </summary>
    private void DisplayInvoke() => gameObject.SetActive(true);
}
