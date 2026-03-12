using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Para UI")]
    public TextMeshProUGUI paraText;

    [Header("Offline Kazanç Popup")]
    public GameObject offlinePanel;         // Inspector'da atanacak (başta kapalı olacak)
    public TextMeshProUGUI offlineMesajText; // "X saatte Y para kazandın!"

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
            paraText.text = "$" + FormatPara(GameManager.Instance.paraMiktari);
    }

    // SaveManager tarafından çağrılır
    public void OfflineKazancGoster(double kazanc)
    {
        if (offlinePanel == null || offlineMesajText == null) return;

        offlineMesajText.text = "Yokken $" + FormatPara(kazanc) + " kazandın!";
        offlinePanel.SetActive(true);
    }

    // Popup'taki "Tamam" butonuna bağlanacak
    public void OfflinePopupKapat()
    {
        if (offlinePanel) offlinePanel.SetActive(false);
    }

    string FormatPara(double miktar)
    {
        if (miktar >= 1000000000000) return (miktar / 1000000000000).ToString("F1") + "T";
        if (miktar >= 1000000000) return (miktar / 1000000000).ToString("F1") + "B";
        if (miktar >= 1000000) return (miktar / 1000000).ToString("F1") + "M";
        if (miktar >= 1000) return (miktar / 1000).ToString("F1") + "K";
        return miktar.ToString("F0");
    }
}