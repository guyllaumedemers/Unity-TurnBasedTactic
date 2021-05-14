using System.Linq;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace ScriptableObjects.Units
{
    [Serializable]
    public class UnitTypes : SerializedScriptableObject
    {
        [PropertySpace(10)]
        [ListDrawerSettings(Expanded = true)]
        public HashSet<string> playerTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        [PropertySpace(10)]
        [ListDrawerSettings(Expanded = true)]
        public HashSet<string> enemyTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        private void OnValidate()
        {
            for (int i = 0; i < playerTypes.Count; i++)
            {
                if (!string.IsNullOrEmpty(playerTypes.ElementAt(i)) && 
                    !char.IsUpper(playerTypes.ElementAt(i).First()))
                {
                    string str = playerTypes.ElementAt(i).FirstCharToUpper();
                    playerTypes.Remove(playerTypes.ElementAt(i));
                    playerTypes.Add(str);
                }
            }
            
            for (int i = 0; i < enemyTypes.Count; i++)
            {
                if (!string.IsNullOrEmpty(enemyTypes.ElementAt(i)) && 
                    !char.IsUpper(enemyTypes.ElementAt(i).First()))
                {
                    string str = enemyTypes.ElementAt(i).FirstCharToUpper();
                    enemyTypes.Remove(enemyTypes.ElementAt(i));
                    enemyTypes.Add(str);
                }
            }
        }
    }
}