using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public struct RealtimeOverlay
{
    public OverlayType overlayType;
    public bool isEnabled;
    public int range;
}

public struct OverlayProperties
{
    public bool canUseAbility;
    public bool canSelfCastOnly;
    public bool canCastOnFriendlies;
    public bool canDamageFriendlies;
}

public enum OverlayType
{
    Cross,
    Diamond,
    Square,
    Self
}

[System.Serializable]
public static class GridManager
{
    private static Grid grid;

    [HideInInspector] public static List<Vector3Int> RangeOverlayTiles { get; private set; }
    [HideInInspector] public static List<Vector3Int> PathNodesOverlayTiles { get; private set; }
    [HideInInspector] public static List<Vector3Int> playerSpawnTiles;
    [HideInInspector] public static List<Vector3Int> enemySpawnTiles;

    private static Tilemap floorsTilemap;
    private static Tilemap wallsTilemap;
    private static Tilemap pitsTilemap;
    private static Tilemap voidTilemap;
    private static Tilemap rangeOverlayTilemap;
    private static Tilemap cursorOverlayTilemap;
    private static Tilemap pathfindingOverlayTilemap;
    private static Tilemap spawnPointsTilemap;

    private static Tile hoverTile;
    private static Tile selectedTile;
    private static Tile deniedTile;
    private static Tile confirmedTile;
    private static Tile warningTile;

    private static Tile playerSpawnTile;
    private static Tile enemySpawnTile;

    public static AStar aStar;

    public static RealtimeOverlay realtimeOverlay;
    public static OverlayProperties overlayProperties;

    public static void PreInitialize()
    {
        grid = GameObject.FindGameObjectWithTag("MapGrid").GetComponent<Grid>();
        floorsTilemap = GameObject.FindGameObjectWithTag("FloorsTilemap").GetComponent<Tilemap>();
        wallsTilemap = GameObject.FindGameObjectWithTag("WallsTilemap").GetComponent<Tilemap>();
        pitsTilemap = GameObject.FindGameObjectWithTag("PitsTilemap").GetComponent<Tilemap>();
        voidTilemap = GameObject.FindGameObjectWithTag("VoidTilemap").GetComponent<Tilemap>();
        rangeOverlayTilemap = GameObject.FindGameObjectWithTag("RangeOverlayTilemap").GetComponent<Tilemap>();
        cursorOverlayTilemap = GameObject.FindGameObjectWithTag("CursorOverlayTilemap").GetComponent<Tilemap>();
        pathfindingOverlayTilemap = GameObject.FindGameObjectWithTag("PathfindingOverlayTilemap").GetComponent<Tilemap>();
        spawnPointsTilemap = GameObject.FindGameObjectWithTag("SpawnPointsTilemap").GetComponent<Tilemap>();

        hoverTile = Resources.Load<Tile>("Tilemaps/Sprites/Overlay/ISO_Overlay");
        selectedTile = Resources.Load<Tile>("Tilemaps/Sprites/Overlay/ISO_Overlay_Selected");
        deniedTile = Resources.Load<Tile>("Tilemaps/Sprites/Overlay/ISO_Overlay_Denied");
        confirmedTile = Resources.Load<Tile>("Tilemaps/Sprites/Overlay/ISO_Overlay_Confirmed");
        warningTile = Resources.Load<Tile>("Tilemaps/Sprites/Overlay/ISO_Overlay_Warning");

        playerSpawnTile = Resources.Load<Tile>("Tilemaps/Sprites/Overlay/PlayerSpawnTile");
        enemySpawnTile = Resources.Load<Tile>("Tilemaps/Sprites/Overlay/EnemySpawnTile");

        RangeOverlayTiles = new List<Vector3Int>();
        PathNodesOverlayTiles = new List<Vector3Int>();
        playerSpawnTiles = new List<Vector3Int>();
        enemySpawnTiles = new List<Vector3Int>();

        aStar = GameObject.Find("AStarManager").GetComponent<AStar>();
    }

