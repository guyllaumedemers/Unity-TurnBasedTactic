using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using System;

public class InputManager : MonoBehaviour
{
    #region Singleton
    private static InputManager instance;

    public static InputManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<InputManager>(true);
                if (instance == null)
                {
                    GameObject go = new GameObject("InputManager");
                    instance = go.AddComponent<InputManager>();
                }
            }
            return instance;
        }
    }

    private InputManager() { }

    #endregion

    [Header("Mouse Controls")]
    public Vector3Int currentTileMousePos;
    public Vector3Int previousTileMousePos;
    public Vector3 currentRayMousePos;

    [Header("Input Management")]
    public Keyboard keyboard;
    public Mouse mouse;

    // Event Handling
    public delegate void PlayerSelectedEvent(PlayerUnit playerUnit);
    public PlayerSelectedEvent playerSelectedEvent;

    public delegate void TileSelectedEvent(Vector3Int tilePos);
    public TileSelectedEvent tileSelectedEvent;

    public delegate void EnemySelectedEvent(AIEnemy enemyUnit);
    public EnemySelectedEvent enemySelectedEvent;

    public delegate void DeselectEvent();
    public DeselectEvent deselectEvent;
    // Input State Management
    public bool canUseInputs;

    private bool menuToggle;

    public void PreInitialize()
    {
        keyboard = Keyboard.current;
        mouse = Mouse.current;
        canUseInputs = true;
        menuToggle = false;
    }

    public void Initialize()
    {
        RegisterEvent();
    }

    public void Refresh()
    {
        if (!menuToggle)
            UpdateMousePosition();

        if (canUseInputs && !menuToggle)
            MapOverlay();

        if (keyboard.escapeKey.wasPressedThisFrame && !menuToggle)
            if (BattleTurnManager.Instance.packageInfo.UnitSelected == null)
                UIManager.Instance.ToggleMenu();
            else
                deselectEvent?.Invoke();

        if (mouse.rightButton.wasPressedThisFrame && !menuToggle)                 // Check if you have deselect an Action OR Unit
            deselectEvent?.Invoke();

        if (canUseInputs && mouse.leftButton.wasPressedThisFrame && !menuToggle)   // Check if you have select a Unit OR Tile OR Enemy
            SelectUnit();
    }

    #region DISPLAY PREVIEW OVERLAY
    /// <summary>
    /// Display Hoovering Tile as well as movement Range
    /// </summary>
    private void MapOverlay()
    {
        if (currentTileMousePos != previousTileMousePos)
            GridManager.HoverOnTile(currentTileMousePos, previousTileMousePos);

        previousTileMousePos = currentTileMousePos;
    }

    /// <summary>
    /// Retrieve Tile position from the Mouse in Grid Space
    /// </summary>
    private void UpdateMousePosition()
    {
        currentTileMousePos = GridManager.GetWorldToCellFromMouse(Input.mousePosition);
        currentRayMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    #endregion

    #region PLAYER TURN
    /// <summary>
    /// Unit Selection, Allows to Select PlayerUnit, Empty Tile OR Enemy Unit
    /// </summary>
    private void SelectUnit()
    {
        Vector3Int tile = GridManager.GetWorldToCellFromMouse(Input.mousePosition);
        Unit unit = GridManager.GetUnitAtTile(tile);
        if (unit == null)                                                   // Add a check to prevent from selecting a tile outside the range of the Unit
        {
            tileSelectedEvent?.Invoke(tile);                                // Set Empty Tile Pos
        }
        else
        {
            if (UnitManager.Instance.CheckUnitType<PlayerUnit>(unit))
            {
                playerSelectedEvent?.Invoke(unit as PlayerUnit);            // Set PlayerUnit Active   => but what if the target is also a Player and we want to target to heal?
            }
            else
            {
                enemySelectedEvent?.Invoke(unit as AIEnemy);                // Set Target to Enemy Pos
            }
        }
    }

    #endregion

    #region INPUT STATE MANAGEMENT
    /// <summary>
    /// Enable/Disable Mouse Inputs so you cannot select a tile when the UI is Enable
    /// </summary>
    public void CanUseInputs(bool value)
    {
        canUseInputs = value;
    }

    public void RegisterEvent()
    {
        BattleTurnManager.Instance.enableInputs += (v) => { CanUseInputs(v); };
    }
    #endregion
}