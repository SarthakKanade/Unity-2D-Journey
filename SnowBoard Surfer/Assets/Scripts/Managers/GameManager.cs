using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] float restartDelay = 2f;

    private void Awake()
    {
        // Singleton Pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Optional: DontDestroyOnLoad(gameObject) if we want it to persist between scenes. 
        // For now, simpler to let it reload with the scene or persist depending on design.
        // Let's keep it simple for now.    
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerCrash += HandleCrash;
        GameEvents.OnPlayerFinished += HandleFinish;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerCrash -= HandleCrash;
        GameEvents.OnPlayerFinished -= HandleFinish;
    }

    private void HandleCrash()
    {
        // Future: Show Game Over UI
        Debug.Log("Game Over: Player Crashed");
        Invoke(nameof(ReloadScene), restartDelay);
    }

    private void HandleFinish()
    {
        // Future: Show Win UI
        Debug.Log("Victory: Player Finished");
        Invoke(nameof(ReloadScene), restartDelay);
    }

    private void ReloadScene()
    {
        SceneManager.LoadScene(0);
    }
}
