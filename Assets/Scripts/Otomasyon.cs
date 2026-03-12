using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Otomasyon : MonoBehaviour
{
    [Header("Otomasyon Bilgileri")]
    public string otomasyonAdi = "Madenci";
    public double bazKazanc = 1;
    public double bazFiyat = 10;
    public int adet = 0;
    public bool pasif = false;

    [Header("Üretim Süresi")]
    public float bazUretimSuresi = 4f;      // Inspector'da her otomasyon için ayrı ver
    private float mevcutUretimSuresi;
    private const float MIN_URETIM_SURESI = 0.1f;
    private const float SURE_AZALMA_ORANI = 0.95f; // her satın almada %5 kısalır

    [Header("Kayıt Sistemi")]
    public int otomasyonIndex = 0;

    [Header("UI Referansları")]
    public TextMeshProUGUI adetText;
    public TextMeshProUGUI fiyatText;
    public TextMeshProUGUI kazancText;
    public TextMeshProUGUI sayacText;       // "3.2s" geri sayaç
    public Image progressBar;              // Image Type: Filled olacak
    public Button satinaAlButonu;
    public Button uretimButonu;            // manuel üretim butonu

    private float timer = 0f;

    void Start()
    {
        mevcutUretimSuresi = HesaplaUretimSuresi();
    }

    void Update()
    {
        if (fiyatText) fiyatText.text = "Fiyat: $" + FormatSayi(MevcutFiyat());
        if (kazancText) kazancText.text = "Kazanç: $" + FormatSayi(MevcutKazanc()) + "/üretim";
        if (adetText) adetText.text = "x" + adet;

        if (satinaAlButonu)
            satinaAlButonu.interactable = GameManager.Instance.paraMiktari >= MevcutFiyat();

        // Hiç satın alınmamışsa timer çalışmasın
        if (adet <= 0)
        {
            if (sayacText) sayacText.text = "--";
            if (progressBar) progressBar.fillAmount = 0f;
            if (uretimButonu) uretimButonu.interactable = false;
            return;
        }

        timer += Time.deltaTime;
        float sure = mevcutUretimSuresi;

        // Progress bar
        if (progressBar)
            progressBar.fillAmount = Mathf.Clamp01(timer / sure);

        // Geri sayaç
        if (sayacText)
        {
            float kalan = Mathf.Max(0f, sure - timer);
            sayacText.text = kalan.ToString("F1") + "s";
        }

        if (pasif)
        {
            // Pasif: süre dolunca otomatik üret, döngü devam eder
            if (uretimButonu) uretimButonu.interactable = false;
            if (timer >= sure)
            {
                timer -= sure; // tam sıfır yerine farkı sakla, kayma olmaz
                Uret();
            }
        }
        else
        {
            // Manuel: süre dolunca buton aktif olur
            bool hazir = timer >= sure;
            if (uretimButonu)
            {
                uretimButonu.interactable = hazir;
                var txt = uretimButonu.GetComponentInChildren<TextMeshProUGUI>();
                if (txt) txt.text = hazir ? "ÜRETİM YAP" : "Bekle...";
            }
        }
    }

    // Üretim butonu OnClick bağlantısı
    public void ManuelUret()
    {
        if (!pasif && adet > 0 && timer >= mevcutUretimSuresi)
        {
            timer = 0f;
            Uret();
        }
    }

    public void SatinAl()
    {
        double fiyat = MevcutFiyat();
        if (GameManager.Instance.paraMiktari >= fiyat)
        {
            GameManager.Instance.paraMiktari -= fiyat;
            adet++;
            mevcutUretimSuresi = HesaplaUretimSuresi();
        }
    }

    // SaveManager tarafından çağrılır
    public void AdetYukle(int kaydedilmisAdet)
    {
        adet = kaydedilmisAdet;
        mevcutUretimSuresi = HesaplaUretimSuresi();
    }

    public void PasifAktiflestir()
    {
        if (pasif) return;
        pasif = true;
        if (uretimButonu) uretimButonu.interactable = false;
    }

    // Upgrades sayfasından çağrılacak (şimdilik hazır)
    public void PasifYap()
    {
        if (!pasif && adet >= 10)
            PasifAktiflestir();
    }

    void Uret()
    {
        double kazanc = MevcutKazanc();
        GameManager.Instance.paraMiktari += kazanc;
        GameManager.Instance.toplamKazanc += kazanc;
    }

    float HesaplaUretimSuresi()
    {
        float sure = bazUretimSuresi * Mathf.Pow(SURE_AZALMA_ORANI, adet);
        return Mathf.Max(sure, MIN_URETIM_SURESI);
    }

    double MevcutFiyat()
    {
        return bazFiyat * System.Math.Pow(1.15, adet);
    }

    double MevcutKazanc()
    {
        return bazKazanc * adet;
    }

    string FormatSayi(double sayi)
    {
        if (sayi >= 1000000000000) return (sayi / 1000000000000).ToString("F1") + "T";
        if (sayi >= 1000000000) return (sayi / 1000000000).ToString("F1") + "B";
        if (sayi >= 1000000) return (sayi / 1000000).ToString("F1") + "M";
        if (sayi >= 1000) return (sayi / 1000).ToString("F1") + "K";
        return sayi.ToString("F0");
    }
}