    public static void Initialize()
    {
        BuildSpawnTilesDict();
        RegisterEvent();
    }

    public static void Refresh()
    {
        if (realtimeOverlay.isEnabled)
        {
            ClearTiles();
            DrawRangeOverlay(realtimeOverlay.overlayType, realtimeOverlay.range, InputManager.Instance.currentTileMousePos, overlayProperties);
        }
    }

    /// <summary>
    /// This is the function that handles the mouse overlay when hovering on the map, handles mostly the tile shown at the mouse position.
    /// </summary>
    /// <param name="currentMousePosition"></param>
    /// <param name="previousMousePosition"></param>
    public static void HoverOnTile(Vector3Int currentMousePosition, Vector3Int previousMousePosition)
    {
        if (aStar.selected != null)
        {
            if (floorsTilemap.HasTile(currentMousePosition) && rangeOverlayTilemap.GetTile(currentMousePosition) == confirmedTile)
                cursorOverlayTilemap.SetTile(currentMousePosition, selectedTile);
            else if (floorsTilemap.HasTile(currentMousePosition) && rangeOverlayTilemap.GetTile(currentMousePosition) != selectedTile)
                cursorOverlayTilemap.SetTile(currentMousePosition, deniedTile);
        }
        else
        {
            if (floorsTilemap.HasTile(currentMousePosition))
                cursorOverlayTilemap.SetTile(currentMousePosition, hoverTile);
        }

        cursorOverlayTilemap.SetTile(previousMousePosition, null);
    }

    /// <summary>
    /// This method sets the properties of the REALTIME overlay. This will ensure that the correct tiles are shown on the grid depending on the effect the spell has on different units.
    /// </summary>
    /// <param name="overlayType"></param>
    /// <param name="range"></param>
    /// <param name="op"></param>
    public static void DrawRealtimeOverlay(OverlayType overlayType, int range, OverlayProperties op)
    {
        overlayProperties = op;

        realtimeOverlay.isEnabled = true;
        realtimeOverlay.overlayType = overlayType;
        realtimeOverlay.range = range;
    }

    /// <summary>
    /// This method sets the properties of the STATIC overlay. This will ensure that the correct tiles are shown on the grid depending on the effect the spell has on different units.
    /// </summary>
    /// <param name="overlayType"></param>
    /// <param name="range"></param>
    /// <param name="op"></param>
    public static void DrawRangeOverlay(OverlayType overlayType, int range, Vector3Int currentMousePosition, OverlayProperties op, Unit unit = null)
    {
        overlayProperties = op;

        switch (overlayType)
        {
            case OverlayType.Cross:
                DrawCrossOverlay(currentMousePosition, range);
                break;
            case OverlayType.Diamond:
                DrawDiamondOverlay(currentMousePosition, range);
                break;
            case OverlayType.Square:
                DrawSquareOverlay(currentMousePosition, range);
                break;
            case OverlayType.Self:
                DrawSelfOverlay(currentMousePosition);
                break;
            default:
                break;
        }

        if (overlayType != OverlayType.Self && unit == null)
        {
            if (floorsTilemap.HasTile(currentMousePosition))
                DrawRangeOverlay(currentMousePosition);
        }
    }

    /// <summary>
    /// This methods returns all the enemies that are in the Range Overlay, no need to specify a location.
    /// </summary>
    /// <returns></returns>
    public static List<Unit> GetUnitsInRange()
    {
        //List<Unit> units = UnitManager.Instance.GetActiveUnitsFromDictionnary<Unit>();
        List<Unit> returnedUnits = new List<Unit>();

        foreach (Vector3Int item in RangeOverlayTiles)
            foreach (Unit unit in UnitManager.Instance.unitDictionnary.Values)
                //for (int i = 0; i < UnitManager.Instance.unitDictionnary.Values.Count; i++)
                if (unit.positionGrid.Equals(item))
                    returnedUnits.Add(unit);

        return returnedUnits;
    }

