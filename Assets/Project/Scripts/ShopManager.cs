using ShroomClicker;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    public ShopItem[] shopItems;
    public GameObject shopItemPrefab;
    public Transform scrollViewShop;

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

        StartCoroutine(LoadShopCoroutine());
    }

    IEnumerator LoadShopCoroutine()
    {
        using UnityWebRequest www = UnityWebRequest.Get("https://192.168.178.27/shop");
        www.certificateHandler = new SaveLoadManager.AcceptAllCertificates();
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
            Debug.LogError("Shop Load Error: " + www.error);
        else
        {
            string json = "{ \"items\": " + www.downloadHandler.text + "}";
            ShopItemList loaded = JsonUtility.FromJson<ShopItemList>(json);
            shopItems = loaded.items;

            BuildShopUI();
        }
    }

    void BuildShopUI()
    {
        foreach (Transform child in scrollViewShop)
            Destroy(child.gameObject);

        foreach (var item in shopItems)
        {
            GameObject go = Instantiate(shopItemPrefab, scrollViewShop);
            Button buyButton = go.transform.Find("BuyButton").GetComponent<Button>();
            TMP_Text buttonText = buyButton.GetComponentInChildren<TMP_Text>();
            buttonText.text = $"{item.name}\n{item.price} Shrooms";

            int id = item.id;
            buyButton.onClick.AddListener(() => BuyItem(id));
        }
    }

    public void BuyItem(int itemId)
    {
        var item = shopItems.FirstOrDefault(i => i.id == itemId);
        if (data == null || item == null) return;

        StartCoroutine(BuyItemCoroutine(itemId));
    }

    IEnumerator BuyItemCoroutine(int itemId)
    {
        SaveLoadManager.Instance.SaveNow();

        var item = shopItems.First(i => i.id == itemId);

        if (data.shrooms < item.price) {yield break;}

        // BuyRequest to Server
        BuyRequest req = new BuyRequest { username = SaveLoadManager.Instance.username, itemId = itemId };
        string json = JsonUtility.ToJson(req);

        using UnityWebRequest www = new UnityWebRequest("https://192.168.178.27/buy", "POST")
        {
            uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json)),
            downloadHandler = new DownloadHandlerBuffer()
        };
        www.SetRequestHeader("Content-Type", "application/json");
        www.certificateHandler = new SaveLoadManager.AcceptAllCertificates();

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("BuyItem FAILED: " + www.error);
            yield break;
        }

        // Get BuyResponse
        var result = JsonUtility.FromJson<BuyResponse>(www.downloadHandler.text);

        // Update local Data
        data.shrooms = result.newShrooms;
        data.totalSps = result.totalSps;

        // Update purchasedItems
        var existing = data.purchasedItems?.FirstOrDefault(x => x.itemId == item.id);
        if (existing != null) existing.quantity += 1;
        else
        {
            var list = data.purchasedItems?.ToList() ?? new List<PlayerItemData>();
            list.Add(new PlayerItemData { itemId = item.id, quantity = 1, sps = item.sps });
            data.purchasedItems = list.ToArray();
        }

        UIManager.Instance.UpdateShrooms(data.shrooms);
        UIManager.Instance.UpdateSPS(data.totalSps);

        SaveLoadManager.Instance.SaveNow();
        BuildShopUI();
    }
}
