using System;
using ScriptableObjects.Items;
using UnityEngine;
using UnityEngine.UI;

public class DragedItemInfo : MonoBehaviour
{
    public Item item = null;
    public int dragedItemIndex = -1;
    
    private Image _image;
    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    public void OnEnable()
    {
        _image.color = Color.white;
    }

    public void OnDisable()
    {
        item = null;
        dragedItemIndex = -1;
        _image.color = new Color(255,255,255, 0);
        _image.sprite = null;
        transform.position = Vector2.zero;
    }
}
