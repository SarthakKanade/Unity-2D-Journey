using UnityEngine;

namespace RushRoute.Core
{
    public enum ZoneType
    {
        Pickup,
        Dropoff
    }

    public class DeliveryTrigger : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private ZoneType type;
        
        // Optional: Visuals to show it's active
        private SpriteRenderer _renderer;
        private Color _baseColor;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            if (_renderer != null)
            {
                _baseColor = _renderer.color;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Only the Player can trigger this
            if (other.CompareTag("Player"))
            {
                if (type == ZoneType.Pickup)
                {
                    // Can only pickup if checking logic in Manager permits (e.g., don't have one already)
                    Debug.Log("Player entered Pickup Zone!");
                    DeliveryManager.Instance.OnPackagePickedUp();
                    
                    // Visual feedback: Hide pickup zone?
                    gameObject.SetActive(false); // Quick hack for now
                }
                else if (type == ZoneType.Dropoff)
                {
                    Debug.Log("Player entered Dropoff Zone!");
                    DeliveryManager.Instance.OnPackageDelivered();
                }
            }
        }

        // Optional: Visual Debugging
        private void OnDrawGizmos()
        {
            if (type == ZoneType.Pickup)
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.red;
            }
            
            // Draw a wire cube matching collider size (approx)
            Gizmos.DrawWireCube(transform.position, Vector3.one * 2);
        }
    }
}
