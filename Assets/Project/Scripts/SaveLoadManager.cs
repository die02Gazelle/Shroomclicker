using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace ShroomClicker
{
    // Data Structure for purchased Items
    [System.Serializable]
    public class PlayerItemData
    {
        public int itemId;
        public int quantity;
        public int sps;
    }

    // Data Structure for Player data
    [System.Serializable]
    public class ShroomData
    {
        public int shrooms;
        public int multiplier;
        public int totalSps;
        public PlayerItemData[] purchasedItems;
        public string last_login;
    }

    // Request Structure for saving Player Data
    [System.Serializable]
    public class SaveRequest
    {
        public string username;
        public int shrooms;
        public int multiplier;
        public PlayerItemData[] purchasedItems;
        public string last_login;
    }

    // Request structure for buying an item
    [System.Serializable]
    public class BuyRequest
    {
        public string username;
        public int itemId;
    }

    // Response structure for buying an item
    [System.Serializable]
    public class BuyResponse
    {
        public ShopItem item;
        public int newShrooms;
        public int totalSps;
    }

    // Data Structure for Shop Items
    [System.Serializable]
    public class ShopItem
    {
        public int id;
        public string name;
        public int price;
        public int sps;
    }

    // Wrapper for deserializing Shop Item List
    [System.Serializable]
    public class ShopItemList
    {
        public ShopItem[] items;
    }

    public class SaveLoadManager : MonoBehaviour
    {
        public string username = "Justin";
        public static SaveLoadManager Instance;
        public ShroomData data = new ShroomData();

        public delegate void OnDataLoaded(ShroomData data);
        public event OnDataLoaded DataLoaded;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            StartCoroutine(LoadPlayerData(username));
        }

        public void OnDataReady(ShroomData loadedData)
        {
            data = loadedData;

            GameManager.Instance.InitWithData(data);
            UIManager.Instance.InitWithData(data);
            ShopManager.Instance.InitWithData(data);

            StartCoroutine(AutoSave());
        }

        IEnumerator SaveCoroutine()
        {
            SaveRequest reqData = new SaveRequest
            {
                username = username,
                shrooms = data.shrooms,
                multiplier = data.multiplier,
                purchasedItems = data.purchasedItems,
                last_login = System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            };

            string json = JsonUtility.ToJson(reqData);
            using UnityWebRequest www = new UnityWebRequest("https://192.168.178.27/save", "POST")
            {
                uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            www.SetRequestHeader("Content-Type", "application/json");
            www.certificateHandler = new AcceptAllCertificates();

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
                Debug.LogError("Save Error: " + www.error + " | Response: " + www.downloadHandler.text);
        }

        public IEnumerator LoadPlayerData(string username)
        {
            using UnityWebRequest www = UnityWebRequest.Get($"https://192.168.178.27/load/{username}");

            www.certificateHandler = new AcceptAllCertificates();

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Load Error: " + www.error);
                yield break;
            }

            var result = JsonUtility.FromJson<ShroomData>(www.downloadHandler.text);

            // save loaded Data
            data = result;

            // let other Scripts know that Data is ready
            DataLoaded?.Invoke(result);
        }
        void OnEnable() => DataLoaded += OnDataReady;
        void OnDisable() => DataLoaded -= OnDataReady;
        IEnumerator AutoSave() {while (true) {yield return new WaitForSeconds(10f); SaveNow();}}
        public void SaveNow() => StartCoroutine(SaveCoroutine());
        private void OnApplicationQuit() => SaveNow();
        private void OnApplicationPause(bool pauseStatus) {if (pauseStatus) SaveNow();}
        public class AcceptAllCertificates : CertificateHandler
        {
            protected override bool ValidateCertificate(byte[] certificateData) => true;
        }
    }
}
