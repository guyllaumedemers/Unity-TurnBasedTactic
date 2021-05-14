using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class Tweening
{
    /// <summary>
    /// Button Hoovering scale up the Button Hoovering On
    /// </summary>
    /// <param name="uiElement"></param>
    public static void ButtonHoovering(GameObject uiElement, float factor, float time)
    {
        ButtonFocus(uiElement, factor, time);
    }

    /// <summary>
    /// Button Click simulate a button press, its scale will shrink first than go back to its initial scale
    /// </summary>
    /// <param name="uiElement"></param>
    public static void ButtonClick(GameObject uiElement, float factor, float time)
    {
        ButtonFocus(uiElement, factor * 0.5f, time);
    }

    /// <summary>
    /// Tab fading in and out
    /// </summary>
    /// <param name="canvasGroup"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="animationTime"></param>
    /// <returns></returns>
    public static IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float start, float end, float animationTime)
    {
        float time = 0f;
        while (time < animationTime)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, end, time / animationTime);
            yield return null;
        }
    }

    public static IEnumerator FadeTextOpacity(TextMeshPro text, float start, float end, float animationTime)
    {
        float time = 0f;
        while (time < animationTime)
        {
            time += Time.deltaTime;
            text.alpha = Mathf.Lerp(start, end, time / animationTime);
            yield return null;
        }
    }

    public static IEnumerator OpacityFade(Image img, Color color, float start, float end, float animationTime)
    {
        float time = 0f;
        while (time < animationTime)
        {
            time += Time.deltaTime;
            float value = Mathf.Lerp(start, end, time / animationTime);
            img.color = new Color(color.r, color.g, color.b, value);
            yield return null;
        }
    }

    /// <summary>
    /// Tab moving in the specify direction
    /// </summary>
    /// <param name="uiElement"></param>
    /// <param name="isLeft"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="animationTime"></param>
    /// <returns></returns>
    public static IEnumerator MoveTab(GameObject uiElement, bool isLeft, float start, float end, float animationTime)
    {
        float time = 0f;
        Vector3 newPos = uiElement.transform.position;
        while (time < animationTime)
        {
            time += Time.deltaTime;
            float dir = Mathf.Lerp(start, end, time / animationTime);
            uiElement.transform.position = (isLeft) ? new Vector3(dir, newPos.y, newPos.z) : new Vector3(newPos.x, dir, newPos.z);
            yield return null;
        }
    }

    /// <summary>
    /// Tab scaling in and out Animation
    /// </summary>
    /// <param name="uiElement"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="animationTime"></param>
    /// <returns></returns>
    public static IEnumerator ScaleTab(GameObject uiElement, float start, float end, float animationTime)
    {
        float time = 0f;
        while (time < animationTime)
        {
            time += Time.deltaTime;
            float dir = Mathf.Lerp(start, end, time / animationTime);
            uiElement.transform.localScale = new Vector3(dir, dir, dir);
            yield return null;
        }
    }

    /// <summary>
    /// Lerp Scale in the direction of the arguments. If its positive it scale up, if its negative it scale down
    /// </summary>
    /// <param name="uiElement"></param>
    /// <param name="dir"></param>
    public static void ButtonFocus(GameObject uiElement, float factor, float time)
    {
        Vector3 scale = uiElement.transform.localScale;
        uiElement.transform.localScale = Utilities.VectorLerping(scale, factor, time);
    }

    /// <summary>
    /// Reset the size of the button after ButtonFocus is called
    /// </summary>
    /// <param name="uiElement"></param>
    public static void ButtonFocusReset(GameObject uiElement)
    {
        uiElement.transform.localScale = new Vector3(1, 1, 1);
    }
}

public static class Utilities
{
    /// <summary>
    /// VectorLerping doesnt take into account the zAxis
    /// </summary>
    /// <param name="lerp"></param>
    /// <param name="factor"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    public static Vector3 VectorLerping(Vector3 lerp, float factor, float time)
    {
        float x = Mathf.Lerp(lerp.x, lerp.x *= factor, time);
        float y = Mathf.Lerp(lerp.y, lerp.y *= factor, time);
        return new Vector3(x, y, 1);
    }

    public static Vector3[] WorldSpaceAnchors(RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        return corners;
    }

    public static bool SwitchSpriteRendererDirection(Vector3Int a, Vector3Int b)
    {
        if ((b.x - a.x) < 0) return true;
        return false;
    }
}
