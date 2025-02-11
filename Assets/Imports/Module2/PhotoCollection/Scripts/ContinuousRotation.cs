using UnityEngine;
namespace DynamicPhotoCamera
{
    /// Rotates game object continuously around selected axes
    public class ContinuousRotation : MonoBehaviour
    {
        [Tooltip("Rotation speed in degrees per second")]
        public float rotationSpeed = 50f;

        [Tooltip("Enable rotation around X axis")]
        public bool axisX;

        [Tooltip("Enable rotation around Y axis")]
        public bool axisY;

        [Tooltip("Enable rotation around Z axis")]
        public bool axisZ;

        private void Update()
        {
            // Calculate rotation for each enabled axis
            Vector3 rotation = new Vector3(
                axisX ? rotationSpeed : 0f,
                axisY ? rotationSpeed : 0f,
                axisZ ? rotationSpeed : 0f
            );

            transform.Rotate(rotation * Time.deltaTime);
        }
    }
}