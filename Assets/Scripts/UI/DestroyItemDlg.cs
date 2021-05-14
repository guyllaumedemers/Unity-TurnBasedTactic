using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DestroyItemDlg : MonoBehaviour
{
    private int _index;
    private Image _itemImage;
    private TMP_Text _itemName;

    public IntUnityEvent intEvent = new IntUnityEvent();

    #region SINGLETON MONOBEHAVIOUR
    private static DestroyItemDlg _instance;
    private DestroyItemDlg() { }
    
    public static DestroyItemDlg Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<DestroyItemDlg>();
                if (_instance == null)
                {
                    //GameObject go = new GameObject();
                    //instance = go.AddComponent<DestroyItemDlg>();
                }
            }
            return _instance;
        }
    }
    #endregion


    /// <summary>
    /// Initializes member variables
    /// </summary>
    public void Initialize()
    {
        _itemImage = transform.FindDeepChild("ItemImage").GetComponent<Image>();
        _itemName = transform.FindDeepChild("ItemName").GetComponent<TMP_Text>();
        ResetDialog();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Resets member variables to default values
    /// </summary>
    private void ResetDialog()
    {
        _index = -1;
        _itemImage.sprite = null;
        _itemName.text = string.Empty;
    }

    /// <summary>
    /// Sets dialog information
    /// </summary>
    /// <param name="index">index of item slot</param>
    /// <param name="image">item image</param>
    /// <param name="strname">item name</param>
    public void SetDialog(int index, Texture2D image, string strname)
    {
        _index = index;
        _itemImage.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), new Vector2()); ;
        _itemName.text = strname;
    }

    /// <summary>
    /// Ok button OnClick function
    /// </summary>
    public void OnOkButton()
    {
        intEvent.Invoke(_index);
        OnCancelButton();
    }

    /// <summary>
    /// Cancel button OnClick function
    /// </summary>
    public void OnCancelButton()
    {
        ResetDialog();
        gameObject.SetActive(false);
    }
}