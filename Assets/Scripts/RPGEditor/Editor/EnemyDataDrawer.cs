using ScriptableObjects.Units;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace RPGEditor
{
    public class EnemyDataDrawer<TEnemyUnitData> : OdinValueDrawer<TEnemyUnitData>
        where TEnemyUnitData : EnemyUnitData
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var rect = EditorGUILayout.GetControlRect(label != null, 45);
            rect = EditorGUI.IndentedRect(rect);
            
            EnemyUnitData enemyUnit = ValueEntry.SmartValue;
            Texture texture = null;

            if (enemyUnit)
            {
                texture = GUIHelper.GetAssetThumbnail(enemyUnit.icon, typeof(TEnemyUnitData), true);
                GUI.Label(rect.AddXMin(50).AlignMiddle(16), EditorGUI.showMixedValue ? "-" : enemyUnit.Name + "  " +
                $"[ AP:{enemyUnit.stats.Ap} HP:{enemyUnit.stats.Hp} DMG:{enemyUnit.stats.Dmg} DEF:{enemyUnit.stats.Def} ]");
            }
            
            ValueEntry.WeakSmartValue = SirenixEditorFields.UnityPreviewObjectField(rect.AlignLeft(45), enemyUnit, texture, ValueEntry.BaseValueType);
        }
    }
}
