using Globals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayMessageBox : MonoBehaviour
{
    #region SINGLETON MONOBEHAVIOUR
    private static DisplayMessageBox instance;
    private DisplayMessageBox() { }
    public static DisplayMessageBox Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<DisplayMessageBox>();
                if (instance == null)
                {
                    GameObject go = new GameObject();
                    instance = go.AddComponent<DisplayMessageBox>();
                }
            }
            return instance;
        }
    }
    #endregion

    [Header("Required Components")]
    RectTransform parent;
    RectTransform child;
    
    bool hasScaled;

    public void CachingUIComponents()
    {
        parent = GetComponent<RectTransform>();
        child = parent.GetChild(0).GetComponent<RectTransform>();
    }

    public void InitializeUIValues()
    {
        hasScaled = false;
        child.localScale = Vector3.zero;
    }

    public void DisplayMessageBoxFunc()
    {
        StartCoroutine(Tweening.ScaleTab(child.gameObject, child.localScale.x, hasScaled ? 0 : 1, GV.shortAnimationTime));
        hasScaled = !hasScaled;
    }
}
