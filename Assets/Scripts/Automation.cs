using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Automation : MonoBehaviour
{
    [Header("Automation Info")]
    public string automationName = "Miner";
    public double baseEarning = 1;
    public double basePrice = 10;
    public int number = 0;
    public bool passive = false;

    [Header("Production Time")]
    public float baseProductionTime = 4f;      // Inspector'da her otomasyon için ayrı ver
    private float currentProductionTime;
    private const float MIN_PRODUCTION_TIME = 0.1f;
    private const float DURATION_REDUCTION_RATE = 0.95f; // her satın almada %5 kısalır

    [Header("Save System")]
    public int automationIndex = 0;

    [Header("UI References")]
    public TextMeshProUGUI numberText;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI earningText;
    public TextMeshProUGUI timerText;       // "3.2s" geri sayaç
    public Image progressBar;              // Image Type: Filled olacak
    public Button buyButton;
    public Button produceButton;            // manuel üretim butonu

    private float timer = 0f;

    void Start()
    {
        currentProductionTime = CalculateProductionTime();
    }

    void Update()
    {
        if (priceText) priceText.text = "Price: $" + NumberFormat(CurrentPrice());
        if (earningText) earningText.text = "Earning: $" + NumberFormat(CurrentEarning()) + "/production";
        if (numberText) numberText.text = "x" + number;

        if (buyButton)
            buyButton.interactable = GameManager.Instance.moneyAmount >= CurrentPrice();

        // Hiç satın alınmamışsa timer çalışmasın
        if (number <= 0)
        {
            if (timerText) timerText.text = "--";
            if (progressBar) progressBar.fillAmount = 0f;
            if (produceButton) produceButton.interactable = false;
            return;
        }

        timer += Time.deltaTime;
        float time = currentProductionTime;

        // Progress bar
        if (progressBar)
            progressBar.fillAmount = Mathf.Clamp01(timer / time);

        // Geri sayaç
        if (timerText)
        {
            float remainder = Mathf.Max(0f, time - timer);
            timerText.text = remainder.ToString("F1") + "s";
        }

        if (passive)
        {
            // Pasif: süre dolunca otomatik üret, döngü devam eder
            if (produceButton) produceButton.interactable = false;
            if (timer >= time)
            {
                timer -= time; // tam sıfır yerine farkı sakla, kayma olmaz
                Produce();
            }
        }
        else
        {
            // Manuel: süre dolunca buton aktif olur
            bool ready = timer >= time;
            if (produceButton)
            {
                produceButton.interactable = ready;
                var txt = produceButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt) txt.text = ready ? "PRODUCE" : "Wait...";
            }
        }
    }

    // Üretim butonu OnClick bağlantısı
    public void ProduceManually()
    {
        if (!passive && number > 0 && timer >= currentProductionTime)
        {
            timer = 0f;
            Produce();
        }
    }

    public void Buy()
    {
        double price = CurrentPrice();
        if (GameManager.Instance.moneyAmount >= price)
        {
            GameManager.Instance.moneyAmount -= price;
            number++;
            currentProductionTime = CalculateProductionTime();
        }
    }

    // SaveManager tarafından çağrılır
    public void LoadNumber(int savedNumber)
    {
        number = savedNumber;
        currentProductionTime = CalculateProductionTime();
    }

    public void ActivatePassive()
    {
        if (passive) return;
        passive = true;
        if (produceButton) produceButton.interactable = false;
    }

    // Upgrades sayfasından çağrılacak (şimdilik hazır)
    public void MakePassive()
    {
        if (!passive && number >= 10)
            ActivatePassive();
    }

    void Produce()
    {
        double earning = CurrentEarning();
        GameManager.Instance.moneyAmount += earning;
        GameManager.Instance.totalEarning += earning;
    }

    float CalculateProductionTime()
    {
        float time = baseProductionTime * Mathf.Pow(DURATION_REDUCTION_RATE, number);
        return Mathf.Max(time, MIN_PRODUCTION_TIME);
    }

    double CurrentPrice()
    {
        return basePrice * System.Math.Pow(1.15, number);
    }

    double CurrentEarning()
    {
        return baseEarning * number;
    }

    string NumberFormat(double number)
    {
        if (number >= 1000000000000) return (number / 1000000000000).ToString("F1") + "T";
        if (number >= 1000000000) return (number / 1000000000).ToString("F1") + "B";
        if (number >= 1000000) return (number / 1000000).ToString("F1") + "M";
        if (number >= 1000) return (number / 1000).ToString("F1") + "K";
        return number.ToString("F0");
    }
}