    public static Unit GetUnitAtTile(Vector3Int unitPosition)
    {
        return UnitManager.Instance.unitDictionnary.ContainsKey(unitPosition) ? UnitManager.Instance.unitDictionnary[unitPosition] : null;
    }

    /// <summary>
    /// This method is going to draw a "+" overlay according to a certain range.
    /// </summary>
    /// <param name="selectedTilePos"></param>
    /// <param name="range"></param>
    private static void DrawCrossOverlay(Vector3Int selectedTilePos, int range)
    {
        for (int i = 0; i <= range; i++)
        {
            Vector3Int tile = new Vector3Int(selectedTilePos.x + i, selectedTilePos.y, 0);

            if (!RangeOverlayTiles.Contains(tile))
                if (floorsTilemap.HasTile(tile) && !CheckIfWall(tile) || !floorsTilemap.HasTile(tile) && CheckIfPit(tile) || !floorsTilemap.HasTile(tile) && CheckIfVoid(tile))
                    RangeOverlayTiles.Add(tile);
                else
                    break;
        }

        for (int i = 0; i <= range; i++)
        {
            Vector3Int tile = new Vector3Int(selectedTilePos.x, selectedTilePos.y + i, 0);

            if (!RangeOverlayTiles.Contains(tile))
                if (floorsTilemap.HasTile(tile) && !CheckIfWall(tile) || !floorsTilemap.HasTile(tile) && CheckIfPit(tile) || !floorsTilemap.HasTile(tile) && CheckIfVoid(tile))
                    RangeOverlayTiles.Add(tile);
                else
                    break;
        }

        for (int i = 0; i <= range; i++)
        {
            Vector3Int tile = new Vector3Int(selectedTilePos.x - i, selectedTilePos.y, 0);

            if (!RangeOverlayTiles.Contains(tile))
                if (floorsTilemap.HasTile(tile) && !CheckIfWall(tile) || !floorsTilemap.HasTile(tile) && CheckIfPit(tile) || !floorsTilemap.HasTile(tile) && CheckIfVoid(tile))
                    RangeOverlayTiles.Add(tile);
                else
                    break;
        }

        for (int i = 0; i <= range; i++)
        {
            Vector3Int tile = new Vector3Int(selectedTilePos.x, selectedTilePos.y - i, 0);

            if (!RangeOverlayTiles.Contains(tile))
                if (floorsTilemap.HasTile(tile) && !CheckIfWall(tile) || !floorsTilemap.HasTile(tile) && CheckIfPit(tile) || !floorsTilemap.HasTile(tile) && CheckIfVoid(tile))
                    RangeOverlayTiles.Add(tile);
                else
                    break;
        }
    }

