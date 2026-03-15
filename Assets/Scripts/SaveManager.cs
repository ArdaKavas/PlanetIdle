using UnityEngine;
using System;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private const string KEY_PARA = "para";
    private const string KEY_TOPLAM_KAZANC = "toplamKazanc";
    private const string KEY_CIKIS_ZAMANI = "cikisZamani";
    private const string KEY_ADET_PREFIX = "oto_adet_";
    private const string KEY_PASIF_PREFIX = "oto_pasif_";

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void Kaydet(GameManager gm)
    {
        PlayerPrefs.SetString(KEY_PARA, gm.paraMiktari.ToString("R"));
        PlayerPrefs.SetString(KEY_TOPLAM_KAZANC, gm.toplamKazanc.ToString("R"));

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        PlayerPrefs.SetString(KEY_CIKIS_ZAMANI, now.ToString());

        Otomasyon[] otomasyonlar = FindObjectsByType<Otomasyon>(FindObjectsSortMode.None);
        foreach (Otomasyon oto in otomasyonlar)
        {
            PlayerPrefs.SetInt(KEY_ADET_PREFIX + oto.otomasyonIndex, oto.adet);
            PlayerPrefs.SetInt(KEY_PASIF_PREFIX + oto.otomasyonIndex, oto.pasif ? 1 : 0);
        }

        PlayerPrefs.Save();
        Debug.Log("[SaveManager] Kaydedildi.");
    }

    public void Yukle(GameManager gm)
    {
        if (PlayerPrefs.HasKey(KEY_PARA))
        {
            double kaydedilenPara;
            if (double.TryParse(PlayerPrefs.GetString(KEY_PARA), out kaydedilenPara))
                gm.paraMiktari = kaydedilenPara;
            // parse başarısız olursa dokunma, GameManager'daki 10 kalsın
        }
        // KEY_PARA yoksa hiç dokunma, 10 kalsın

        if (PlayerPrefs.HasKey(KEY_TOPLAM_KAZANC))
        {
            double kaydedilenKazanc;
            if (double.TryParse(PlayerPrefs.GetString(KEY_TOPLAM_KAZANC), out kaydedilenKazanc))
                gm.toplamKazanc = kaydedilenKazanc;
        }

        Otomasyon[] otomasyonlar = FindObjectsByType<Otomasyon>(FindObjectsSortMode.None);
        foreach (Otomasyon oto in otomasyonlar)
        {
            int kaydedilmisAdet = PlayerPrefs.GetInt(KEY_ADET_PREFIX + oto.otomasyonIndex, 0);
            bool kaydedilmisPasif = PlayerPrefs.GetInt(KEY_PASIF_PREFIX + oto.otomasyonIndex, 0) == 1;

            oto.AdetYukle(kaydedilmisAdet);
            if (kaydedilmisPasif) oto.PasifAktiflestir();
        }

        Debug.Log($"[SaveManager] Yüklendi. Para: {gm.paraMiktari:F0}");
    }

    public double OfflineKazancHesapla(GameManager gm)
    {
        if (!PlayerPrefs.HasKey(KEY_CIKIS_ZAMANI)) return 0;
        if (!long.TryParse(PlayerPrefs.GetString(KEY_CIKIS_ZAMANI), out long cikisZamani)) return 0;

        long simdi = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long gecenSaniye = Math.Min(simdi - cikisZamani, 28800); // max 8 saat
        if (gecenSaniye <= 0) return 0;

        // Sadece pasif otomasyonlardan gelen kazancı hesapla
        double toplamOffline = 0;
        Otomasyon[] otomasyonlar = FindObjectsByType<Otomasyon>(FindObjectsSortMode.None);
        foreach (Otomasyon oto in otomasyonlar)
        {
            if (!oto.pasif || oto.adet <= 0) continue;

            double kazancPerUretim = oto.bazKazanc * oto.adet;
            float sure = Mathf.Max(oto.bazUretimSuresi * Mathf.Pow(0.95f, oto.adet), 0.1f);
            double uretimSayisi = gecenSaniye / sure;

            toplamOffline += kazancPerUretim * uretimSayisi * 0.5; // %50 verimlilik
        }

        gm.paraMiktari += toplamOffline;
        gm.toplamKazanc += toplamOffline;

        Debug.Log($"[SaveManager] Offline kazanç: {toplamOffline:F0} ({gecenSaniye}s)");
        return toplamOffline;
    }

    public void KaydiSifirla()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("[SaveManager] Kayıt silindi.");
    }
}