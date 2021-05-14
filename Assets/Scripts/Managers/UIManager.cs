using UnityEngine;

public class UIManager : MonoBehaviour, Flow
{
    #region SINGLETON
    private static UIManager instance;
    private UIManager() { }
    public static UIManager Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType<UIManager>();
                if (!instance)
                {
                    GameObject go = new GameObject();
                    instance = go.AddComponent<UIManager>();
                }
            }
            return instance;
        }
    }
    #endregion

    #region UPDATE
    /// <summary>
    /// Display the PlayerHUD and ActionMenu according to the current state of a Turn
    /// </summary>
    public void DisplayHUD()
    {
        PlayerHUD.Instance.HUDInteraction();
        UIComponentEndTurn.Instance.DisplayButton();
    }

    /// <summary>
    /// Load the player selected info into the player HUD
    /// CANNOT BE CALLED EVERY TIME THE UI GETS TOGGLE AS IT OFTEN GET SET TO NULL CAUSING ISSUES
    /// </summary>
    /// <param name="playerUnit"></param>
    public void LoadHUD(PlayerUnit playerUnit)
    {
        PlayerHUD.Instance.UpdateUnitInfo(playerUnit);
    }

    /// <summary>
    /// Display the Active Turn Tab and Update its Active Side
    /// </summary>
    public void UpdateTurnDisplay(bool activeTurn)
    {
        StartCoroutine(DisplayActiveTurn.Instance.UpdateTurnDisplay(activeTurn));
    }
    #endregion

    #region INITIALIZATION
    /// <summary>
    /// This toggles the Menu ON or OFF and also stops time and prevents interaction with the game
    /// </summary>
    private GameObject menuCanvas;

    public void ToggleMenu()
    {
        MenuManager.Instance.MenuToggle();

        if (!menuCanvas.activeSelf)
        {
            menuCanvas.SetActive(true);
            Time.timeScale = 0;
        }
        else
        {
            menuCanvas.SetActive(false);
            Time.timeScale = 1;
        }
        
    }

    /// <summary>
    /// Initialize Both PlayerHUD and ActiveTurn UI
    /// </summary>
    private void InitializeUIValues()
    {
        PlayerHUD.Instance.InitializeHUD();
        DisplayWinOrLose.Instance.InitializeUIValues();
        DisplayActiveTurn.Instance.InitializeUIValues();
        DisplayMessageBox.Instance.InitializeUIValues();
        UIComponentEndTurn.Instance.InitializeUIValues();
    }

    /// <summary>
    /// Cache all UI Components
    /// </summary>
    private void ChacheUIComponents()
    {
        PlayerHUD.Instance.CachingUIComponents();
        DisplayWinOrLose.Instance.CachingUIComponents();
        DisplayActiveTurn.Instance.CachingUIComponents();
        DisplayMessageBox.Instance.CachingUIComponents();
        UIComponentEndTurn.Instance.CachingUIComponents();
        menuCanvas = GameObject.FindGameObjectWithTag("Menu");
        menuCanvas.SetActive(false);
    }
    #endregion

    #region FLOW
    public void PreInitialize()
    {
    }

    public void Initialize()
    {
        ChacheUIComponents();
        InitializeUIValues();
    }

    public void Refresh()
    {
    }
    #endregion
}