    /// <summary>
    /// This method is going to draw a "diamond" shape overlay.
    /// </summary>
    /// <param name="selectedTilePos"></param>
    /// <param name="range"></param>
    private static void DrawDiamondOverlay(Vector3Int selectedTilePos, int range)
    {
        if (range > 0)
        {
            for (int x = 0; x <= range; x++)
            {
                for (int y = 0; y <= range - x; y++)
                {
                    Vector3Int tile = new Vector3Int(selectedTilePos.x + x, selectedTilePos.y + y, 0);

                    if (floorsTilemap.HasTile(tile) && !RangeOverlayTiles.Contains(tile))
                        RangeOverlayTiles.Add(tile);
                }
            }

            for (int x = 0; x <= range; x++)
            {
                for (int y = 0; y <= range - x; y++)
                {
                    Vector3Int tile = new Vector3Int(selectedTilePos.x + x, selectedTilePos.y - y, 0);

                    if (floorsTilemap.HasTile(tile) && !RangeOverlayTiles.Contains(tile))
                    {
                        RangeOverlayTiles.Add(tile);
                    }

                }
            }

            for (int x = 0; x <= range; x++)
            {
                for (int y = 0; y <= range - x; y++)
                {
                    Vector3Int tile = new Vector3Int(selectedTilePos.x - x, selectedTilePos.y + y, 0);

                    if (floorsTilemap.HasTile(tile) && !RangeOverlayTiles.Contains(tile))
                        RangeOverlayTiles.Add(tile);
                }
            }

            for (int x = 0; x <= range; x++)
            {
                for (int y = 0; y <= range - x; y++)
                {
                    Vector3Int tile = new Vector3Int(selectedTilePos.x - x, selectedTilePos.y - y, 0);

                    if (floorsTilemap.HasTile(tile) && !RangeOverlayTiles.Contains(tile))
                        RangeOverlayTiles.Add(tile);
                }
            }
        }
        else
        {
            Vector3Int tile = new Vector3Int(selectedTilePos.x, selectedTilePos.y, 0);

            if (floorsTilemap.HasTile(tile))
                RangeOverlayTiles.Add(tile);
        }
    }

    /// <summary>
    /// This method draws a square overlay.
    /// </summary>
    /// <param name="selectedTilePos"></param>
    /// <param name="range"></param>
    private static void DrawSquareOverlay(Vector3Int selectedTilePos, int range)
    {
        for (int x = 0; x <= range; x++)
        {
            for (int y = 0; y <= range; y++)
            {
                Vector3Int tile = new Vector3Int(selectedTilePos.x + x, selectedTilePos.y + y, 0);

                if (floorsTilemap.HasTile(tile))
                    RangeOverlayTiles.Add(tile);
            }
        }

        for (int x = 0; x <= range; x++)
        {
            for (int y = 0; y <= range; y++)
            {
                Vector3Int tile = new Vector3Int(selectedTilePos.x + x, selectedTilePos.y - y, 0);

                if (floorsTilemap.HasTile(tile))
                    RangeOverlayTiles.Add(tile);
            }
        }

        for (int x = 0; x <= range; x++)
        {
            for (int y = 0; y <= range; y++)
            {
                Vector3Int tile = new Vector3Int(selectedTilePos.x - x, selectedTilePos.y + y, 0);

                if (floorsTilemap.HasTile(tile))
                    RangeOverlayTiles.Add(tile);
            }
        }

        for (int x = 0; x <= range; x++)
        {
            for (int y = 0; y <= range; y++)
            {
                Vector3Int tile = new Vector3Int(selectedTilePos.x - x, selectedTilePos.y - y, 0);

                if (floorsTilemap.HasTile(tile))
                    RangeOverlayTiles.Add(tile);
            }
        }
    }

    /// <summary>
    /// This method draws an overlay tile on the Unit's tile position, this is meant for spells that can be casted on the caster.
    /// </summary>
    /// <param name="selectedTilePos"></param>
    private static void DrawSelfOverlay(Vector3Int selectedTilePos)
    {
        if (floorsTilemap.HasTile(selectedTilePos))
            RangeOverlayTiles.Add(selectedTilePos);

        rangeOverlayTilemap.SetTile(RangeOverlayTiles[0], confirmedTile);
    }

    /// <summary>
    /// All the check methods below are to be used to check if there is a certain type of tiles in the way of the overlay.
    /// </summary>
    /// <param name="tilePosition"></param>
    /// <returns></returns>
    private static bool CheckIfWall(Vector3Int tilePosition)
    {
        bool hasWall = false;

        for (int i = 0; i <= 3; i++)
        {
            Vector3Int wallTile = new Vector3Int(tilePosition.x, tilePosition.y, i);

            if (wallsTilemap.HasTile(wallTile))
            {
                hasWall = true;
                break;
            }
        }

        return hasWall;
    }

