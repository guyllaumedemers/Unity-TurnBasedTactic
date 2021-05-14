using Globals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class BattleTurnManager : MonoBehaviour
{
    #region SINGELTON MONOBEHAVIOUR
    private static BattleTurnManager instance;
    private BattleTurnManager() { }
    public static BattleTurnManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<BattleTurnManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject();
                    instance = go.AddComponent<BattleTurnManager>();
                }
            }
            return instance;
        }
    }
    #endregion

    // Event Handling
    public delegate void PerformActionEvent();
    public PerformActionEvent performEvent;

    public delegate void EnemySelectedEvent();
    public EnemySelectedEvent enemySelectedEvent;

    public delegate void TrackingTurnEvent();
    public TrackingTurnEvent trackingTurnEvent;

    public delegate void EnableInputs(bool isActive);
    public EnableInputs enableInputs;

    public delegate void EndTurnEvent();
    public EndTurnEvent turnEvent;

    public delegate void DisplayWinnerEvent(bool playerWin);
    public DisplayWinnerEvent displayWinnerEvent;

    public TurnPackageInfo packageInfo;

    public void Initialize()
    {
        RegisterEvents();                                                       // Register Events onStart
        InitializeBattleTurnValues();                                           // Initialize BattleTurn values onStart
        InitializeAudioAndPlay();
        StartCoroutine(StartNewBattleCycle());
    }

    public void Refresh()
    {
        if (packageInfo.UnitSelected != null)
        {
            GridManager.DrawCurrentPathOverlay();
        }
    }

    /// <summary>
    /// Start the battle, once the Turn display complete => it will invoke the SwitchTurn
    /// </summary>
    public IEnumerator StartNewBattleCycle()
    {
        if (Time.timeSinceLevelLoad < 5)
            yield return SceneLoader.Instance.FadeScreen(false, 0.25f);

        GridManager.aStar.Deselect();
        ResetUnitsAPInDictionnary();
        ResetTurnDependencies();
        UIManager.Instance.UpdateTurnDisplay(GetActiveTurn);
    }

    public void EndBattleState()
    {
        displayWinnerEvent.Invoke(UnitManager.Instance.PlayerIsWinning());
    }

    #region EVENT MANAGEMENT

    /// <summary>
    /// Function called by the delegate from the ActiveTurn class
    /// DO NOT CHANGE, IN ORDER TO MATCH THE UI, PLAYERTURNSTATE HAS TO BE INSIDE THE ELSE STATEMENT
    /// </summary>
    private void SwitchTurn()
    {
        GetActiveTurn = !GetActiveTurn;
        if (GetActiveTurn)
            StartCoroutine(EnemyTurnState());
        else
            PlayerTurnState();
    }

    /// <summary>
    /// PlayerTurn start the turn of a single Unit
    /// AT EACH PlayerTurn, the trackingTurnEvent is invoked in order to keep track of the buffs durations
    /// </summary>
    private void PlayerTurnState()
    {
        trackingTurnEvent?.Invoke();
        ResetTurnDependencies();
    }

    /// <summary>
    /// When launching the PlayerTurnState, we should start a coroutine that display the Active Turn
    /// </summary>
    public void ResetTurnDependencies()
    {
        ResetEvent();                                                       // Reset events
        ResetPackageInfo();
    }

    private void PlayerSelected(PlayerUnit playerSelected)
    {
        if (performEvent != null)                                           // check if an event is register, if not continue else break
            return;

        UpdatePackageInfo(playerSelected);                                  // Store PlayerUnit
        LoadUI(playerSelected);                                             // Load PlayerData once to PlayerHUD
        ToggleUI();                                                         // Enable UI
        enableInputs.Invoke(false);                                         // Disable inputs
        GridManager.aStar.Select(playerSelected);
        // Deselection
        InputManager.Instance.deselectEvent = null;
        InputManager.Instance.deselectEvent += () =>                        // Register functions to the deselect Event
        {
            GridManager.aStar.Deselect();                                   // Set Unit null A*
            GridManager.realtimeOverlay.isEnabled = false;
            ToggleUI();                                                     // Hide UI
            enableInputs.Invoke(true);                                      // Enable inputs
            ResetPackageInfo();
        };
    }

    private void ActionSelected(MethodInfo mi, int index)
    {
        ToggleUI();                                                         // Hide UI
        enableInputs.Invoke(true);                                          // Enable inputs
        performEvent += () => { mi.Invoke(null, new object[] { packageInfo.UnitSelected, index, packageInfo.TargetTile }); };
        performEvent += () =>
        {
            AudioManager.Instance.InitializeRoutingAndPlay(AudioManager.Instance.genericClips, GV.channelID_ui, 1, GV.uiFX, false);
        };
        // Deselection
        InputManager.Instance.deselectEvent = null;                         // Reset Event
        InputManager.Instance.deselectEvent += () =>
        {
            GridManager.aStar.Deselect();                                   // Set Unit null A*
            GridManager.realtimeOverlay.isEnabled = false;
            ResetEvent();                                                   // Set Perform Event to null since we deselect the action
            PlayerSelected(packageInfo.UnitSelected as PlayerUnit);
        };
    }

    /// <summary>
    /// Target Selected NEEDS to take into account Distance Checks to prevent processing the invoke of the action registered
    /// </summary>
    /// <param name="target"></param>
    private void TargetSelected(Vector3Int target)
    {
        if (packageInfo.UnitSelected == null || performEvent == null || !GridManager.CheckIfNodeIsValid(target))  // Check if the PlayerUnit is set, if not this event is called too early
            return;
        packageInfo.TargetTile = target;                                    // Set the target tile to move to OR retrieve the target to attack
        performEvent?.Invoke();                                             // Performing the action should trigger the A*
        StartCoroutine(CheckTurnState());
    }

    /// <summary>
    /// The EnemyTurnState needs to wait for the Player last animation to complete before launching its behaviour
    /// When launching the EnemyTurnState, we should start a coroutine that display the Active Turn
    /// </summary>
    private IEnumerator EnemyTurnState()
    {
        enableInputs.Invoke(false);
        while (!UnitManager.Instance.HasCompleteTurn<AIEnemy>())
        {                                                                                           // we have to then wait until the AI make his move and complete his move
            ResetTurnDependencies();
            yield return StartCoroutine(EnemyWaitBeforeTurn());                                     // Select the Enemy with the Highest HP and AP and Run its State Machine
            if (packageInfo.UnitSelected == null)                                                   // safety catch to not update the dictionnary if we have no unit selected
                yield break;
            while (!GridManager.aStar.moveCompleted)
            {
                if (GridManager.aStar.GraphNodes.Count == 0)
                    break;
                yield return null;
            }
            RemoveInactiveEntries();
            if (UnitManager.Instance.HasNoUnitLeft<PlayerUnit>())
            {
                EndBattleState();
                yield break;
            }
            yield return new WaitForSeconds(GV.pauseAnimationTime);
            ((AIEnemy)packageInfo?.UnitSelected).IsItMyTurn = false;
        }
        enableInputs.Invoke(true);
        StartCoroutine(StartNewBattleCycle());                                                                      // EnemyTurn is complete, no AP left bug still alive. resetAP and start PlayerTurn
    }

    private IEnumerator EnemyWaitBeforeTurn()
    {
        yield return new WaitForSeconds(GV.pauseAnimationTime);
        enemySelectedEvent.Invoke();
    }

    private IEnumerator CheckTurnState()
    {
        while (!GridManager.aStar.moveCompleted)
        {
            if (GridManager.aStar.GraphNodes.Count == 0)
                break;
            yield return null;
        }
        RemoveInactiveEntries();                                                                                                    // Remove all entries that have hp <= 0
        if (UnitManager.Instance.HasNoUnitLeft<AIEnemy>())                                                                          // check that the EnemyPlayer has Units left
        {
            EndBattleState();
            yield break;
        }
        if (UnitManager.Instance.HasCompleteTurn<PlayerUnit>())
        {
            yield return new WaitForSeconds(GV.pauseAnimationTime);
            UIManager.Instance.UpdateTurnDisplay(GetActiveTurn);                                    // UpdateTurnDisplay calls the UI Display that show the next Active Player (E or P)
        }
        else
        {
            GridManager.aStar.Deselect();
            GridManager.realtimeOverlay.isEnabled = false;
            GridManager.ClearTiles();
            ResetTurnDependencies();                                                                // Reset Dependencies start a new Turn for the Active player (Player only)
        }
    }
    #endregion

    #region INITIALIZATION VALUES AND UPDATES
    /// <summary>
    /// Reset cycle will be called after the EnemyTurn is complete in order to reset the Unit AP inside the dictionnary
    /// </summary>
    private void ResetUnitsAPInDictionnary()
    {
        UnitManager.Instance.ResetAP();
    }

    /// <summary>
    /// Update State of UI
    /// </summary>
    private void ToggleUI()
    {
        UIManager.Instance.DisplayHUD();
    }

    /// <summary>
    /// Load the Player Data to the PlayerHUD
    /// </summary>
    /// <param name="playerUnit"></param>
    private void LoadUI(PlayerUnit playerUnit)
    {
        UIManager.Instance.LoadHUD(playerUnit);
    }

    /// <summary>
    /// Set the selected Unit to the properties inside the PackageInfo
    /// </summary>
    /// <param name="unit"></param>
    private void UpdatePackageInfo(Unit unit)
    {
        packageInfo.UnitSelected = unit;
        packageInfo.UnitLastPosition = unit.positionGrid;
    }

    /// <summary>
    /// Remove all the entries that have hp <= 0
    /// </summary>
    private void RemoveInactiveEntries()
    {
        UnitManager.Instance.RemoveInactiveUnits();
    }

    private void RegisterEvents()
    {
        // Player Selection
        InputManager.Instance.playerSelectedEvent += PlayerSelected;
        // Tile Selection
        InputManager.Instance.tileSelectedEvent += TargetSelected;
        // Player Target Selection
        InputManager.Instance.playerSelectedEvent += (p) => { TargetSelected(p.positionGrid); };
        // Enemy Selection
        InputManager.Instance.enemySelectedEvent += (e) => { TargetSelected(e.positionGrid); };
        // Actions Selection
        ActionMenu.Instance.actionSelectedEvent += ActionSelected;
        // Active Turn Display
        DisplayActiveTurn.Instance.animationEvent += SwitchTurn;
        // Update dicitonnary for AI
        UnitManager.Instance.aiUnitSelectedEvent += (e) => UpdatePackageInfo(e);
        // End Button Event
        turnEvent += EndTurnButtonPress;
    }

    private void ResetEvent()
    {
        performEvent = null;
        InputManager.Instance.deselectEvent = null;
    }

    private void ResetPackageInfo()
    {
        packageInfo = new TurnPackageInfo();
    }

    private void InitializeBattleTurnValues()
    {
        GetActiveTurn = true;
        ResetPackageInfo();
    }
    #endregion

    public bool GetActiveTurn { get; private set; }

    private void InitializeAudioAndPlay()
    {
        AudioManager.Instance.InitializeRoutingAndPlay(AudioManager.Instance.battleClips, GV.channelID_ambience, 0, GV.ambience, true);
    }

    public void InvokeEndTurnEvent()
    {
        turnEvent.Invoke();
    }

    private void EndTurnButtonPress()
    {
        GridManager.ClearTiles();
        UIManager.Instance.UpdateTurnDisplay(GetActiveTurn);
    }
}

