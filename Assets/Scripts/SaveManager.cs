using UnityEngine;
using System;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private const string KEY_MONEY = "money";
    private const string KEY_TOTAL_EARNING = "totalEarning";
    private const string KEY_EXIT_TIME = "exitTime";
    private const string KEY_NUMBER_PREFIX = "auto_number_";
    private const string KEY_PASSIVE_PREFIX = "auto_passive_";

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void Save(GameManager gm)
    {
        PlayerPrefs.SetString(KEY_MONEY, gm.moneyAmount.ToString("R"));
        PlayerPrefs.SetString(KEY_TOTAL_EARNING, gm.totalEarning.ToString("R"));

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        PlayerPrefs.SetString(KEY_EXIT_TIME, now.ToString());

        Automation[] automations = FindObjectsByType<Automation>(FindObjectsSortMode.None);
        foreach (Automation auto in automations)
        {
            PlayerPrefs.SetInt(KEY_NUMBER_PREFIX + auto.automationIndex, auto.number);
            PlayerPrefs.SetInt(KEY_PASSIVE_PREFIX + auto.automationIndex, auto.passive ? 1 : 0);
        }

        PlayerPrefs.Save();
        Debug.Log("[SaveManager] Saved.");
    }

    public void Load(GameManager gm)
    {
        if (PlayerPrefs.HasKey(KEY_MONEY))
        {
            double savedMoney;
            if (double.TryParse(PlayerPrefs.GetString(KEY_MONEY), out savedMoney))
                gm.moneyAmount = savedMoney;
            // parse başarısız olursa dokunma, GameManager'daki 10 kalsın
        }
        // KEY_PARA yoksa hiç dokunma, 10 kalsın

        if (PlayerPrefs.HasKey(KEY_TOTAL_EARNING))
        {
            double savedEarning;
            if (double.TryParse(PlayerPrefs.GetString(KEY_TOTAL_EARNING), out savedEarning))
                gm.totalEarning = savedEarning;
        }

        Automation[] automations = FindObjectsByType<Automation>(FindObjectsSortMode.None);
        foreach (Automation auto in automations)
        {
            int savedNumber = PlayerPrefs.GetInt(KEY_NUMBER_PREFIX + auto.automationIndex, 0);
            bool savedPassive = PlayerPrefs.GetInt(KEY_PASSIVE_PREFIX + auto.automationIndex, 0) == 1;

            auto.LoadNumber(savedNumber);
            if (savedPassive) auto.ActivatePassive();
        }

        Debug.Log($"[SaveManager] Loaded. Money: {gm.moneyAmount:F0}");
    }

    public double CalculateOfflineEarning(GameManager gm)
    {
        if (!PlayerPrefs.HasKey(KEY_EXIT_TIME)) return 0;
        if (!long.TryParse(PlayerPrefs.GetString(KEY_EXIT_TIME), out long exitTime)) return 0;

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long passedSecond = Math.Min(now - exitTime, 28800); // max 8 saat
        if (passedSecond <= 0) return 0;

        // Sadece pasif otomasyonlardan gelen kazancı hesapla
        double totalOffline = 0;
        Automation[] automations = FindObjectsByType<Automation>(FindObjectsSortMode.None);
        foreach (Automation auto in automations)
        {
            if (!auto.passive || auto.number <= 0) continue;

            double earningPerProduction = auto.baseEarning * auto.number;
            float time = Mathf.Max(auto.baseProductionTime * Mathf.Pow(0.95f, auto.number), 0.1f);
            double productionNumber = passedSecond / time;

            totalOffline += earningPerProduction * productionNumber * 0.5; // %50 verimlilik
        }

        gm.moneyAmount += totalOffline;
        gm.totalEarning += totalOffline;

        Debug.Log($"[SaveManager] Offline earning: {totalOffline:F0} ({passedSecond}s)");
        return totalOffline;
    }

    public void ResetSave()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("[SaveManager] Save deleted.");
    }
}