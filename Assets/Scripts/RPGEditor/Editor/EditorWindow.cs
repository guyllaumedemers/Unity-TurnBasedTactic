using System.Linq;
using Globals;
using ScriptableObjects.Abilities;
using ScriptableObjects.Items;
using ScriptableObjects.Units;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace RPGEditor
{
    public class EditorWindow : OdinMenuEditorWindow
    {
        [MenuItem("Tools/RPG Editor")]
        private static void Open()
        {
            TextureManager.Instance.Initialize("Items");
            var window = GetWindow<EditorWindow>();
        }

        protected override void OnDestroy()
        {
            AssetDatabase.SaveAssets();
            base.OnDestroy();
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree(false);
            
            tree.DefaultMenuStyle.SetHeight(50);
            tree.DefaultMenuStyle.SetIconSize(40);
            tree.Config.DrawSearchToolbar = true;
            
            tree.AddAssetAtPath("UnitTypes", GV.UnitSoPath + GV.UnitTypesAsset, typeof(UnitTypes)).AddIcon(EditorIcons.Globe);

            tree.Add("Player", null, EditorIcons.Flag);
            tree.Add("Player/PlayerUnits", null, TextureManager.Instance.GetFirstLike("Man_1_nobg"));
            tree.AddAllAssetsAtPath("Player/PlayerUnits", GV.UnitSoPath, typeof(PlayerUnitData), true, true);
            tree.EnumerateTree().AddIcons<PlayerUnitData>(x => x.icon);

            tree.AddAssetAtPath("Player/Inventory", GV.InventorySoPath + GV.EditorInventoryAsset).AddIcon(TextureManager.Instance.GetFirstLike("bag"));
            
            tree.Add("EnemyUnits", null, EditorIcons.Crosshair);
            tree.AddAllAssetsAtPath("EnemyUnits", GV.UnitSoPath, typeof(EnemyUnitData), true, true);
            tree.EnumerateTree().AddIcons<EnemyUnitData>(x => x.icon);
            
            tree.Add("Abilities", null, EditorIcons.GridImageTextList);
            tree.AddAllAssetsAtPath("Abilities", GV.AbilitiesSoPath, typeof(Ability), true).ForEach(AddDragHandles);
            tree.EnumerateTree().Where(x => x.Value as Item).ForEach(AddDragHandles);
            tree.EnumerateTree().AddIcons<Ability>(x => x.icon);
            
            tree.Add("Items", null, EditorIcons.ShoppingBasket);
            tree.Add("Items/Weapons", null, TextureManager.Instance.GetFirstLike("sword_nobg"));
            tree.AddAllAssetsAtPath("Items/Weapons", GV.ItemsSoPath, typeof(Weapon), true).ForEach(AddDragHandles);
            tree.EnumerateTree().Where(x => x.Value as Item).ForEach(AddDragHandles);
            tree.EnumerateTree().AddIcons<Item>(x => x.icon);
            
            tree.Add("Items/Armor", null, TextureManager.Instance.GetFirstLike("armor"));
            tree.AddAllAssetsAtPath("Items/Armor", GV.ItemsSoPath, typeof(Armor), true).ForEach(AddDragHandles);
            tree.EnumerateTree().Where(x => x.Value as Item).ForEach(AddDragHandles);
            tree.EnumerateTree().AddIcons<Item>(x => x.icon);
            
            tree.Add("Items/Consumables", null, TextureManager.Instance.GetFirstLike("potion_nobg"));
            tree.AddAllAssetsAtPath("Items/Consumables", GV.ItemsSoPath, typeof(Consumable), true).ForEach(AddDragHandles);
            tree.EnumerateTree().Where(x => x.Value as Item).ForEach(AddDragHandles);
            tree.EnumerateTree().AddIcons<Item>(x => x.icon);
            
            return tree;
        }

        private void AddDragHandles(OdinMenuItem menuItem) => menuItem.OnDrawItem += x => DragAndDropUtilities.DragZone(menuItem.Rect, menuItem.Value, false, false);
        
        protected override void OnBeginDrawEditors()
        {
            var selected = MenuTree.Selection.FirstOrDefault();
            var toolbarHeight = MenuTree.Config.SearchToolbarHeight;
            
            SirenixEditorGUI.BeginHorizontalToolbar(toolbarHeight);
            {
                if (selected != null)
                    GUILayout.Label(selected.Name);
                
                // if (SirenixEditorGUI.ToolbarButton(new GUIContent("Create UnitTypes")))
                // {
                //     AssetDatabase.DeleteAsset(GV.UnitSoPath + GV.UnitTypesAsset);
                //     AssetDatabase.CreateAsset(CreateInstance<UnitTypes>(), GV.UnitSoPath + GV.UnitTypesAsset);
                //     
                //     TrySelectMenuItemWithObject(typeof(UnitTypes));
                // }
                
                if (SirenixEditorGUI.ToolbarButton(new GUIContent("Create Unit")))
                {
                    ScriptableObjectCreator.ShowDialog<UnitData>(GV.UnitSoPath, obj =>
                    {
                        obj.Name = obj.name;
                        TrySelectMenuItemWithObject(obj);
                    });
                }

                if (SirenixEditorGUI.ToolbarButton(new GUIContent("Create Inventory")))
                {
                    AssetDatabase.DeleteAsset(GV.ItemsSoPath + GV.EditorInventoryAsset);
                    AssetDatabase.CreateAsset(CreateInstance<EditorInventory>(), GV.InventorySoPath + GV.EditorInventoryAsset);
                    
                    TrySelectMenuItemWithObject(typeof(EditorInventory));
                }
                
                if (SirenixEditorGUI.ToolbarButton(new GUIContent("Create Ability")))
                {
                    ScriptableObjectCreator.ShowDialog<Ability>(GV.AbilitiesSoPath, obj =>
                    {
                        obj.Name =  obj.name;
                        TrySelectMenuItemWithObject(obj);
                    });
                }
                
                if (SirenixEditorGUI.ToolbarButton(new GUIContent("Create Item")))
                {
                    ScriptableObjectCreator.ShowDialog<Item>(GV.ItemsSoPath, obj =>
                    {
                        obj.Name = obj.name;
                        TrySelectMenuItemWithObject(obj);
                    });
                }

                if (SirenixEditorGUI.ToolbarButton(new GUIContent("Save")))
                {
                    AssetDatabase.SaveAssets();
                }
            }
            SirenixEditorGUI.EndHorizontalToolbar();
        }
    }
}
#endif
