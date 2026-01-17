using UnityEngine;

namespace RushRoute.Core
{
    [CreateAssetMenu(fileName = "NewCarData", menuName = "RushRoute/Car Data")]
    public class CarData : ScriptableObject
    {
        [Header("Movement Stats")]
        [Tooltip("Maximum speed of the car.")]
        public float MaxSpeed = 20f;

        [Tooltip("How fast the car accelerates.")]
        public float Acceleration = 10f;

        [Tooltip("How fast the car decelerates when not pressing gas.")]
        public float Drag = 3f;

        [Tooltip("How fast the car turns.")]
        public float TurnSpeed = 200f;

        [Header("Drifting Stats")]
        [Tooltip("0 = No Drift (Train), 1 = Ice. Higher is simpler turning.")]
        [Range(0f, 1f)]
        public float DriftFactor = 0.9f;
        
        [Tooltip("How much speed we lose while drifting sideways.")]
        [Range(0f, 1f)]
        public float TractionControl = 0.8f;

        [Header("Visuals")]
        public Sprite CarSprite;
    }
}
