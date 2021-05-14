
using System;
using ScriptableObjects.Items;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ScriptableObjects.Units
{
    [Serializable]
    public class PlayerUnitData : UnitData
    {
        private UnitTypes _unityTypes;

        [VerticalGroup("Info/Split/Right")]
        [ValueDropdown("@_unityTypes.playerTypes")]
        public string type;
        
        [TabGroup("Equipment"), HideLabel]
        public Equipment equipment;

#if UNITY_EDITOR
        private void OnEnable()
        {
            _unityTypes = AssetDatabase.LoadAssetAtPath<UnitTypes>("Assets/Resources/SO/Units/UnitTypes.asset");
        }
#endif
        /// <summary>
        /// Get the player type
        /// </summary>
        /// <returns>player type</returns>
        public string GetPlayerType() => type;

        /// <summary>
        /// Get Weapon in main hand slot
        /// </summary>
        /// <returns>Weapon item</returns>
        public Weapon GetMainHand() => equipment.mainHand;

        /// <summary>
        /// Get Armor in body slot
        /// </summary>
        /// <returns>Armor item</returns>
        public Armor GetBody() => equipment.body;

        /// <summary>
        /// Set the main hand slot to Weapon item
        /// </summary>
        /// <param name="weapon">Weapon item</param>
        public void SetMainHand(Weapon weapon) => equipment.mainHand = weapon;

        /// <summary>
        /// Set the body slot to Armor item
        /// </summary>
        /// <param name="armor">Armor item</param>
        public void SetBody(Armor armor) => equipment.body = armor;
        
        /// <summary>
        /// Set the main hand slot to null
        /// </summary>
        public void EmptyMainHand() => equipment.mainHand = null;
        
        /// <summary>
        /// Set the body slot to null
        /// </summary>
        public void EmptyBody() => equipment.body = null;
    }
}
