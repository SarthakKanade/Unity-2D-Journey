using UnityEngine;

namespace RushRoute.Core
{
    public enum PackageType
    {
        Standard,   // Normal delivery
        Fragile,    // Don't crash
        Hot,        // Timer runs faster
        Heavy,      // Car is slower
        Rolling     // Input is inverted? Or Slippery?
    }

    [CreateAssetMenu(fileName = "NewPackage", menuName = "RushRoute/Package Data")]
    public class PackageData : ScriptableObject
    {
        [Header("Identity")]
        public string PackageName = "Standard Box";
        [TextArea] public string Description = "Just a regular box.";
        public Sprite Icon;

        [Header("Economics")]
        [Tooltip("Base cash earned for delivering this.")]
        public int Payout = 50;

        [Tooltip("Base time added to the clock (or initial time given).")]
        public float TimeToDeliver = 30f;

        [Header("The Twist")]
        public PackageType Type = PackageType.Standard;
        
        [Tooltip("Severity of the twist (e.g. 1.5x damage for Fragile).")]
        public float ModifierStrength = 1.0f;

        private void OnValidate()
        {
            name = PackageName;
        }
    }
}
