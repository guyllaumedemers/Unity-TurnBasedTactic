using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Globals;
using TMPro;

public class MenuManager : MonoBehaviour
{
    #region SINGELTON MONOBEHAVIOUR
    private static MenuManager instance;
    private MenuManager() { }
    public static MenuManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<MenuManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject();
                    go.name = "MenuManager";
                    instance = go.AddComponent<MenuManager>();
                }
            }
            return instance;
        }
    }
    #endregion

    public GameObject mainMenu;
    public GameObject settingsMenu;
    public GameObject readmeMenu;
    Resolution[] resolutions;

    public Dropdown resolutionDropdown;
    public Toggle fullScreenToggle;
    public Slider volumeSliderMusic;
    public Slider volumeSliderSFX;

    string playerPrefResolutionKey = "resolution";
    int selectedResolutionIndex;

    string playerPrefVolumeMusicKey = "volumeMusic";
    float playerPrefVolumeMusic;

    string playerPrefVolumeSoundKey = "volumeSound";
    float playerPrefVolumeSound;

    Scene currentScene;
    [SerializeField] TextMeshProUGUI startBtnText;

    GameObject audioManager;

    public void PreInitialize()
    {
        currentScene = SceneManager.GetActiveScene();
        selectedResolutionIndex = PlayerPrefs.GetInt(playerPrefResolutionKey);
        playerPrefVolumeMusic = PlayerPrefs.GetFloat(playerPrefVolumeMusicKey);
        playerPrefVolumeSound = PlayerPrefs.GetFloat(playerPrefVolumeSoundKey);

        AudioManager.Instance.PreInitialize();
    }

    public void Initialize()
    {
        audioManager = GameObject.Find("AudioManager");
        DontDestroyOnLoad(audioManager);

        GetResolutions();
        GetFullScreen();

        if (SceneManager.GetActiveScene().buildIndex != 0)
            startBtnText.text = "Restart";

        InitializeRoutingAndPlay();

        GetVolumeMusic();
        GetVolumeSFX();
    }

    public void PlayGame()
    {
        PlayerPrefs.DeleteKey(GV.bossBattleState);
        SceneManager.LoadScene(1);

        if (Time.timeScale < 1)
            Time.timeScale = 1;
    }

    public void ShowReadme()
    {
        mainMenu.SetActive(false);
        readmeMenu.SetActive(true);
    }

    public void SettingsMenu()
    {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }

    public void Back()
    {
        mainMenu.SetActive(true);

        if (settingsMenu.activeSelf)
        {
            settingsMenu.SetActive(false);
            GetVolumeMusic();
            GetVolumeSFX();
        }
        else
        {
            readmeMenu.SetActive(false);
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void ApplyOptions()
    {
        SetResolution();
        SetFullScreen();
        SaveVolumeMusic();
        SaveVolumeSFX();
    }

    public void MenuToggle()
    {
        mainMenu.SetActive(true);
        settingsMenu.SetActive(false);
        readmeMenu.SetActive(false);
    }

    private void GetResolutions()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();

        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height + " @ " + resolutions[i].refreshRate + " hz";

            options.Add(option);

            if (playerPrefResolutionKey != null)
                currentResolutionIndex = selectedResolutionIndex;
            else if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
                currentResolutionIndex = i;
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    private void GetFullScreen()
    {
        if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow)
            fullScreenToggle.isOn = true;
        else
            fullScreenToggle.isOn = false;
    }

    private void SetResolution()
    {
        Resolution selectedResolution = resolutions[resolutionDropdown.value];
        Screen.SetResolution(selectedResolution.width, selectedResolution.height, Screen.fullScreen);

        PlayerPrefs.SetInt(playerPrefResolutionKey, resolutionDropdown.value);
    }

    private void SetFullScreen()
    {
        if (fullScreenToggle.isOn)
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        else
            Screen.fullScreenMode = FullScreenMode.Windowed;
    }

    public void InitializeRoutingAndPlay()
    {
        if (!AudioManager.Instance.sources[GV.channelID_music].isPlaying)
        {
            AudioManager.Instance.InitializeRoutingAndPlay(AudioManager.Instance.battleClips, GV.channelID_music, 2, GV.backgroundMusic, true);
            AudioManager.Instance.sources[GV.channelID_music].Play();
        }
    }

    public void GetVolumeMusic()
    {
        AudioManager.Instance.GetMusicVolume(out float volume);

        if (playerPrefVolumeMusic != 0)
            volumeSliderMusic.value = playerPrefVolumeMusic;
        else
            volumeSliderMusic.value = volume;
    }

    public void GetVolumeSFX()
    {
        AudioManager.Instance.GetSFXVolume(out float volume);

        if (playerPrefVolumeSound != 0)
            volumeSliderSFX.value = playerPrefVolumeSound;
        else
            volumeSliderSFX.value = volume;
    }

    public void SetVolumeMusic()
    {
        AudioManager.Instance.SetMusicVolume(volumeSliderMusic.value);
    }

    public void SetVolumeSFX()
    {
        AudioManager.Instance.SetSFXVolume(volumeSliderSFX.value);
    }

    public void SaveVolumeMusic()
    {
        playerPrefVolumeMusic = volumeSliderMusic.value;
        PlayerPrefs.SetFloat(playerPrefVolumeMusicKey, playerPrefVolumeMusic);
    }

    public void SaveVolumeSFX()
    {
        playerPrefVolumeSound = volumeSliderSFX.value;
        PlayerPrefs.SetFloat(playerPrefVolumeSoundKey, playerPrefVolumeSound);
    }
}