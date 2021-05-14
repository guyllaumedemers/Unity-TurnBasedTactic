using ScriptableObjects.Units;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace RPGEditor
{
    public class PlayerDataDrawer<TPlayerUnitData> : OdinValueDrawer<TPlayerUnitData>
        where TPlayerUnitData : PlayerUnitData
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var rect = EditorGUILayout.GetControlRect(label != null, 45);
            rect = EditorGUI.IndentedRect(rect);
            
            PlayerUnitData playerUnit = ValueEntry.SmartValue;
            Texture texture = null;

            if (playerUnit)
            {
                texture = GUIHelper.GetAssetThumbnail(playerUnit.icon, typeof(TPlayerUnitData), true);
                GUI.Label(rect.AddXMin(50).AlignMiddle(16), EditorGUI.showMixedValue ? "-" : playerUnit.Name + "  " +
                    $"[ AP:{playerUnit.stats.Ap} HP:{playerUnit.stats.Hp} DMG:{playerUnit.stats.Dmg} DEF:{playerUnit.stats.Def} ]");
            }
            
            ValueEntry.WeakSmartValue = SirenixEditorFields.UnityPreviewObjectField(rect.AlignLeft(45), playerUnit, texture, ValueEntry.BaseValueType);
        }
    }
}