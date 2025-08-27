using ShroomClicker;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int multiplierBaseCost = 50;
    public int multiplierMultiplier = 2;

    private bool isSpsRunning;

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

        if (!isSpsRunning)
        {
            StartCoroutine(ShroomsPerSecondCoroutine());
            isSpsRunning = true;
        }
    }

    public void OnClickMultiplier()
    {
        int nextCost = CalculateNextMultiplierCost(data.multiplier);

        if (data.shrooms >= nextCost)
        {
            SaveLoadManager.Instance.data.shrooms -= nextCost;
            SaveLoadManager.Instance.data.multiplier *= multiplierMultiplier;

            SaveLoadManager.Instance.SaveNow();

            UIManager.Instance.UpdateShrooms(SaveLoadManager.Instance.data.shrooms);
            UIManager.Instance.UpdateMultiplierUI(nextCost * multiplierMultiplier);

        }
    }

    int CalculateNextMultiplierCost(int currentMultiplier)
    {
        int purchases = (int)Mathf.Log(currentMultiplier, multiplierMultiplier);
        return multiplierBaseCost * (int)Mathf.Pow(multiplierMultiplier, purchases);
    }
    IEnumerator ShroomsPerSecondCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            SaveLoadManager.Instance.data.shrooms += data.totalSps;

            UIManager.Instance.UpdateShrooms(SaveLoadManager.Instance.data.shrooms);

            SaveLoadManager.Instance.SaveNow();
        }
    }
}
