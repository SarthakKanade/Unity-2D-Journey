using UnityEngine;

public class GameArea : MonoBehaviour
{
    public static GameArea Instance { get; private set; }

    [Header("Screen Configuration")]
    [SerializeField] Camera mainCamera;
    
    // Bounds
    public Vector2 MinBounds { get; private set; }
    public Vector2 MaxBounds { get; private set; }

    // Zone Y-Thresholds (World Space)
    public float BacklineY { get; private set; }  // Top 15%
    public float MidlineY { get; private set; }   // Center 50%
    public float FrontlineY { get; private set; } // Bottom 35% (Above Player Zone)
    
    // DDO: 10% UI, 45% Player, 45% Enemy
    public float PlayerMinY { get; private set; } // The line between UI and Player
    
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        if (mainCamera == null) mainCamera = Camera.main;
        
        InitBounds();
    }

    void InitBounds()
    {
        MinBounds = mainCamera.ViewportToWorldPoint(new Vector2(0, 0));
        MaxBounds = mainCamera.ViewportToWorldPoint(new Vector2(1, 1));
        
        float totalHeight = MaxBounds.y - MinBounds.y;
        
        // 1. UI Zone (Bottom 10%)
        PlayerMinY = MinBounds.y + (totalHeight * 0.10f);

        // 2. Player Zone (Next 45%)
        // Ends at 10% + 45% = 55%
        FrontlineY = MinBounds.y + (totalHeight * 0.55f);
        
        // 3. Enemy Zone (Top 45%)
        // Starts at FrontlineY (55%)
        // Ends at MaxBounds.y (100%)
        
        // --- Enemy Lane Subdivisions ---
        // Enemy Zone Height is 45% of total
        float enemyZoneHeight = totalHeight * 0.45f;
        
        // Backline is top 20% of Enemy Zone
        BacklineY = MaxBounds.y - (enemyZoneHeight * 0.20f); 
        
        // Midline is next 40%? Or specific?
        // Let's just split the remaining space evenly for Mid/Front
        // Frontline Top / Mid Bottom
        MidlineY = MaxBounds.y - (enemyZoneHeight * 0.75f);  
        
        Debug.Log($"Bounds: Top({MaxBounds.y}) | Back({BacklineY}) | Mid({MidlineY}) | Front/PlayerTop({FrontlineY}) | PlayerBot/UITop({PlayerMinY})");
    }
    
    public Vector2 GetRandomPointInZone(PathType zoneType)
    {
        float x = Random.Range(MinBounds.x + 0.5f, MaxBounds.x - 0.5f);
        float y = 0;
        
        switch (zoneType)
        {
            case PathType.Backline:
                y = Random.Range(BacklineY, MaxBounds.y);
                break;
            case PathType.Frontline: // Tank Zone (Bottom of Enemy Area)
                y = Random.Range(FrontlineY, MidlineY);
                break;
            default: // Mid / Grunt
                y = Random.Range(MidlineY, BacklineY); 
                break;
        }
        return new Vector2(x, y);
    }

    private void OnDrawGizmos()
    {
        if (mainCamera == null) return;
        
        // Re-calc specific for Gizmos if not playing
        Vector2 min = mainCamera.ViewportToWorldPoint(new Vector2(0, 0));
        Vector2 max = mainCamera.ViewportToWorldPoint(new Vector2(1, 1));
        float h = max.y - min.y;
        float w = max.x - min.x;
        Vector2 center = (min + max) / 2;

        // Draw UI Zone (Bottom 10%) - Blue
        float uiLine = min.y + (h * 0.10f);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(new Vector3(min.x, uiLine), new Vector3(max.x, uiLine));
        Gizmos.DrawWireCube(new Vector3(center.x, (min.y + uiLine)/2), new Vector3(w, uiLine - min.y));

        // Draw Player Top Boundary (55%) - Green
        float playerTop = min.y + (h * 0.55f);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(min.x, playerTop), new Vector3(max.x, playerTop));

        // Enemy Lanes (Red)
        float eH = h * 0.45f;
        float midLine = max.y - (eH * 0.75f);
        float backLine = max.y - (eH * 0.20f);

        Gizmos.color = Color.yellow; // Mid/Front Split
        Gizmos.DrawLine(new Vector3(min.x, midLine), new Vector3(max.x, midLine));
        
        Gizmos.color = Color.red; // Back/Mid Split
        Gizmos.DrawLine(new Vector3(min.x, backLine), new Vector3(max.x, backLine));
    }
}
