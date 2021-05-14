using ScriptableObjects.Abilities;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace RPGEditor
{
    public class AbilityDrawer<TAbility> : OdinValueDrawer<TAbility> where TAbility : Ability
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var rect = EditorGUILayout.GetControlRect(label != null, 45);

            if (label != null)
                rect.xMin = EditorGUI.PrefixLabel(rect.AlignCenterY(15), label).xMin;
            else
                rect = EditorGUI.IndentedRect(rect);

            Ability ability = ValueEntry.SmartValue;
            Texture texture = null;

            if (ability)
            {
                texture = GUIHelper.GetAssetThumbnail(ability.icon, typeof(TAbility), true);
                GUI.Label(rect.AddXMin(50).AlignMiddle(16), EditorGUI.showMixedValue ? "-" : ability.Name.ToString());
            }

            ValueEntry.WeakSmartValue = SirenixEditorFields.UnityPreviewObjectField(rect.AlignLeft(45), ability, texture, ValueEntry.BaseValueType);
        }
    }
}
#endif