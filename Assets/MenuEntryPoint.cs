using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuEntryPoint : MonoBehaviour
{
    private void Awake()
    {
        MenuManager.Instance.PreInitialize();
    }

    private void Start()
    {
        MenuManager.Instance.Initialize();
    }
}
