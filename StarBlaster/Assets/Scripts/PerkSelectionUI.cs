using System.Collections.Generic;
using UnityEngine;

public class PerkSelectionUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject panelContainer; // The visual panel to hide/show
    [SerializeField] Transform cardContainer;   // Where to spawn cards
    [SerializeField] PerkCardUI cardPrefab;     // The card template

    // Start removed to prevent auto-hiding when SetActive(true) calls Start() delayed.

    public void Show()
    {
        // 1. Pause Game
        Time.timeScale = 0f;
        panelContainer.SetActive(true);

        // 2. Clear Old Cards
        foreach (Transform child in cardContainer)
        {
            Destroy(child.gameObject);
        }

        // 3. Get Options from Manager
        List<PerkDataSO> options = PerkManager.Instance.GeneratePerkOptions(3);

        // 4. Spawn Cards
        foreach (var perk in options)
        {
            PerkCardUI card = Instantiate(cardPrefab, cardContainer);
            int nextLevel = PerkManager.Instance.GetPerkLevel(perk.id) + 1;
            card.Setup(perk, nextLevel, this);
        }
    }

    public void OnPerkSelected(PerkDataSO perk)
    {
        // 1. Apply Perk
        PerkManager.Instance.AcquirePerk(perk.id);

        // 2. Resume Game
        panelContainer.SetActive(false);
        Time.timeScale = 1f;

        // 3. Notify WaveManager to continue
        WaveManager.Instance.StartNextWave();
    }
}
