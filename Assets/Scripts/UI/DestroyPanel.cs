using UnityEngine;
using UnityEngine.EventSystems;

public class DestroyPanel : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag.transform.GetComponent<InventoryMouseInteraction>() != null)
        {
            int index = eventData.pointerDrag.transform.GetComponent<InventoryMouseInteraction>().ItemIndex;
            InventoryManager.Instance.DestroyItemPrompt(index);
        }
    }
}
