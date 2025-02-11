using UnityEngine;
using UnityEngine.EventSystems;
namespace DynamicPhotoCamera
{
    public class PhotoInputBlocker : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private PhotoController controller;
        public void OnPointerDown(PointerEventData eventData)
        {
            controller.uiManager.CantPhotos();
        }
    }
}
