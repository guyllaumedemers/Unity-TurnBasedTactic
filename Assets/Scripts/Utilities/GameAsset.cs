using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAsset : MonoBehaviour
{
    #region SINGLETON MONOBEHAVIOUR
    private static GameAsset instance;
    private GameAsset() { }
    public static GameAsset Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameAsset>();
                if (instance == null)
                {
                    instance = Instantiate(Resources.Load<GameAsset>("Prefabs/"));
                    Debug.Log(instance.gameObject.name);
                }
            }
            return instance;
        }
    }
    #endregion

    public GameObject damageHitInfo;
}
