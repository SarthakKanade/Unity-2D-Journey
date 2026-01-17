using UnityEngine;
using TMPro;

namespace RushRoute.Core
{
    public class UIManager : MonoBehaviour
    {
        [Header("HUD References")]
        [SerializeField] private TMP_Text cashText;
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private TMP_Text notificationText; // For "Delivered!" popups later

        private void Start()
        {
            // Update UI immediately with current state
            UpdateCashUI(GameManager.Instance.CurrentCash);

            // Subscribe to events
            GameManager.Instance.OnCashChanged += UpdateCashUI;
        }
        
        private void Update()
        {
            // Poll for time (simpler than event for continuous values)
            if (DeliveryManager.Instance != null && timeText != null)
            {
                float time = DeliveryManager.Instance.TimeRemaining;
                // Format: 00:00 or just seconds
                timeText.text = $"{Mathf.CeilToInt(time)}s";

                // Visual Panic: Make it red if low
                if (time < 10f) timeText.color = Color.red;
                else timeText.color = Color.white;
            }
        }

        private void OnDestroy()
        {
            // Always unsubscribe to prevent memory leaks!
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnCashChanged -= UpdateCashUI;
            }
        }

        private void UpdateCashUI(int newAmount)
        {
            if (cashText != null)
            {
                cashText.text = $"${newAmount}";
            }
        }
    }
}
