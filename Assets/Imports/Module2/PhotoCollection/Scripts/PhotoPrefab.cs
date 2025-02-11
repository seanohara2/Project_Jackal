using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Collections.ObjectModel;
namespace DynamicPhotoCamera
{
    // Handles individual photo functionality and interactions
    public class PhotoPrefab : MonoBehaviour
    {
        #region References
        // Reference to main photo controller
        public PhotoController photoController;
        // Reference to this photo's image component
        public Image thisImage;

        // Starting position in UI space
        public Vector3 startPos;
        // Parent container type (collection/desk)
        public string startParent;
        // Index identifier for this photo
        public int thisNumberIndex;
        // Position data key
        public string key;
        // Content data key
        public string keydata;

        // Photo frame UI element
        public GameObject frame;
        // Delete button UI element 
        public GameObject deleteButton;

        #endregion

        // Initializes photo components
        public void TheStart()
        {
            InitializeKeys();
            LoadPhotoData();
            RegisterWithController();
            SetInitialScale();
            this.enabled = false;
        }

        // Sets up data keys
        private void InitializeKeys()
        {
            key = "photopos" + thisNumberIndex;
            keydata = "DataKey" + thisNumberIndex;
        }

        // Loads saved photo data
        private void LoadPhotoData()
        {
            var data = PhotoStorageManager.LoadPhotoData(thisNumberIndex);
            if (data != null)
            {
                ApplyPhotoData(data);
            }
            else
            {
                startParent = "collection";
            }
        }

        // Applies loaded data to photo
        private void ApplyPhotoData(PhotoData data)
        {
            startParent = data.ParentType;
            startParent = "collection";
        }

        // Adds photo to controller list
        private void RegisterWithController()
        {
            if (!photoController.allCards.Contains(this))
            {
                photoController.allCards.Add(this);
            }
        }

        // Sets initial photo scale
        private void SetInitialScale()
        {
            transform.localScale = new Vector3(photoController.sizeMini, photoController.sizeMini, photoController.sizeMini);
        }

        // Handles photo click event
        public void ClickIt(Image thisObj)
        {
            thisImage.color = photoController.uiManager.halfTransparentColor;
            transform.SetParent(photoController.uiManager.targetSquarePos.transform);
            InitiateDragMode(thisObj);
        }

        // Sets up drag mode
        private void InitiateDragMode(Image thisObj)
        {
            DisableOtherCards();
            SetupForDragging();
            startPos = transform.localPosition;
            photoController.movingCard = this;
            this.enabled = true;
            photoController.audioManager.PlayRandomSound(photoController.audioManager.dragSounds);
        }

        // Disables interaction with other photos
        private void DisableOtherCards()
        {
            foreach (var card in photoController.allCards)
            {
                if (card.thisImage)
                {
                    card.thisImage.raycastTarget = false;
                }
                card.transform.localScale = card.GetAppropriateScale();
            }
        }

        // Gets correct scale based on state
        private Vector3 GetAppropriateScale()
        {
            float scale = photoController.sizeMini;
            return new Vector3(scale, scale, scale);
        }

        // Prepares photo for dragging
        private void SetupForDragging()
        {
            transform.localScale = new Vector3(photoController.sizeMax, photoController.sizeMax, photoController.sizeMax);
        }

        // Updates position while moving
        public void MoveIt(Vector3 inputPos)
        {
            inputPos.z = 0;
            transform.position = inputPos;
        }

        // Removes photo from system
        public void Delete()
        {
            photoController.audioManager.PlayRandomSound(photoController.audioManager.clickSounds);
            photoController.allCards.Remove(this);
            PhotoStorageManager.DeletePhotoData(thisNumberIndex);
            photoController.DeleteIt(thisImage, this);
        }

        // Saves current position
        public void SavePosition()
        {
            PhotoStorageManager.SavePhotoData(this);
        }

        // Handles end of drag operation
        public void LetItGo(Vector3 inputPos)
        {
            HandlePlacement();
        }

        // Handles photo placement
        private void HandlePlacement()
        {
            transform.SetParent(photoController.uiManager.holder);
            transform.SetSiblingIndex(transform.parent.childCount - 1);
            FinalizePhotoPlacement();
        }

        // Finalizes photo placement
        private void FinalizePhotoPlacement()
        {
            photoController.audioManager.PlayRandomSound(photoController.audioManager.dropSounds);
            thisImage.color = photoController.uiManager.whiteColor;
            transform.localScale = GetAppropriateScale();
            SavePosition();
            photoController.SortPhotos();
            photoController.ShufflePositions();
            photoController.movingCard = null;
            this.enabled = false;
        }
    }
}