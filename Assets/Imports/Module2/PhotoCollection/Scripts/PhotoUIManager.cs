using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
namespace DynamicPhotoCamera
{
    // Controls UI elements and interactions for photo system
    public class PhotoUIManager : MonoBehaviour
{
    // Photo controller reference
    [HideInInspector] public PhotoController photoController;

    public Color32 whiteColor;
    public Color32 halfTransparentColor;

    // Main container for photo collection
    public GameObject photoHolder;
    // Root UI holder transform
    public RectTransform holder;

    // Target position for photo square
    public GameObject targetSquarePos;
    public GameObject temporaltargetSquarePos;
    // Position for plus icon
    public GameObject plusPos;
    // Photo capture square
    public Image photoSquare;
    // Photo capture confirmation
    public Image photoSquareConfirm;

    // Maximum square scale
    private float maxScale = 0.5f;
    // Square growth speed
    private float growSpeed = 10f;
    // Square rotation speed
    private Vector3 rotationSpeed = new Vector3(0, 0, -360);
    // Scale step for rotation
    private Vector3 rotationScaleStep = new Vector3(0.1f, 0.1f, 0.1f);
    // Rotation animation multiplier
    private float rotationIndex = 1;
    // Scale animation multiplier  
    private float scaleIndex = 4f;

    // Square movement flag
    [HideInInspector] public bool moveSquare;
    // Photo capture blocked flag
    public bool cantPhoto;

    private float elapsedTime = 0.0f;
    private bool init;

    // Updates capture square UI 
    public void UpdatePhotoSquare(bool IsClicking)
    {
        HandleSquareMovement();
        HandleSquareScaling(IsClicking);
    }

    // Processes square movement animation
    private void HandleSquareMovement()
    {
        if (!moveSquare) return;

        var squareTransform = photoSquareConfirm.gameObject.transform;
        elapsedTime += Time.deltaTime;
        float t = Mathf.Clamp01(elapsedTime / photoController.inputController.longClickDuration);

        if (t >= photoController.inputController.longClickDuration)
        {
            elapsedTime = 0.0f;
        }
        squareTransform.localScale = Vector3.Lerp(
            squareTransform.localScale,
            Vector3.zero,
            Time.deltaTime * scaleIndex
        );

        if (squareTransform.localScale.x <= 0)
        {
            moveSquare = false;
        }
    }

    // Handles square scale animation
    private void HandleSquareScaling(bool IsClicking)
    {
        var squareTransform = photoSquare.gameObject.transform;

        if (IsClicking && photoController.movingCard == null && !cantPhoto)
        {
            if (squareTransform.localScale.x < maxScale)
            {
                squareTransform.localScale += rotationScaleStep * growSpeed * Time.deltaTime;
                if (photoController.inputController.DeviceType != "Handheld")
                {
                    squareTransform.Rotate(rotationSpeed / rotationIndex * Time.deltaTime);
                }
            }
            else
            {
                squareTransform.localRotation = Quaternion.Lerp(
                    squareTransform.localRotation,
                    Quaternion.Euler(Vector3.zero),
                    Time.deltaTime * growSpeed
                );
            }
        }
        else
        {
            squareTransform.localScale = Vector3.zero;
            squareTransform.localRotation = Quaternion.Lerp(
                squareTransform.localRotation,
                Quaternion.Euler(Vector3.zero),
                Time.deltaTime
            );
        }
    }

    // Resets photo interaction states
    public void ResetPhotos()
    {
        foreach (var card in photoController.allCards)
        {
            if (card.thisImage)
            {
                card.thisImage.raycastTarget = true;
            }
        }
        cantPhoto = false;
        photoController.inputController.IsClicking = false;
    }

    // Initializes capture square
    public void SetSquare()
    {
        var squareTransform = photoSquareConfirm.gameObject.transform;
        squareTransform.position = plusPos.transform.position;
        squareTransform.localScale = new Vector3(maxScale, maxScale, maxScale);
        moveSquare = true;
    }

    // Sets up photo UI elements
    private void SetupPhotoUI(PhotoPrefab photo)
    {
        photo.frame.SetActive(true);
        photo.deleteButton.SetActive(true);
    }

    // Enables photo capture
    public void CanPhotos() => cantPhoto = false;

    // Disables photo capture
    public void CantPhotos()
    {
        photoController.inputController.IsClicking = false;
        cantPhoto = true;
    }
    
    // Disables photo capture
    public void CantPhotos2()
    {
        Debug.Log("CantPhotos2");
        cantPhoto = true;
    }
}
}