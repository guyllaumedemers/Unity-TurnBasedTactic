namespace ScriptableObjects.Items
{
    using System;
    
    [Serializable]
    public struct ItemSlot
    {
        public Item item;
        public int itemCount;
    }
}