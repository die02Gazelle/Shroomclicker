using UnityEngine;

public class DevManager : MonoBehaviour
{
    public static DevManager Instance;
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }
}
