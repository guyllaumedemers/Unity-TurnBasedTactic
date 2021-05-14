using System;
using Globals;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Linq;

namespace ScriptableObjects.Items
{
    [CreateAssetMenu(fileName = GV.PlayerInventoryAsset, menuName = "InventorySystem/PlayerInventory") ]
    public class PlayerInventory : SerializedScriptableObject
    {
        [ShowInInspector]
        [ListDrawerSettings(HideRemoveButton = true, HideAddButton = true)]
        private ItemSlot[] _inventory = new ItemSlot[GV.MAXInventorySize];

        /// <summary>
        /// Get player inventory
        /// </summary>
        /// <returns>inventory</returns>
        public ItemSlot[] GetInventory() => _inventory;

        /// <summary>
        /// Adds an item to inventory at fist not full stack or call AddToEmptySlot
        /// </summary>
        /// <param name="item"></param>
        /// <param name="count"></param>
        public bool AddItem(Item item, int count = 1)
        {
            if (item == null) return false;
            
            if (item.stackSize > 1)
            {
                for (int i = 0; i < _inventory.Length; i++)
                {
                    if (_inventory[i].item != null &&
                        _inventory[i].item.Equals(item) &&
                        _inventory[i].itemCount + count <= _inventory[i].item.stackSize)
                    {
                        _inventory[i].itemCount += count;
                        return true;
                    }
                }
            }
            return AddToEmptySlot(item, count);
        }
        
        /// <summary>
        /// Adds item to inventory at first null item if inventory is not full
        /// </summary>
        /// <param name="item"></param>
        /// <param name="count"></param>
        private bool AddToEmptySlot(Item item, int count)
        {
            if (item != null && GetEmptyCount() > 0)
            {
                for (int i = 0; i < GV.MAXInventorySize; i++)
                {
                    if (_inventory[i].item == null)
                    {
                        _inventory[i].item = item;
                        _inventory[i].itemCount = count;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Removes count from stack or destroys item
        /// </summary>
        /// <param name="index"></param>
        public void RemoveItem(int index)
        {
            if (index < 0) return;

            if (_inventory[index].itemCount > 1)
            {
                _inventory[index].itemCount--;
            }
            else
            {
                _inventory[index].item = null;
                _inventory[index].itemCount = 0;
            }
        }
        
        /// <summary>
        /// Return count of null items in inventory
        /// </summary>
        /// <returns>count of empty items</returns>
        public int GetEmptyCount() => _inventory.Count(x => x.item == null);

        /// <summary>
        /// Gets lenght of ItemSlot array
        /// </summary>
        /// <returns>lenght of ItemSlot array</returns>
        public int GetLenght() => _inventory.Length;

        /// <summary>
        /// Get the item at specific index
        /// </summary>
        /// <param name="index">index of array</param>
        /// <returns>item at index</returns>
        public Item GetItem(int index) => index >= 0 ? _inventory[index].item : null;

        public Type GetItemType(int index) => _inventory[index].item.GetType();
        
        /// <summary>
        /// Get itemcount for item at specific index
        /// </summary>
        /// <param name="index">index of item</param>
        /// <returns>itemCount of item</returns>
        public int GetItemCount(int index) => _inventory[index].itemCount;
        
        /// <summary>
        /// Get the Texture2D of item at specific index
        /// </summary>
        /// <param name="index">index of item</param>
        /// <returns>Texture2D of item</returns>
        public Texture2D GetIcon(int index) => _inventory[index].item.icon;

        /// <summary>
        /// Get name of item at specific index
        /// </summary>
        /// <param name="index">index of item</param>
        /// <returns>name string</returns>
        public string GetName(int index) => _inventory[index].item.Name;

        /// <summary>
        /// Get Item at index
        /// </summary>
        /// <param name="index">index of item</param>
        /// <returns>Item</returns>
        private ItemSlot GetItemSlot(int index) => _inventory[index];

        /// <summary>
        /// Add ItemSlot at index
        /// </summary>
        /// <param name="itemSlot">ItemSlot</param>
        /// <param name="index">index</param>
        private void AddItemSlotAt(ItemSlot itemSlot, int index) => _inventory[index] = itemSlot;

        /// <summary>
        /// Swaps two items indexes
        /// </summary>
        /// <param name="fromIndex">first item index</param>
        /// <param name="toIndex">second item index</param>
        public void SwapItemSlots(int fromIndex, int toIndex)
        {
            ItemSlot tempItemSlot = GetItemSlot(toIndex);
            AddItemSlotAt(_inventory[fromIndex], toIndex);
            AddItemSlotAt(tempItemSlot, fromIndex);
        }

        /// <summary>
        /// Check if ItemSlot at index is null
        /// </summary>
        /// <param name="itemIndex">index of ItemSlot</param>
        /// <returns>true of false</returns>
        private bool IsItemSlotEmpty(int itemIndex) => _inventory[itemIndex].item == null;

        /// <summary>
        /// Add Item at index
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="index">index</param>
        /// <returns>true if ItemSlot is empty</returns>
        public bool AddItemAt(Item item, int index)
        {
            if (!IsItemSlotEmpty(index)) return false;
            _inventory[index].item = item;
            return true;

        }

        /// <summary>
        /// Clears inventory
        /// </summary>
        public void ClearInventory()
        {
            for(int i = 0; i < _inventory.Length; i++)
                _inventory[i] = new ItemSlot();
        }
    }
}