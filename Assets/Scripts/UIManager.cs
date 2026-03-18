using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Money UI")]
    public TextMeshProUGUI moneyText;

    [Header("Offline Earning Popup")]
    public GameObject offlinePanel;         // Inspector'da atanacak (başta kapalı olacak)
    public TextMeshProUGUI offlineMessageText; // "X saatte Y para kazandın!"

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // Popup başlangıçta kapalı
        if (offlinePanel) offlinePanel.SetActive(false);
    }

    void Update()
    {
        if (GameManager.Instance != null)
            moneyText.text = "$" + MoneyFormat(GameManager.Instance.moneyAmount);
    }

    // SaveManager tarafından çağrılır
    public void ShowOfflineEarning(double earning)
    {
        if (offlinePanel == null || offlineMessageText == null) return;

        offlineMessageText.text = "You earned $" + MoneyFormat(earning) + " while you weren't here!";
        offlinePanel.SetActive(true);
    }

    // Popup'taki "Tamam" butonuna bağlanacak
    public void OfflineClosePopup()
    {
        if (offlinePanel) offlinePanel.SetActive(false);
    }

    string MoneyFormat(double amount)
    {
        if (amount >= 1000000000000) return (amount / 1000000000000).ToString("F1") + "T";
        if (amount >= 1000000000) return (amount / 1000000000).ToString("F1") + "B";
        if (amount >= 1000000) return (amount / 1000000).ToString("F1") + "M";
        if (amount >= 1000) return (amount / 1000).ToString("F1") + "K";
        return amount.ToString("F0");
    }
}