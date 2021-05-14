using ScriptableObjects.Items;
using ScriptableObjects.Units;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjects;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// The MapManager handles the creation/loading of the Units on a map.
/// </summary>
public class MapManager : MonoBehaviour
{
    #region SINGLETON MONOBEHAVIOUR
    private static MapManager instance;
    private MapManager() { }
    public static MapManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<MapManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject();
                    instance = go.AddComponent<MapManager>();
                }
            }
            return instance;
        }
    }
    #endregion
    [Header("Map Data Scriptable Objects")]
    public MapData mapData;
    public MapData savedData;

    private readonly string SCRIPTABLE_UNITS_PATH = "SO/Units/player units/";
    private readonly string SCRIPTABLE_ITEMS_PATH = "SO/Items/";

    public void PreInitialize()
    {
        throw new System.NotImplementedException();
    }

    public void Initialize()
    {
        CreatePlayerUnits();
        CreateEnemyUnits();

        if (SceneManager.GetActiveScene().buildIndex == 1)
            InventoryManager.Instance.ClearInventory();
    }

    public void Refresh()
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// This method calls the UnitManager and creates the units that are in the MapData and have them spawn on a tile
    /// The way the Unit save/load works is that for the first map of the game, the MapData will contain the Player Units
    /// If there are PlayerUnits in the MapData SO, it will create the units with this.
    /// 
    /// For each subsequent map of the game, the MapData only contains the EnemyUnits and no PlayerUnits
    /// If a map is loaded and there is no PlayerUnits, it will use the SavedData SO to create the Units.
    /// The SavedData SO is built every time a map ends.
    /// 
    /// This is to ensure that only the units left in the previous maps are transferred to subsequent maps
    /// </summary>
    private void CreatePlayerUnits()
    {
        if (mapData.playerUnits.Count > 0)
        {
            foreach (PlayerUnitData playerUnitData in mapData.playerUnits)
            {
                if (GridManager.playerSpawnTiles.Count > 0)
                    UnitManager.Instance.CreateUnit(playerUnitData);
                else
                    Debug.LogError("Not enough tiles to spawn unit: " + playerUnitData.name);
            }
        }
        else
        {
            LoadPlayerUnits();
            ClearSavedData();
        }
    }

    /// <summary>
    /// This method calls the UnitManager and creates the units that are in the MapData and have them spawn on a tile
    /// </summary>
    private void CreateEnemyUnits()
    {
        foreach (EnemyUnitData enemyUnitData in mapData.enemyUnits)
        {
            if (GridManager.enemySpawnTiles.Count > 0)
                UnitManager.Instance.CreateUnit(enemyUnitData);
            else
                Debug.LogError("Not enough tiles to spawn unit: " + enemyUnitData.name);
        }
    }

    /// <summary>
    /// The way the saving works is that when a map is won, it will create new units inside the "SavedData" scriptable object.
    /// Since we are resetting all the player stats and only keeping the Equipment, we can instantiate new units completely and
    /// simply "transfer" the equipment of the unit into the new unit created in the SavedData scriptable object.
    /// </summary>
    public void SavePlayerUnits()
    {
        ClearSavedData();

        foreach (KeyValuePair<Vector3Int, Unit> item in UnitManager.Instance.unitDictionnary)
        {
            if (item.Value is PlayerUnit)
            {
                PlayerUnitData oldPlayerUnitData = item.Value.unitData as PlayerUnitData;

                PlayerUnitData newPlayerUnitData = Resources.Load<PlayerUnitData>(SCRIPTABLE_UNITS_PATH + oldPlayerUnitData.name).Clone();
                Equipment newEquipment = new Equipment();

                if(oldPlayerUnitData.equipment.mainHand != null)
                    newEquipment.mainHand = Resources.Load<Weapon>(SCRIPTABLE_ITEMS_PATH + oldPlayerUnitData.equipment.mainHand.name).Clone();

                if (oldPlayerUnitData.equipment.body != null)
                    newEquipment.body = Resources.Load<Armor>(SCRIPTABLE_ITEMS_PATH + oldPlayerUnitData.equipment.body.name).Clone();

                newPlayerUnitData.equipment = newEquipment;
                savedData.playerUnits.Add(newPlayerUnitData);
            }
        }        
    }

    /// <summary>
    /// This function is pretty much the same as the "Create" functions above but instead, will create the units using the "SavedData"
    /// Method was copy-pasted for clarity.
    /// </summary>
    private void LoadPlayerUnits()
    {
        foreach (PlayerUnitData playerUnitData in savedData.playerUnits)
        {
            if (GridManager.playerSpawnTiles.Count > 0)
                UnitManager.Instance.CreateUnit(playerUnitData);
            else
                Debug.LogError("Not enough tiles to spawn unit: " + playerUnitData.name);
        }
    }

    private void ClearSavedData()
    {
        savedData.playerUnits.Clear();
    }
}
