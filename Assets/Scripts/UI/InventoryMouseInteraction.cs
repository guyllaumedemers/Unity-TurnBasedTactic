using ScriptableObjects.Items;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryMouseInteraction : MonoBehaviour, IPointerDownHandler, IDragHandler, IBeginDragHandler, IEndDragHandler,
    IDropHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Canvas canvas;
    public GameObject dragedItem;

    private Image _dragedItemImage;
    private DragedItemInfo _dragedItemInfo;

    private Image _itemImage;
    private CanvasGroup _canvasGroup;

    private RectTransform _buttonRect;
    
    
    // Events
    public UnityEvent leftClickEvent;
    public UnityEvent middleClickEvent;
    public IntUnityEvent rightClickEvent = new IntUnityEvent();
    public IntUnityEvent doubleClickEvent = new IntUnityEvent();
    public IntIntItemUnityEvent onDropEvent = new IntIntItemUnityEvent();
    
    public int ItemIndex { get; set; } = -1;
    public bool IsDragable { get; set; } = false;
    //public bool IsUsesable { get; set; } = false;
    //public bool IsEquipable { get; set; } = false;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _itemImage = transform.Find("ItemImage").GetComponent<Image>();

        _dragedItemImage = dragedItem.GetComponent<Image>();
        _dragedItemInfo = dragedItem.GetComponent<DragedItemInfo>();
        _buttonRect = gameObject.GetComponent<RectTransform>();
        
        dragedItem.SetActive(false);
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
                rightClickEvent.Invoke(ItemIndex);
                OverlayTooltip.Instance.DisableTooltip();
                break;
            default:
                break;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!IsDragable) return;

        dragedItem.SetActive(true);
        _dragedItemInfo.dragedItemIndex = ItemIndex;
        _dragedItemInfo.item = InventoryManager.Instance.GetItem(ItemIndex);

        _canvasGroup.alpha = 0.6f;
        _dragedItemImage.sprite = _itemImage.sprite;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!IsDragable) return;
        
        OverlayTooltip.Instance.DisableTooltip();
        dragedItem.transform.position = canvas.transform.TransformPoint(GetMovePos());
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!IsDragable) return;

        dragedItem.SetActive(false);
        _canvasGroup.alpha = 1f;
    }

    public void OnDrop(PointerEventData eventData)
    {
        onDropEvent.Invoke(_dragedItemInfo.dragedItemIndex, ItemIndex, _dragedItemInfo.item);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 2)
            doubleClickEvent.Invoke(ItemIndex);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Item item = InventoryManager.Instance.GetItem(ItemIndex);
        if (item != null)
        {
            string str = $"{item.Name}\n\n" +
                         $"{item.GetModsString()}\n" +
                         "Double click to use";
            
            OverlayTooltip.Instance.EnableTooltip(1f, _buttonRect,new Vector2(1.1f, 0.3f), str);
        }
            
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        OverlayTooltip.Instance.DisableTooltip();
    }

    private Vector3 GetMovePos()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            Input.mousePosition, canvas.worldCamera,
            out var movePos);

        return movePos;
    }
}