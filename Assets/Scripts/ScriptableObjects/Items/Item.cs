using Globals;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ScriptableObjects.Items
{
    public abstract class Item : SerializedScriptableObject 
    {
        protected const string BoxGroupName = "Info";

        [BoxGroup(BoxGroupName), HideLabel, PreviewField(55)]
        [HorizontalGroup(BoxGroupName + "/Split", 55, LabelWidth = 67)]
        public Texture2D icon;
        
        [VerticalGroup(BoxGroupName + "/Split/Right")]
        public string Name;
        
        [VerticalGroup(BoxGroupName + "/Split/Right")]
        [Range(1, GV.MAXStackSize)]
        public byte stackSize;

        public virtual string GetModsString()
        {
            return "";
        }
    }
}
