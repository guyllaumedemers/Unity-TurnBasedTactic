using System;
using Sirenix.OdinInspector;

namespace ScriptableObjects.Items
{
    [Serializable]
    public class Equipment
    {
        [AssetsOnly]
        public Weapon mainHand;
        
        [AssetsOnly]
        public Armor body;
    }
}