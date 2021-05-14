using Globals;
using Sirenix.OdinInspector;

namespace ScriptableObjects.Items
{
    public class EditorInventory : SerializedScriptableObject
    {
        public ItemSlot[,] inventory = new ItemSlot[GV.MAXInventorySize / 4, 4];

        public void ResetInventory()
        {
            inventory = new ItemSlot[GV.MAXInventorySize / 4, 4];
        }
    }
}