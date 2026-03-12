using UnityEngine;
using UnityEngine.UI;

public class TiklamaButonu : MonoBehaviour
{
    public double tiklamaKazanci = 1;

    public void Tikla()
    {
        GameManager.Instance.ManuelKazanc(tiklamaKazanci);
    }
}