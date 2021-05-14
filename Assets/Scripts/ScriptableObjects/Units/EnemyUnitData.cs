using System.Collections.Generic;
using ScriptableObjects.Items;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ScriptableObjects.Units
{
    public class EnemyUnitData : UnitData
    {
        private UnitTypes _unityTypes;

        [VerticalGroup("Info/Split/Right")]
        [ValueDropdown("@_unityTypes.enemyTypes")]
        public string type;

        [TabGroup("Drops")] 
        public List<Item> drops;

#if UNITY_EDITOR
        private void OnEnable()
        {
            _unityTypes = AssetDatabase.LoadAssetAtPath<UnitTypes>("Assets/Resources/SO/Units/UnitTypes.asset");
        }
#endif
    }
}

