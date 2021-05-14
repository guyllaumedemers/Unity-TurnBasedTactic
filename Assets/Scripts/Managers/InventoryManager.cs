using System;
using System.Collections.Generic;
using System.Linq;
using ScriptableObjects.Items;
using ScriptableObjects.Stats;
using ScriptableObjects.Units;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class InventoryManager : MonoBehaviour
{
    [PropertyOrder(1), ListDrawerSettings(HideRemoveButton = true, HideAddButton = true)]
    public ItemSlot[] inventory;
    
    private EditorInventory _soIventory;
    private PlayerInventory _playerInventory;
    private Unit _playerUnit;
    
    private RectTransform _itemContainer;
    private RectTransform _itemSlotTemplate;
    
    private RectTransform _mainHandTransform;
    private RectTransform _bodyTransform;
    private Image _mainHandImage;
    private Image _bodyImage;

    private Dictionary<string, int> _typesOnCooldown = new Dictionary<string, int>();
    
    #region SINGLETON MONOBEHAVIOUR
    private static InventoryManager _instance;
    private InventoryManager() { }
    
    public static InventoryManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<InventoryManager>();
                if (_instance == null)
                {
                    //GameObject go = new GameObject();
                    //instance = go.AddComponent<InventoryManager>();
                }
            }
            return _instance;
        }
    }
    #endregion
    
    /// <summary>
    /// Initializes class
    /// </summary>
    public void PreInitialize()
    {
        InitializeMembers();
        DestroyItemDlg.Instance.Initialize();
        InventoryFullDlg.Instance.Initialize();
        
        DestroyItemDlg.Instance.intEvent.AddListener(RemoveItem);
        _mainHandTransform.GetComponent<EquipmentMouseInteraction>().doubleClickEvent.AddListener(UnEquipMainHand);
        _bodyTransform.GetComponent<EquipmentMouseInteraction>().doubleClickEvent.AddListener(UnEquipBody);
        
        InputManager.Instance.playerSelectedEvent += PlayerSelected;
        InputManager.Instance.deselectEvent += PlayerDeselected;
        
        BattleTurnManager.Instance.trackingTurnEvent += OnTurnCompleted;
        
        LoadPlayerInventory();
        LoadEditorInventory();
        
        RefreshInventoryItems();
    }
    
    /// <summary>
    /// Initializes member variables
    /// </summary>
    private void InitializeMembers()
    {
        _itemContainer = transform.FindDeepChild("ItemContainer").GetComponent<RectTransform>();
        _itemSlotTemplate = _itemContainer.FindDeepChild("ItemSlotTemplate").GetComponent<RectTransform>();
        _mainHandTransform = transform.FindDeepChild("MainHandSlot").GetComponent<RectTransform>();
        _bodyTransform = transform.FindDeepChild("BodySlot").GetComponent<RectTransform>();
        
        _mainHandImage = _mainHandTransform.FindDeepChild("ItemImage").GetComponent<Image>();
        _bodyImage = _bodyTransform.FindDeepChild("ItemImage").GetComponent<Image>();
        
        _itemSlotTemplate.gameObject.SetActive(false);
    }

    /// <summary>
    /// Invoked when it's the player's turn
    /// </summary>
    private void OnTurnCompleted()
    {
        foreach (var kp in _typesOnCooldown.ToList())
        {
            _typesOnCooldown[kp.Key]--;
            if (_typesOnCooldown[kp.Key] <= 0)
                _typesOnCooldown.Remove(kp.Key);
        }
        
        RefreshInventoryItems();
    }

    /// <summary>
    /// Loads player inventory from scriptable object
    /// </summary>
    private void LoadPlayerInventory()
    {
        _playerInventory = (PlayerInventory) Resources.Load("SO/Inventory/PlayerInventory");
        inventory = _playerInventory.GetInventory();
        //ClearInventory();
    }

    /// <summary>
    /// Loads etidor inventory from scriptable object if items are present and resets it
    /// </summary>
    private void LoadEditorInventory()
    {
        _soIventory = (EditorInventory) Resources.Load("SO/Inventory/EditorInventory");
        if (_soIventory.inventory.Cast<ItemSlot>().Count(x => x.item != null) <= 0) return;
        
        foreach (var itemSlot in _soIventory.inventory.Cast<ItemSlot>().Where(i => i.item != null).ToArray())
            AddItem(itemSlot.item, itemSlot.itemCount);
        
        _soIventory.ResetInventory();
    }
    
    /// <summary>
    /// Sets _playerUnit when player unit is selected, refreshes the units equipment
    /// </summary>
    /// <param name="selectedplayerunit">current selected player</param>
    private void PlayerSelected(PlayerUnit selectedplayerunit)
    {
        _playerUnit = selectedplayerunit;
        RefreshUnitEquipment();
    }

    /// <summary>
    /// Set _playerUnit to null when player unit is deselected
    /// </summary>
    private void PlayerDeselected() => _playerUnit = null;

    /// <summary>
    /// Recreates inventory from template and assigns button function calls to transforms
    /// </summary>
    [Button("Refresh Inventory"), PropertyOrder(0), PropertySpace(10)]
    private void RefreshInventoryItems()
    {
        DestroyItemContainerChildTransforms();
        
        for (int itemIndex = 0; itemIndex < _playerInventory.GetLenght(); itemIndex++)
        {
            RectTransform itemSlotRectTransform = Instantiate(_itemSlotTemplate, _itemContainer).GetComponent<RectTransform>();
            Image itemIcon = itemSlotRectTransform.Find("ItemImage").GetComponent<Image>();
            TextMeshProUGUI itemCount = itemSlotRectTransform.Find("ItemCount").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI itemCooldown = itemSlotRectTransform.Find("ItemCooldown").GetComponent<TextMeshProUGUI>();
            InventoryMouseInteraction inventoryMouseInteraction = itemSlotRectTransform.GetComponent<InventoryMouseInteraction>();
            
            CreateItemButton(itemIndex, itemSlotRectTransform, itemIcon, itemCount, itemCooldown, inventoryMouseInteraction);
        }
    }

    /// <summary>
    /// Creates an item button
    /// </summary>
    /// <param name="itemIndex">item index</param>
    /// <param name="itemSlotRectTransform"> item slot rect transform</param>
    /// <param name="itemIcon">item icon image</param>
    /// <param name="itemCount">item count TMP</param>
    /// <param name="itemCooldown">item cooldown TMP</param>
    /// <param name="inventoryMouseInteraction">mouse interaction</param>
    private void CreateItemButton(int itemIndex, 
                                  RectTransform itemSlotRectTransform, 
                                  Image itemIcon, 
                                  TextMeshProUGUI itemCount, 
                                  TextMeshProUGUI itemCooldown, 
                                  InventoryMouseInteraction inventoryMouseInteraction)
    {
        inventoryMouseInteraction.ItemIndex = itemIndex;
        inventoryMouseInteraction.onDropEvent.AddListener(OnDrop);
        itemSlotRectTransform.gameObject.SetActive(true);

        if (_playerInventory.GetItem(itemIndex) != null)
        {
            itemIcon.sprite = CreateSprite(_playerInventory.GetIcon(itemIndex));
            inventoryMouseInteraction.IsDragable = true;
            
            string itemName = _playerInventory.GetName(itemIndex);
            if (_typesOnCooldown.ContainsKey(itemName))
                SetItemCooldown(_typesOnCooldown[itemName], itemIcon, itemCooldown, itemCount);
            else
            {
                SetItemNormal(_playerInventory.GetItemCount(itemIndex), itemIcon, itemCooldown, itemCount);
                
                Type type = _playerInventory.GetItemType(itemIndex);
                if (type == typeof(Consumable))
                    inventoryMouseInteraction.doubleClickEvent.AddListener(UseConsumable);
                else if (type == typeof(Weapon) || type == typeof(Armor))
                    inventoryMouseInteraction.doubleClickEvent.AddListener(EquipItem);
            }
        }
        else
        {
            inventoryMouseInteraction.IsDragable = false;   
        }
    }

    /// <summary>
    /// Destroys child transfomrs of item container except the template and background
    /// </summary>
    private void DestroyItemContainerChildTransforms()
    {
        foreach (Transform t in _itemContainer)
        {
            if (t.gameObject.name.Equals("ItemSlotTemplate") || t.gameObject.name.Equals("Background")) continue;
            Destroy(t.gameObject);
        }
    }

    /// <summary>
    /// Create a sprite from Texture2D
    /// </summary>
    /// <param name="icon">Texture2D</param>
    /// <returns>Sprite of Texture2D</returns>
    private Sprite CreateSprite(Texture2D icon) => Sprite.Create(icon, new Rect(0, 0, icon.width, icon.height), new Vector2());
    
    /// <summary>
    /// Set item to normal state
    /// </summary>
    /// <param name="stackSize">number of stack size</param>
    /// <param name="itemIcon">image of item</param>
    /// <param name="itemCooldown">item cooldown TMP</param>
    /// <param name="itemCount">item count TMP</param>
    private void SetItemNormal(int stackSize, Image itemIcon, TextMeshProUGUI itemCooldown, TextMeshProUGUI itemCount)
    {
        itemIcon.color = Color.white;
        itemCooldown.SetText("");
        itemCount.SetText(stackSize > 1 ? stackSize.ToString() : string.Empty);
    }

    /// <summary>
    /// Set item to cooldown state
    /// </summary>
    /// <param name="cooldown">cooldown number</param>
    /// <param name="itemIcon">image of item</param>
    /// <param name="itemCooldown">item cooldown TMP</param>
    /// <param name="itemCount">item count TMP</param>
    private void SetItemCooldown(int cooldown, Image itemIcon, TextMeshProUGUI itemCooldown, TextMeshProUGUI itemCount)
    {
        itemIcon.color = Color.black;
        itemCooldown.SetText(cooldown.ToString().AddColor(Color.red));
        itemCount.SetText("");
    }

    /// <summary>
    /// Swap Item on drop of drag and drop
    /// </summary>
    /// <param name="fromIndex"></param>
    /// <param name="to"></param>
    /// <param name="item"></param>
    private void OnDrop(int fromIndex, int to, Item item)
    {
        if (fromIndex >= 0)
            SwapItemSlots(fromIndex, to);
        else if (item != null)
        {
            if (GetItem(to) == null)
                UnEquipItemAt(item, to);
            else if (GetItem(to).GetType() == item.GetType())
                EquipItem(to);
        }
    }

    /// <summary>
    /// Updates unit equipment
    /// </summary>
    private void RefreshUnitEquipment()
    {
        if (((PlayerUnitData)_playerUnit.unitData).GetMainHand() != null)
            SetEquipmentItemSlot(_mainHandImage, _mainHandTransform, ((PlayerUnitData)_playerUnit.unitData).GetMainHand().icon);
        else
        {
            ClearImage(_mainHandImage);
            _mainHandTransform.GetComponent<EquipmentMouseInteraction>().IsDragable = false;
        }
        
        if ( ((PlayerUnitData)_playerUnit.unitData).GetBody() != null )
            SetEquipmentItemSlot(_bodyImage, _bodyTransform, ((PlayerUnitData)_playerUnit.unitData).GetBody().icon);
        else
        {
            ClearImage(_bodyImage); 
            _bodyTransform.GetComponent<EquipmentMouseInteraction>().IsDragable = false;
        }
    }

    /// <summary>
    /// Set the Equipment item slot
    /// </summary>
    /// <param name="itemImage">item image</param>
    /// <param name="itemTransform">item slot rect transform</param>
    /// <param name="itemIcon">item Texture2D</param>
    private void SetEquipmentItemSlot(Image itemImage, RectTransform itemTransform, Texture2D itemIcon)
    {
        itemImage.color = Color.white;
        itemImage.sprite = CreateSprite(itemIcon);
        itemTransform.GetComponent<EquipmentMouseInteraction>().IsDragable = true;
    }

    /// <summary>
    /// Sets the image sprite to null and black color with zero alpha
    /// </summary>
    /// <param name="img">image of Item</param>
    private void ClearImage(Image img)
    {
        img.sprite = null;
        img.color = new Color (255, 255, 255, 0);
    }

    /// <summary>
    /// Removes weapon in main hand and adds it to inventory
    /// </summary>
    private void UnEquipMainHand()
    {
        if (IsInventoryFull() || ((PlayerUnitData)_playerUnit.unitData).GetMainHand() == null) return;
        
        _mainHandTransform.GetComponent<EquipmentMouseInteraction>().IsDragable = false;

        _playerUnit.SetItemMods(((PlayerUnitData)_playerUnit.unitData).GetMainHand().mods.stats, -1);
        AddItem(((PlayerUnitData)_playerUnit.unitData).GetMainHand());
        ((PlayerUnitData)_playerUnit.unitData).EmptyMainHand();
        
        RefreshUnitEquipment();
    }

    /// <summary>
    /// Removes armor in body and adds it to inventory
    /// </summary>
    private void UnEquipBody()
    {
        if (IsInventoryFull() || ((PlayerUnitData)_playerUnit.unitData).GetBody() == null) return;

        _bodyTransform.GetComponent<EquipmentMouseInteraction>().IsDragable = false;

        _playerUnit.SetItemMods(((PlayerUnitData) _playerUnit.unitData).GetBody().mods.stats, -1);
        AddItem(((PlayerUnitData)_playerUnit.unitData).GetBody());
        ((PlayerUnitData)_playerUnit.unitData).EmptyBody();
        
        RefreshUnitEquipment();
    }

    /// <summary>
    /// Equips weapon or armor from inventory, removes and adds modifications to player unit, swaps out object with inventory
    /// </summary>
    /// <param name="index">index of Item</param>
    public void EquipItem(int index)
    {
        if (_playerUnit == null) return;
        
        if(_playerInventory.GetItemType(index) != typeof(Weapon) && 
           _playerInventory.GetItemType(index) != typeof(Armor)) return;
        
        Item oldItem = null;
        Item newItem = _playerInventory.GetItem(index);
            
        List<StatValue> oldMods = null;
        List<StatValue> newMods = new List<StatValue>();
            
        switch (newItem) // Type switch
        {
            case Weapon weapon:
            {
                if (((PlayerUnitData) _playerUnit.unitData).GetMainHand() != null)
                {
                    oldItem = ((PlayerUnitData) _playerUnit.unitData).GetMainHand();
                    oldMods = ((Weapon)oldItem).mods.stats;
                }
                    
                ((PlayerUnitData) _playerUnit.unitData).SetMainHand(weapon);
                newMods = weapon.mods.stats;
                break;
            }
            case Armor armor:
            {
                if (((PlayerUnitData) _playerUnit.unitData).GetBody() != null)
                {
                    oldItem = ((PlayerUnitData) _playerUnit.unitData).GetBody();
                    oldMods = ((Armor)oldItem).mods.stats;
                }
                    
                ((PlayerUnitData) _playerUnit.unitData).SetBody(armor);
                newMods = armor.mods.stats;
                break;
            }
        }
        
        RemoveItem(index);
        
        if (oldItem != null)
        {
            AddItem(oldItem);
            _playerUnit.SetItemMods(oldMods, - 1);
        }
        
        _playerUnit.SetItemMods(newMods);
        
        RefreshUnitEquipment();
    }

    /// <summary>
    /// Adds an Item to inventory at fist not full stack or call AddToEmptySlot
    /// </summary>
    /// <param name="item"></param>
    /// <param name="count"></param>
    private bool AddItem(Item item, int count = 1)
    {
        if (IsInventoryFull()) return false;
        
        count = count <= 0 ? 1 : count;
        _playerInventory.AddItem(item, count);
        RefreshInventoryItems();
        
        return true;
    }

    /// <summary>
    /// Sends modifications from consumable item to the ActionSystem and destroys the item from inventory
    /// </summary>
    /// <param name="index">index of Item</param>
    private void UseConsumable(int index)
    {
        if (_playerUnit == null) return;
        
        Consumable consumable = _playerInventory.GetItem(index) as Consumable;
        if (consumable != null)
        {
            ActionSystem.UseConsumable(_playerUnit, consumable, _playerUnit.positionGrid);

            if(!_typesOnCooldown.ContainsKey(consumable.Name))
                _typesOnCooldown.Add(consumable.Name, consumable.cooldown);
            
            RemoveItem(index);
        }
    }
    
    /// <summary>
    /// Removes count from stack or destroys Item
    /// </summary>
    /// <param name="index">index of Item</param>
    private void RemoveItem(int index)
    {
        _playerInventory.RemoveItem(index);
        RefreshInventoryItems();
    }

    /// <summary>
    /// Display the destroy item dialog
    /// </summary>
    /// <param name="index">index of Item to destroy</param>
    public void DestroyItemPrompt(int index)
    {
        if (_playerInventory.GetItem(index) != null)
        {
            DestroyItemDlg.Instance.gameObject.SetActive(true);
            DestroyItemDlg.Instance.SetDialog(index, _playerInventory.GetIcon(index), _playerInventory.GetName(index));   
        }
    }
    
    /// <summary>
    /// Check if inventory has a free Item slot
    /// </summary>
    /// <returns>true if inventory has a free slot</returns>
    private bool IsInventoryFull()
    {
        if (_playerInventory.GetEmptyCount() > 0) return false;
        DisplayInventoryFullDlg();
        return true;
    }

    /// <summary>
    /// Display inventory full dialog
    /// </summary>
    private void DisplayInventoryFullDlg() => InventoryFullDlg.Instance.gameObject.SetActive(true);

    /// <summary>
    /// Swap 2 Items position in inventory
    /// </summary>
    /// <param name="fromIndex">index of first Item</param>
    /// <param name="toIndex">index of second Item</param>
    private void SwapItemSlots(int fromIndex, int toIndex)
    {
        _playerInventory.SwapItemSlots(fromIndex, toIndex);
        RefreshInventoryItems();
    }
    
    /// <summary>
    /// Get Item in main hand slot
    /// </summary>
    /// <returns>Item</returns>
    public Item GetMainHand() => ((PlayerUnitData) _playerUnit.unitData).GetMainHand();
    
    /// <summary>
    /// Get Item in body slot
    /// </summary>
    /// <returns>Item</returns>
    public Item GetBody() => ((PlayerUnitData) _playerUnit.unitData).GetBody();
    
    /// <summary>
    /// Un equip item at index of inventory
    /// </summary>
    /// <param name="item">Item to un equip</param>
    /// <param name="index">index of inventory</param>
    private void UnEquipItemAt(Item item, int index)
    {
        if (_playerInventory.AddItemAt(item, index) || _playerInventory.AddItem(item))
        {
            if (_playerInventory.GetItemType(index) == typeof(Weapon))
            {
                _playerUnit.SetItemMods(((PlayerUnitData)_playerUnit.unitData).equipment.mainHand.mods.stats, -1);
                ((PlayerUnitData)_playerUnit.unitData).EmptyMainHand();
            }
            else if(_playerInventory.GetItemType(index) == typeof(Armor))
            {
                _playerUnit.SetItemMods(((PlayerUnitData)_playerUnit.unitData).equipment.body.mods.stats, -1);
                ((PlayerUnitData)_playerUnit.unitData).EmptyBody();
            }
        }
        
        RefreshUnitEquipment();
        RefreshInventoryItems();
    }

    /// <summary>
    /// Get Item at index
    /// </summary>
    /// <param name="index">index of Item</param>
    /// <returns>Item</returns>
    public Item GetItem(int index) => _playerInventory.GetItem(index);

    /// <summary>
    /// Get the Item type at index
    /// </summary>
    /// <param name="index">index of Item</param>
    /// <returns>type of Item</returns>
    public Type GetItemType(int index) => _playerInventory.GetItemType(index);

    /// <summary>
    /// Adds enemy drop items to inventory returns when inventory is full
    /// </summary>
    /// <param name="items">list of items</param>
    public void AddEnemyDrops(List<Item> items)
    {
        foreach (var item in items)
        {
            if(!AddItem(item))
                break;
        }
    }
    
    /// <summary>
    /// Clears player inventory
    /// </summary>
    public void ClearInventory() => _playerInventory.ClearInventory();
}