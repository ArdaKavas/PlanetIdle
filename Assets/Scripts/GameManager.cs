using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Money")]
    public double moneyAmount = 10;   // Başlangıç: Madenci satın almaya yeter
    public double moneyPerSecond = 0;  // Artık kullanılmıyor ama SaveManager için kalıyor
    public double totalEarning = 0;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        Debug.Log($"Money before load: {moneyAmount}");
        SaveManager.Instance.Load(this);
        Debug.Log($"Money after load: {moneyAmount}");

        double offlineEarning = SaveManager.Instance.CalculateOfflineEarning(this);
        if (offlineEarning > 0)
            UIManager.Instance.ShowOfflineEarning(offlineEarning);
    }

    public void ManualEarning(double amount)
    {
        moneyAmount += amount;
        totalEarning += amount;
    }

    void OnApplicationPause(bool pause)
    {
        if (pause)
            SaveManager.Instance.Save(this);
    }

    void OnApplicationQuit()
    {
        SaveManager.Instance.Save(this);
    }
}