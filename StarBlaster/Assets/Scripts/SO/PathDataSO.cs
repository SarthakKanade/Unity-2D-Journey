using UnityEngine;

[CreateAssetMenu(fileName = "NewPathData", menuName = "Stats/Path Data")]
public class PathDataSO : ScriptableObject
{
    [SerializeField] Transform pathPrefab;
    [SerializeField] PathType pathType;

    public Transform GetPathPrefab() { return pathPrefab; }
    public PathType GetPathType() { return pathType; }

    public Transform GetStartingWaypoint()
    {
        return pathPrefab.GetChild(0);
    }

    public Transform[] GetWaypoints()
    {
        // Cache this if performance issues arise, but for a roguelite spawn it's fine.
        Transform[] waypoints = new Transform[pathPrefab.childCount];
        for (int i = 0; i < pathPrefab.childCount; i++)
        {
            waypoints[i] = pathPrefab.GetChild(i);
        }
        return waypoints;
    }
}
