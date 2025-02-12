using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DynamicPhotoCamera
{
    /// Controls photo capture, management, and visualization functionality.
    /// Handles screenshot creation, photo storage, and UI interaction.
    public class PhotoController : MonoBehaviour
    {
        #region Component References
        // Main input controller reference
        public InputController inputController;
        // Sound manager
        public AudioController audioManager;
        // UI manager for photo-related elements
        public PhotoUIManager uiManager;
        // Controls cursor camera movement
        public CursorCam pointerOnScreen;
        // Controls cursor for controller users
        public CursorManager cursorManager;

        [Header("UI / Raycasting References")]
        public GraphicRaycaster raycaster;      // Typically on your main Canvas
        public EventSystem eventSystem;         // Standard Unity EventSystem in your scene
        #endregion

        #region Photo Settings
        [HideInInspector] public int howManySprites;
        private GameObject photocard;
        [SerializeField] private GameObject photo;
        private Sprite screenShotSprite;
        private Texture2D screenShot;
        private Texture2D croppedScreenshot;
        private RenderTexture targetTexture;
        #endregion

        #region Capture Settings
        [Header("Capture Settings")]
        [Range(1, 500)]
        [SerializeField] private int cropSize = 100;
        [SerializeField] private RenderTextureFormat renderTextureFormat = RenderTextureFormat.Default;
        [SerializeField] private int depth = 24;

        private int resWidth = 906;
        private int resHeight = 419;
        public float sizeMini = 0.5f;
        public float sizeNormal = 0.7f;
        public float sizeMax = 1f;

        private bool returnDot;
        #endregion

        #region Photo Management
        [HideInInspector] public PhotoPrefab movingCard;   // Photo currently selected
        private List<Vector3> positionsCards;
        public List<PhotoPrefab> allCards;
        private List<int> subsequenceCards;
        private int boardX;
        #endregion

        #region State Variables
        private bool initialReady;
        private float value;

        // NEW: Track whether the cursor is locked or unlocked for UI interaction
        private bool isCursorLocked = true;
        #endregion

        #region Constants
        private GameObject photocardObj;
        private Vector2 mousePosition;
        #endregion

        // Initializes system and loads saved photos
        private IEnumerator Start()
        {
            yield return new WaitForSeconds(0.5f);
            var globalData = PhotoStorageManager.LoadGlobalData();
            howManySprites = globalData.photoCount;

            inputController.photoController = this;
            uiManager.photoController = this;
            cursorManager = FindObjectOfType<CursorManager>(); // Reference to shared cursor manager

            if (!initialReady)
            {
                initialReady = true;
                List<GameObject> toTurnOff = new List<GameObject>();

                subsequenceCards = LoadSequence();

                // EXTENSION POINT: Modify photo loading logic here
                for (int i = 0; i < subsequenceCards.Count; i++)
                {
                    photocard = Instantiate(photo, transform.position, transform.rotation, uiManager.photoHolder.transform);
                    photocard.transform.rotation = Quaternion.identity;
                    photocard.transform.localPosition = Vector3.zero;
                    Sprite loadedSprite = LoadSpriteFromDisk("photo" + subsequenceCards[i]);

                    photocard.GetComponent<Image>().sprite = loadedSprite;
                    PhotoPrefab script = photocard.GetComponent<PhotoPrefab>();
                    if (photocard.GetComponent<Image>().sprite == null)
                    {
                        script.thisNumberIndex = subsequenceCards[i];
                        toTurnOff.Add(photocard);
                    }
                    else
                    {
                        script.thisNumberIndex = subsequenceCards[i];
                        allCards.Add(script);
                        script.photoController = this;
                    }
                }

                if (uiManager.photoHolder.transform.childCount > 0)
                {
                    uiManager.temporaltargetSquarePos =
                        uiManager.photoHolder.transform.GetChild(uiManager.photoHolder.transform.childCount - 1).gameObject;
                }

                for (int i = 0; i < toTurnOff.Count; i++)
                {
                    Destroy(toTurnOff[i].gameObject);
                }

                for (int i = 0; i < allCards.Count; i++)
                {
                    allCards[i].TheStart();
                }

                SortPhotos();
                ShufflePositions();
            }

            uiManager.CanPhotos();

            // By default, lock the cursor at start (if desired).
            LockCursor(true);
        }

        private void Update()
        {
            // 1) Toggle the cursor lock/unlock with the Tab key or Up D-Pad button:
            if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.JoystickButton13))
            {
                isCursorLocked = !isCursorLocked;
                cursorManager.Update();
                LockCursor(isCursorLocked);
            }

            // 2) Check for Y button press to delete the 'movingCard'
            if (Input.GetKeyDown(KeyCode.JoystickButton3)) // Y button on many controllers
            {
                TryDeleteMovingCard();
            }
        }

        /// <summary>
        /// Locks or unlocks the system cursor and visibility.
        /// </summary>
        private void LockCursor(bool shouldLock)
        {
            if (shouldLock)
            {
                Cursor.lockState = CursorLockMode.Locked;
                cursorManager.HideCursor();
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                cursorManager.ShowCursor();
                Cursor.visible = true;
            }
        }

        /// <summary>
        /// Attempts to delete the currently selected (moving) card when Y is pressed.
        /// </summary>
        private void TryDeleteMovingCard()
        {
            if (movingCard != null)
            {
                Image movingCardImage = movingCard.GetComponent<Image>();
                if (movingCardImage != null)
                {
                    DeleteIt(movingCardImage, movingCard);
                }
            }
            else
            {
                Debug.Log("No photo is currently selected for deletion.");
            }
        }

        // Loads sprite from storage
        public Sprite LoadSpriteFromDisk(string fileName, int maxSizeInMB = 10)
        {
            string filePath = Path.Combine(Application.persistentDataPath, fileName);

            if (!File.Exists(filePath))
            {
                return null;
            }

            if (new FileInfo(filePath).Length > maxSizeInMB * 1024 * 1024)
            {
                Debug.LogError($"File {fileName} exceeds {maxSizeInMB}MB limit");
                return null;
            }

            byte[] bytes = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(bytes);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
            return sprite;
        }

        // Removes photo from system
        public void DeleteIt(Image thisObj, PhotoPrefab photo)
        {
            uiManager.CantPhotos();
            uiManager.ResetPhotos();
            uiManager.cantPhoto = true;
            string filePathToDelete = Path.Combine(Application.persistentDataPath, "photo" + photo.thisNumberIndex);
            DeleteFile(filePathToDelete);
            allCards.Remove(photo);
            Destroy(photo.gameObject);
            SortPhotos();
            ShufflePositions();

            movingCard = null;
            Debug.Log("Screenshot deleted.");
        }

        // Removes file from disk
        public void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        // Updates photo order
        public void SortPhotos()
        {
            SaveCurrentSequence(allCards);
        }

        // Retrieves saved photo sequence
        public List<int> LoadSequence()
        {
            var globalData = PhotoStorageManager.LoadGlobalData();
            return globalData.sequence;
        }

        // Stores current photo sequence
        public void SaveCurrentSequence(List<PhotoPrefab> allCards)
        {
            subsequenceCards = new List<int>();
            HashSet<int> seenIndices = new HashSet<int>();

            for (int i = 0; i < allCards.Count; i++)
            {
                int currentIndex = allCards[i].thisNumberIndex;
                if (!seenIndices.Contains(currentIndex))
                {
                    subsequenceCards.Add(currentIndex);
                    seenIndices.Add(currentIndex);
                }
            }

            var globalData = PhotoStorageManager.LoadGlobalData();
            globalData.sequence = subsequenceCards;
            PhotoStorageManager.SaveGlobalData(globalData);
        }

        // Updates photo positions
        public void ShufflePositions()
        {
            for (int i = 0; i < allCards.Count; i++)
            {
                allCards[i].gameObject.transform.SetParent(uiManager.photoHolder.transform);
                allCards[i].gameObject.transform.SetSiblingIndex(allCards[i].thisNumberIndex);
                allCards[i].gameObject.transform.localScale = new Vector3(sizeMini, sizeMini, sizeMini);
            }
        }

        // Creates and saves screenshot
        public void MakeScrenshot(Vector2 mousePosition)
        {
            if (movingCard != null || uiManager.cantPhoto)
                return;

            RenderTexture targetTexture = null;
            try
            {
                // Take screenshot
                resWidth = inputController.currentCamera.pixelWidth;
                resHeight = inputController.currentCamera.pixelHeight;

                screenShot = new Texture2D(resWidth, resHeight);
                targetTexture = RenderTexture.GetTemporary(resWidth, resHeight, depth, renderTextureFormat);

                inputController.currentCamera.targetTexture = targetTexture;
                inputController.currentCamera.Render();
                RenderTexture.active = targetTexture;

                screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
                screenShot.Apply();

                inputController.currentCamera.targetTexture = null;

                // Crop screenshot
                int clickX = Mathf.RoundToInt(mousePosition.x);
                int clickY = Mathf.RoundToInt(mousePosition.y);

                int startX = Mathf.Clamp(clickX - cropSize / 2, 0, Screen.width - cropSize);
                int startY = Mathf.Clamp(clickY - cropSize / 2, 0, Screen.height - cropSize);

                Color[] pixels = screenShot.GetPixels(startX, startY, cropSize, cropSize);
                croppedScreenshot = new Texture2D(cropSize, cropSize);
                croppedScreenshot.SetPixels(pixels);
                croppedScreenshot.Apply();

                // Create sprite and photo card
                screenShotSprite = Sprite.Create(croppedScreenshot, new Rect(0, 0, croppedScreenshot.width, croppedScreenshot.height), new Vector2(0.5f, 0.5f));

                string fileName = "photo" + howManySprites;
                photocardObj = Instantiate(photo, transform.position, transform.rotation, uiManager.photoHolder.transform);
                uiManager.temporaltargetSquarePos =
                    uiManager.photoHolder.transform.GetChild(uiManager.photoHolder.transform.childCount - 1).gameObject;

                PhotoPrefab script = photocardObj.GetComponent<PhotoPrefab>();
                script.thisNumberIndex = howManySprites;
                script.photoController = this;
                script.TheStart();

                if (!allCards.Contains(script))
                    allCards.Add(script);

                photocardObj.transform.localRotation = Quaternion.identity;
                photocardObj.transform.localPosition = Vector3.zero;

                // Update counter and save
                howManySprites++;
                var globalData = PhotoStorageManager.LoadGlobalData();
                globalData.photoCount = howManySprites;
                PhotoStorageManager.SaveGlobalData(globalData);

                SaveSpriteToDisk(screenShotSprite, fileName);
                uiManager.SetSquare();
                SortPhotos();
                inputController.forbiddenInput = true;
                Debug.Log("Screenshot saved to collection.");
                audioManager.PlayRandomSound(audioManager.shotSounds);
            }
            finally
            {
                if (targetTexture != null)
                    RenderTexture.ReleaseTemporary(targetTexture);
            }
        }

        // Stores sprite to disk
        public void SaveSpriteToDisk(Sprite sprite, string fileName)
        {
            Texture2D texture = sprite.texture;
            byte[] bytes = texture.EncodeToPNG();
            string filePath = Path.Combine(Application.persistentDataPath, fileName);
            File.WriteAllBytes(filePath, bytes);
            LoadSpriteFromFile(filePath, photocardObj);
        }

        // Loads sprite from file to photo card
        public void LoadSpriteFromFile(string filePath, GameObject card)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var bytes = new byte[fileStream.Length];
                fileStream.Read(bytes, 0, (int)fileStream.Length);
                Texture2D texture = new Texture2D(1, 1);
                texture.LoadImage(bytes);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                photocardObj.GetComponent<Image>().sprite = sprite;
            }
        }
    }
}
