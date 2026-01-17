using UnityEngine;
using System;

namespace RushRoute.Core
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver,
        Garage
    }

    /// <summary>
    /// Global Manager handling High-Level Game State and Dependencies.
    /// Implements the Singleton pattern.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState CurrentState { get; private set; }

        public event Action<GameState> OnGameStateChanged;

        // --- Economy (Simple for Arc 1) ---
        public int CurrentCash { get; private set; }
        public event Action<int> OnCashChanged;

        private void Awake()
        {
            // Singleton Pattern: Ensure only one GameManager exists
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }

        private void Start()
        {
            // Start in MainMenu state (or Playing if just testing scene)
            ChangeState(GameState.MainMenu);
        }

        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState) return;

            CurrentState = newState;
            Debug.Log($"[GameManager] State Changed to: {newState}");
            
            OnGameStateChanged?.Invoke(newState);
        }

        // Example: Call this when clicking "Play"
        public void StartGame()
        {
            ChangeState(GameState.Playing);
        }

        public void AddCash(int amount)
        {
            CurrentCash += amount;
            Debug.Log($"[GameManager] Cash Added: ${amount}. Total: ${CurrentCash}");
            OnCashChanged?.Invoke(CurrentCash);
        }
    }
}
