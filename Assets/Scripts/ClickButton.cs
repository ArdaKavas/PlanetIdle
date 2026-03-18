using UnityEngine;
using UnityEngine.UI;

public class ClickButton : MonoBehaviour
{
    public double clickEarning = 1;

    public void Click()
    {
        GameManager.Instance.ManualEarning(clickEarning);
    }
}