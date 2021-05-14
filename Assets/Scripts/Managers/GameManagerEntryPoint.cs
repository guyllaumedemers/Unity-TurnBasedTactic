using UnityEngine;

public class GameManagerEntryPoint : MonoBehaviour
{
    #region SINGLETON MONOBEHAVIOUR
    private static GameManagerEntryPoint instance;
    private GameManagerEntryPoint() { }
    public static GameManagerEntryPoint Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManagerEntryPoint>();
                if (instance == null)
                {
                    GameObject go = new GameObject();
                    instance = go.AddComponent<GameManagerEntryPoint>();
                }
            }
            return instance;
        }
    }
    #endregion

    /// <summary>
    /// Lines that are commented out are not yet implemented
    /// </summary>

    private void Awake()
    {
        InputManager.Instance.PreInitialize();
        InventoryManager.Instance.PreInitialize();
        GridManager.PreInitialize();
        UnitManager.Instance.PreInitialize();
        CameraManager.Instance.PreInitialize();
        MenuManager.Instance.PreInitialize();
        SceneLoader.Instance.PreInitialize();
    }


    private void Start()
    {
        MenuManager.Instance.Initialize();
        UIManager.Instance.Initialize();                // UI needs to be first initialize so it can register the event for the ActionMenu
        OverlayTooltip.Instance.Initialize();
        GridManager.Initialize();
        MapManager.Instance.Initialize();
        BattleTurnManager.Instance.Initialize();
        InputManager.Instance.Initialize();             // Register event to the battle turn manager
        UnitManager.Instance.Initialize();              // depend on the battle turn manager since it register to it
    }

    private void Update()
    {
        GridManager.Refresh();
        InputManager.Instance.Refresh();
        BattleTurnManager.Instance.Refresh();
        CameraManager.Instance.Refresh();
    }
}