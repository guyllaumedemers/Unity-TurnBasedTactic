using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// The Scene Loader is responsible for handling the transition between scenes.
/// It also contains the Coroutine for the "Fade-In/Out" for the screen
/// </summary>
public class SceneLoader : MonoBehaviour
{
    #region SINGLETON MONOBEHAVIOUR
    private static SceneLoader instance;
    private SceneLoader() { }
    public static SceneLoader Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SceneLoader>(true);
                if (instance == null)
                {
                    Debug.LogError("Warning, SceneLoader singleton could not find ActionMenu in scene");
                    GameObject go = new GameObject();
                    go.name = "SceneLoader";
                    instance = go.AddComponent<SceneLoader>();
                }
            }
            return instance;
        }
    }
    #endregion

    GameObject fadeCanvas;
    Image fadeImage;
    int currentSceneIndex;
    private int totalSceneCount;

    public void PreInitialize()
    {
        fadeCanvas = GameObject.FindGameObjectWithTag("FadeCanvas");
        fadeImage = fadeCanvas.GetComponentInChildren<Image>();
        totalSceneCount = SceneManager.sceneCountInBuildSettings;
    }

    /// <summary>
    /// Simple coroutine to fade the screen to black and vice-versa.
    /// </summary>
    /// <param name="fadeToBlack"></param>
    /// <param name="fadeSpeed"></param>
    /// <returns></returns>
    public IEnumerator FadeScreen(bool fadeToBlack = true, float fadeSpeed = 0.25f)
    {
        InputManager.Instance.canUseInputs = false;
        fadeCanvas.SetActive(true);

        Color fadingColor = fadeImage.color;
        float fadeAmount;

        if (fadeToBlack)
        {
            while (fadeImage.color.a < 1)
            {
                fadeAmount = fadingColor.a + (fadeSpeed * Time.deltaTime);
                fadingColor = new Color(fadingColor.r, fadingColor.g, fadingColor.b, fadeAmount);
                fadeImage.color = fadingColor;
                yield return null;
            }
        }
        else
        {
            while (fadeImage.color.a > 0)
            {
                fadeAmount = fadingColor.a - (fadeSpeed * Time.deltaTime);
                fadingColor = new Color(fadingColor.r, fadingColor.g, fadingColor.b, fadeAmount);
                fadeImage.color = fadingColor;
                yield return null;
            }
            fadeCanvas.SetActive(false);
        }
    }

    /// <summary>
    /// This method is used to load the next level. By passing the "playerWin" parameter, it will load the appropriate scene.
    /// If playerWin = true, it will load the next map in the BuildIndex. If false, it will go back to the menu.
    /// Also, if there is no next map, it will also go back to the menu.
    /// </summary>
    /// <param name="playerWin"></param>
    /// <returns></returns>
    public IEnumerator LoadNextScene(bool playerWin = true)
    {
        currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        GridManager.realtimeOverlay.isEnabled = false;
        GridManager.aStar.Deselect();
        GridManager.ClearTiles();
        BattleTurnManager.Instance.ResetTurnDependencies();

        int nextScene = currentSceneIndex + 1;

        yield return new WaitForSeconds(3);
        yield return StartCoroutine(FadeScreen());

        if (playerWin && nextScene < totalSceneCount)
        {
            MapManager.Instance.SavePlayerUnits();
            SceneManager.LoadScene(nextScene);
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }
}
