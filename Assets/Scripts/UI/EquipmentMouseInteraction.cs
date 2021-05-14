using System;
using ScriptableObjects.Items;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipmentMouseInteraction : MonoBehaviour, IPointerDownHandler, IDragHandler, IBeginDragHandler, 
    IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Canvas canvas;
    public GameObject dragedItem;

    private Type _type;
    private Image _itemImage;
    private DragedItemInfo _dragedItemInfo;
    private CanvasGroup _canvasGroup;

    // Events
    public UnityEvent leftClickEvent;
    public UnityEvent middleClickEvent;
    public UnityEvent rightClickEvent;
    public UnityEvent doubleClickEvent;
    
    private RectTransform _buttonRect;

    public bool IsDragable { get; set; } = false;
    
    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _itemImage = transform.Find("ItemImage").GetComponent<Image>();
        _dragedItemInfo = dragedItem.GetComponent<DragedItemInfo>();
        _buttonRect = gameObject.GetComponent<RectTransform>();
        
        _type = transform.name switch
        {
            "MainHandSlot" => typeof(Weapon),
            "BodySlot" => typeof(Armor),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                leftClickEvent.Invoke();
                break;
            case PointerEventData.InputButton.Middle:
                middleClickEvent.Invoke();
                break;
            case PointerEventData.InputButton.Right:
                rightClickEvent.Invoke();
                OverlayTooltip.Instance.DisableTooltip();
                break;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(!IsDragable) return;
        
        dragedItem.SetActive(true);
        dragedItem.GetComponent<Image>().sprite = _itemImage.sprite;
        dragedItem.GetComponent<DragedItemInfo>().item = eventData.pointerDrag.gameObject.name switch
        {
            "MainHandSlot" => InventoryManager.Instance.GetMainHand(),
            "BodySlot" => InventoryManager.Instance.GetBody(),
            _ => throw new ArgumentOutOfRangeException()
        };
        _canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!IsDragable) return;
        
        OverlayTooltip.Instance.DisableTooltip();
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            Input.mousePosition, canvas.worldCamera,
            out var movePos);
        
        dragedItem.transform.position = canvas.transform.TransformPoint(movePos);
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        dragedItem.SetActive(false);
        _canvasGroup.alpha = 1f;
    }
    
    public void OnDrop(PointerEventData eventData)
    {
        if (_dragedItemInfo.dragedItemIndex < 0) return;
        if (InventoryManager.Instance.GetItemType(_dragedItemInfo.dragedItemIndex) == _type)
            InventoryManager.Instance.EquipItem(_dragedItemInfo.dragedItemIndex);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        Item item = transform.name switch
        {
            "MainHandSlot" => InventoryManager.Instance.GetMainHand(),
            "BodySlot" => InventoryManager.Instance.GetBody(),
            _ => throw new ArgumentOutOfRangeException()
        };

        if (item != null)
        {
            string str = $"{item.Name}\n\n" +
                         $"{item.GetModsString()}\n" +
                         "Double Click to use";
            OverlayTooltip.Instance.EnableTooltip(1f, _buttonRect, new Vector2(1.1f, 0.3f), str);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OverlayTooltip.Instance.DisableTooltip();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 2)
            doubleClickEvent.Invoke();
    }
}
