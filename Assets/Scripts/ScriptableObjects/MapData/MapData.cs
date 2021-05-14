using ScriptableObjects.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapData", menuName = "Map Data", order = 1)]
public class MapData : ScriptableObject
{
    [Header("Player Units")]
    public List<PlayerUnitData> playerUnits;
    [Header("Enemy Units")]
    public List<EnemyUnitData> enemyUnits;
}
