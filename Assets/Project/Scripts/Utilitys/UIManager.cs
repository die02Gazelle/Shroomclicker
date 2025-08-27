using ShroomClicker;
using System.Collections;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public TextMeshProUGUI shroomsText;
    public TextMeshProUGUI spsText;
    public TextMeshProUGUI multiplierText;

    private ShroomData data;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    public void InitWithData(ShroomData initData)
    {
        data = initData;

        UpdateShrooms(data.shrooms);
        UpdateSPS(data.totalSps);
        UpdateMultiplierUI(GameManager.Instance.multiplierBaseCost * (int)Mathf.Pow(GameManager.Instance.multiplierMultiplier, (int)Mathf.Log(data.multiplier, GameManager.Instance.multiplierMultiplier)));
    }

    public void UpdateShrooms(int amount)
    {
        shroomsText.text = "Shrooms: " + amount;
    }

    public void UpdateSPS(int sps)
    {
        spsText.text = sps + " sps";
    }

    public void UpdateMultiplierUI(int nextCost)
    {
        multiplierText.text = $"Multiplikator {data.multiplier}\n{nextCost} Shrooms";
    }
}
