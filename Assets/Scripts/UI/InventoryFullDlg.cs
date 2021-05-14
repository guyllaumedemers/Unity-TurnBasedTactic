using UnityEngine;

public class InventoryFullDlg : MonoBehaviour
{
    #region SINGLETON MONOBEHAVIOUR
    private static InventoryFullDlg _instance;
    private InventoryFullDlg() { }
    
    public static InventoryFullDlg Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<InventoryFullDlg>();
                if (_instance == null)
                {
                    //GameObject go = new GameObject();
                    //instance = go.AddComponent<InventoryFullDlg>();
                }
            }
            return _instance;
        }
    }
    #endregion

    public void Initialize()
    {
        gameObject.SetActive(false);
    }
}