    private static bool CheckIfPit(Vector3Int tilePosition)
    {
        bool hasPit = false;

        for (int i = 0; i >= -3; i--)
        {
            Vector3Int pitTile = new Vector3Int(tilePosition.x, tilePosition.y, i);

            if (pitsTilemap.HasTile(pitTile))
            {
                hasPit = true;
                break;
            }
        }

        return hasPit;
    }

    private static bool CheckIfVoid(Vector3Int tilePosition)
    {
        bool hasVoid = false;

        Vector3Int voidTile = new Vector3Int(tilePosition.x, tilePosition.y, 0);

        if (voidTilemap.HasTile(voidTile))
            hasVoid = true;

        return hasVoid;
    }

    /// <summary>
    /// This function actually set the proper tile colors and visually show the tile on the map.
    /// </summary>
    /// <param name="currentMousePosition"></param>
    private static void DrawRangeOverlay(Vector3Int currentMousePosition)
    {
        foreach (Vector3Int t in RangeOverlayTiles)
        {
            cursorOverlayTilemap.SetTile(t, null);

            if (GetUnitAtTile(t) != null && overlayProperties.canUseAbility)
            {
                if (GetUnitAtTile(t) is AIEnemy)
                {
                    rangeOverlayTilemap.SetTile(t, confirmedTile);
                }
                else
                {
                    if (overlayProperties.canCastOnFriendlies)
                        if (overlayProperties.canDamageFriendlies)
                            rangeOverlayTilemap.SetTile(t, warningTile);
                        else
                            rangeOverlayTilemap.SetTile(t, confirmedTile);
                }
            }
            else
            {
                rangeOverlayTilemap.SetTile(t, deniedTile);
            }
        }

        if (!realtimeOverlay.isEnabled && aStar.selected == null)
        {
            rangeOverlayTilemap.SetTile(currentMousePosition, null);
            cursorOverlayTilemap.SetTile(currentMousePosition, deniedTile);
            RangeOverlayTiles.Remove(currentMousePosition);
        }
    }

    public static void ClearTiles()
    {
        foreach (Vector3Int t in PathNodesOverlayTiles)
            pathfindingOverlayTilemap.SetTile(t, null);

        foreach (Vector3Int t in RangeOverlayTiles)
            rangeOverlayTilemap.SetTile(t, null);

        cursorOverlayTilemap.SetTile(InputManager.Instance.previousTileMousePos, null);

        RangeOverlayTiles.Clear();
        PathNodesOverlayTiles.Clear();
    }

