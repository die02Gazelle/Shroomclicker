using ShroomClicker;
using UnityEngine;
using System.Collections;

public class DataReadyLoader : MonoBehaviour
{
    public static DataReadyLoader Instance { get; private set; }
    public bool IsReady { get; private set; } = false;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    IEnumerator Start()
    {
        // Warten bis SaveLoadManager & data ready sind
        while (SaveLoadManager.Instance == null || SaveLoadManager.Instance.data == null)
        {
            yield return null;
        }

        IsReady = true;

        OnReady();
    }

    void OnReady()
    {
        var data = SaveLoadManager.Instance.data;

        // Manager initialisieren
        if (UIManager.Instance != null) UIManager.Instance.InitWithData(data);
        if (ShopManager.Instance != null) ShopManager.Instance.InitWithData(data);
        if (GameManager.Instance != null) GameManager.Instance.InitWithData(data);
    }
}
