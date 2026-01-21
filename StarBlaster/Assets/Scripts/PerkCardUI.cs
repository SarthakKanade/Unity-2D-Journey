using UnityEngine;
using UnityEngine.UI;
using TMPro;

using UnityEngine.EventSystems; // Required for Hover

public class PerkCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] TextMeshProUGUI descriptionText;
    [SerializeField] TextMeshProUGUI levelText; 
    [SerializeField] TextMeshProUGUI typeText;   // NEW: Survival, Heat, etc
    [SerializeField] TextMeshProUGUI rarityText; // NEW: Restored Rarity
    [SerializeField] Image iconImage;
    [SerializeField] Image cardBackground; 
    [SerializeField] Button selectButton;

    [Header("Animation")]
    [SerializeField] float hoverScale = 1.1f;
    [SerializeField] float animationSpeed = 0.1f;

    PerkDataSO currentPerk;
    PerkSelectionUI uiManager;
    Vector3 originalScale;
    bool isHovering = false;

    void Awake()
    {
        selectButton.onClick.AddListener(OnSelectClicked);
        originalScale = transform.localScale;
    }

    void Update()
    {
        // Smooth Hover Animation
        float targetScale = isHovering ? hoverScale : 1.0f;
        transform.localScale = Vector3.Lerp(transform.localScale, originalScale * targetScale, animationSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
    }

    public void Setup(PerkDataSO perk, int nextLevel, PerkSelectionUI manager)
    {
        currentPerk = perk;
        uiManager = manager;

        // Visuals
        if (titleText != null) titleText.text = perk.displayName;
        if (descriptionText != null) descriptionText.text = perk.GetDescriptionForLevel(nextLevel);
        
        // NEW: Fill Type and Rarity
        if (typeText != null) typeText.text = perk.category.ToString().ToUpper();
        if (rarityText != null) 
        {
            rarityText.text = perk.rarity.ToString();
            rarityText.color = GetRarityColor(perk.rarity);
        }

        // Level Logic
        if (levelText != null)
        {
            if (nextLevel == 1)
            {
                levelText.text = "New!";
                levelText.color = Color.yellow;
            }
            else
            {
                levelText.text = $"Lvl {nextLevel - 1} -> <color=green>{nextLevel}</color>";
                levelText.color = Color.white;
            }
        }

        // Background Color
        if (cardBackground != null) cardBackground.color = GetRarityColor(perk.rarity);
        
        // Icon
        if (iconImage != null)
        {
            iconImage.sprite = perk.icon;
            iconImage.enabled = perk.icon != null;
        }
    }

    void OnSelectClicked()
    {
        if (currentPerk != null) uiManager.OnPerkSelected(currentPerk);
    }

    Color GetRarityColor(PerkRarity rarity)
    {
        switch (rarity)
        {
            case PerkRarity.Common: return Color.white;
            case PerkRarity.Rare: return new Color(0.3f, 0.6f, 1f); // Blue
            case PerkRarity.Epic: return new Color(0.8f, 0.3f, 1f); // Purple
            default: return Color.white;
        }
    }
}
