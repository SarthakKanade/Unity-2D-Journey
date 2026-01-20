using UnityEngine;
using TMPro;

public class UIDisplay : MonoBehaviour
{
    [Header("Wave UI")]
    [SerializeField] TextMeshProUGUI waveText;
    [SerializeField] TextMeshProUGUI quotaText;

    void Update()
    {
        if (WaveManager.Instance != null)
        {
            if(waveText) 
                waveText.text = $"Wave: {WaveManager.Instance.GetCurrentWave()}";
            
            if(quotaText)
            {
                int remaining = WaveManager.Instance.GetRemainingQuota();
                quotaText.text = $"Enemies Left: {remaining}";
            }
        }
    }
}