    /// <summary>
    /// Returns the Grid Cell position according to the mouse position.
    /// </summary>
    /// <param name="currentMousePos"></param>
    /// <returns></returns>
    public static Vector3Int GetWorldToCellFromMouse(Vector3 currentMousePos)
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(currentMousePos);
        mouseWorldPos.z = 0;
        return grid.WorldToCell(mouseWorldPos);
    }

    /// <summary>
    /// Returns the Grid Cell position from an object placed on the Grid.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public static Vector3Int GetWorldToCellFromTile(Vector3 position)
    {
        position.z = 0;
        return grid.WorldToCell(position);
    }

    public static Vector3 GetTileToCellFromWorld(Vector3Int position)
    {
        Vector3 v = grid.CellToWorld(position) + new Vector3(0, 0.375f, 0);
        return v;
    }

    public static T GetObjectByRay<T>(Vector3 ray) where T : class
    {
        RaycastHit2D hit = Physics2D.Raycast(ray, Vector2.up, 0);

        if (hit)
            return hit.transform.GetComponentInParent<T>();

        return null;
    }

    /// <summary>
    /// This method creates the list of all the AStar pathnodes.
    /// </summary>
    public static void GeneratePathfindingOverlay()
    {
        foreach (Vector3 graphNode in aStar.GraphNodes)
        {
            Vector3Int graphNodePosOnGrid = grid.WorldToCell(graphNode);
            RangeOverlayTiles.Add(graphNodePosOnGrid);
        }
    }

    /// <summary>
    /// This method draws the "realtime" path line when hovering on PathNodes when moving.
    /// </summary>
    public static void DrawCurrentPathOverlay()
    {
        pathfindingOverlayTilemap.ClearAllTiles();
        PathNodesOverlayTiles.Clear();
        // handle movement along path
        aStar.HandlePathNodeSelection();
    }

    /// <summary>
    /// This method actually draws the "blue" tiles on the Tilemap.
    /// </summary>
    public static void DrawCurrentPathNodes()
    {
        foreach (Vector3Int t in PathNodesOverlayTiles)
            pathfindingOverlayTilemap.SetTile(t, confirmedTile);
    }

    /// <summary>
    /// This method calls the AStar move generation method and the GeneratePathfindingOverlay and draws the tiles on the tilemap (Function meant to avoid calling the 3
    /// functions seperately.
    /// </summary>
    public static void ShowPathfinding()
    {
        aStar.GenerateMoves();
        GeneratePathfindingOverlay();
        DrawRangeOverlay(InputManager.Instance.currentTileMousePos);
    }

    public static bool CheckIfNodeIsValid(Vector3Int position)
    {
        if (rangeOverlayTilemap.HasTile(position))
            return true;

        return false;
    }

    public static bool CheckIfPositionIsValid(Vector3Int position)
    {
        return !(CheckIfWall(position) || CheckIfPit(position));
    }

    /// <summary>
    /// This methods checks the Tilemap and builds the list of all the spawnable tiles.
    /// </summary>
    public static void BuildSpawnTilesDict()
    {
        for (int y = spawnPointsTilemap.origin.y; y < (spawnPointsTilemap.origin.y + spawnPointsTilemap.size.y); y++)
        {
            for (int x = spawnPointsTilemap.origin.x; x < (spawnPointsTilemap.origin.x + spawnPointsTilemap.size.x); x++)
            {
                TileBase tile = spawnPointsTilemap.GetTile<TileBase>(new Vector3Int(x, y, 0));

                if (tile != null)
                    if (tile == enemySpawnTile)
                        enemySpawnTiles.Add(new Vector3Int(x, y, 0));
                    else
                        playerSpawnTiles.Add(new Vector3Int(x, y, 0));
            }
        }
    }

    /// <summary>
    /// This method gets a random "Enemy" tile and sends back its position, this is to be used with the UnitManager to get a spawning position for the enemy
    /// </summary>
    /// <returns></returns>
    public static Vector3Int GetRandomEnemySpawnPosition()
    {
        int rndIndex = Random.Range(0, enemySpawnTiles.Count);

        Vector3Int rndSpawnPosition = enemySpawnTiles[rndIndex];

        enemySpawnTiles.Remove(rndSpawnPosition);

        return rndSpawnPosition;
    }

    /// <summary>
    /// This method gets a random "Player" tile and sends back its position, this is to be used with the UnitManager to get a spawning position for the player
    /// </summary>
    /// <returns></returns>
    public static Vector3Int GetRandomPlayerSpawnPosition()
    {
        int rndIndex = Random.Range(0, playerSpawnTiles.Count);

        Vector3Int rndSpawnPosition = playerSpawnTiles[rndIndex];

        playerSpawnTiles.Remove(rndSpawnPosition);

        return rndSpawnPosition;
    }

    /// <summary>
    /// This method ensures that all the AStar node tiles have the right color when showing them. It is called with the event below.
    /// </summary>
    private static void AStarNodesColor()
    {
        foreach (Vector3Int t in RangeOverlayTiles)
            rangeOverlayTilemap.SetTile(t, selectedTile);
    }

    private static void RegisterEvent()
    {
        UnitManager.Instance.aiUnitSelectedEvent += (e) => aStar.Select(e);
        ActionMenu.Instance.moveSelectedEvent += AStarNodesColor;
    }
}
