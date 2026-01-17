using UnityEngine;
using System.Collections.Generic;

namespace RushRoute.Core
{
    public class DeliveryManager : MonoBehaviour
    {
        public static DeliveryManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private List<PackageData> availablePackages;
        [SerializeField] private Transform[] pickupZones;
        [SerializeField] private Transform[] dropoffZones;

        [Header("Time Settings")]
        [Tooltip("Starting time in seconds.")]
        [SerializeField] private float startingTime = 30f;
        
        // Current State
        public bool HasPackage { get; private set; }
        public PackageData CurrentPackage { get; private set; }
        public float TimeRemaining { get; private set; }
        public bool IsGameActive { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            StartShift();
        }

        private void Update()
        {
            if (!IsGameActive) return;

            // Tick Tock
            TimeRemaining -= Time.deltaTime;

            if (TimeRemaining <= 0)
            {
                EndShift();
            }
        }

        public void StartShift()
        {
            TimeRemaining = startingTime;
            IsGameActive = true;
            SpawnNewJob();
        }

        private void EndShift()
        {
            IsGameActive = false;
            TimeRemaining = 0;
            Debug.Log("SHIFT OVER! TIME IS UP!");
            // TODO: Trigger Game Over UI
        }

        public void SpawnNewJob()
        {
            HasPackage = false;
            CurrentPackage = null;

            // 1. Pick a random package type
            if (availablePackages.Count > 0)
            {
                int packageIndex = Random.Range(0, availablePackages.Count);
                CurrentPackage = availablePackages[packageIndex];
            }

            // 2. Activate a random Pickup Zone
            Debug.Log($"New Job! Pickup: {CurrentPackage.PackageName}");
        }

        public void OnPackagePickedUp()
        {
            if (HasPackage) return;

            HasPackage = true;
            Debug.Log("Package Picked Up! Go to Dropoff!");
            // TODO: Show Dropoff Marker
        }

        public void OnPackageDelivered()
        {
            if (!HasPackage) return;

            Debug.Log($"Delivered {CurrentPackage.PackageName}! Earned ${CurrentPackage.Payout}");
            
            // Add Cash
            GameManager.Instance.AddCash(CurrentPackage.Payout);
            
            // Add Time Bonus from Package
            float timeBonus = CurrentPackage.TimeToDeliver; // Simplified: Just add the package's "Time" value as a bonus
            TimeRemaining += timeBonus;
            Debug.Log($"Time Bonus: +{timeBonus}s");
            
            SpawnNewJob();
        }

        public void ReportPackageBroken()
        {
            if (!HasPackage) return;
            
            Debug.Log($"Failed Job: {CurrentPackage.PackageName} was destroyed!");
            
            // Penalty? Maybe lose time?
            TimeRemaining -= 5f; // Lose 5 seconds for breaking it

            SpawnNewJob();
        }
    }
}
