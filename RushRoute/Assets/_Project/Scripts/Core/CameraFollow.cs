using UnityEngine;

namespace RushRoute.Core
{
    public class CameraFollow : MonoBehaviour
    {
        [Tooltip("The thing we want to look at (The Car).")]
        [SerializeField] private Transform target;

        [Tooltip("How high is the camera? (-10 is standard for 2D).")]
        [SerializeField] private Vector3 offset = new Vector3(0, 0, -10f);

        [Tooltip("How smooth the camera moves (0 = Instant, 1 = Very Laggy).")]
        [Range(0, 1)]
        [SerializeField] private float smoothSpeed = 0.125f;

        private void LateUpdate()
        {
            if (target == null) return;

            // Where do we want to be?
            Vector3 desiredPosition = target.position + offset;

            // Smoothly go there
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }
    }
}
