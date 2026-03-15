using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Para")]
    public double paraMiktari = 10;   // Başlangıç: Madenci satın almaya yeter
    public double paraPerSaniye = 0;  // Artık kullanılmıyor ama SaveManager için kalıyor
    public double toplamKazanc = 0;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        Debug.Log($"Yukle öncesi para: {paraMiktari}");
        SaveManager.Instance.Yukle(this);
        Debug.Log($"Yukle sonrası para: {paraMiktari}");

        double offlineKazanc = SaveManager.Instance.OfflineKazancHesapla(this);
        if (offlineKazanc > 0)
            UIManager.Instance.OfflineKazancGoster(offlineKazanc);
    }

    public void ManuelKazanc(double miktar)
    {
        paraMiktari += miktar;
        toplamKazanc += miktar;
    }

    void OnApplicationPause(bool pause)
    {
        if (pause)
            SaveManager.Instance.Kaydet(this);
    }

    void OnApplicationQuit()
    {
        SaveManager.Instance.Kaydet(this);
    }
}