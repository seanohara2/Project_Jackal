using UnityEngine;
namespace DynamicPhotoCamera
{
    /// Controls an object that follows cursor or touch input position on screen.
    /// Provides cross-platform support for both desktop and mobile platforms.
    public class CursorCam : MonoBehaviour
    {
        [SerializeField]
        private PhotoController photoController;

        [Tooltip("Offset vector applied to the object's position relative to cursor/touch position")]
        [SerializeField]
        public Vector3 positionOffset = Vector3.zero;

        /// Gets or sets the position offset from cursor/touch position
        public Vector3 PositionOffset
        {
            get => positionOffset;
            set => positionOffset = value;
        }

        /// Gets the associated PhotoController instance
        public PhotoController Controller => photoController;
    }
}