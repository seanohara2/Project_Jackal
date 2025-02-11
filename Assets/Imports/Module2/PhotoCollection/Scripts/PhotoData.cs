using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;
namespace DynamicPhotoCamera
{
    // Contains data for saving/loading photo states
    [Serializable]
    public class PhotoData
    {
        // Photo's position in UI space
        public Vector3 position;
        // Whether photo is in collection or pinned 
        public string parentType;
        // Index of spawnable object from photo
        public int holdedObjectToCopy;
        // Photo description and metadata
        public string description;
        // Photo rotation in degrees
        public int rotation;
        // Position data identifier
        public string key;
        // Photo content identifier
        public string keyData;

        // Position access property
        public Vector3 Position => position;
        // Parent type access property
        public string ParentType => parentType;
        // Held object access property
        public int HoldedObjectToCopy => holdedObjectToCopy;
        // Description access property
        public string Description => description;
        // Rotation access property
        public int Rotation => rotation;
        // Key access property
        public string Key => key;
        // Key data access property
        public string KeyData => keyData;

        // Creates data from PhotoPrefab
        public PhotoData(PhotoPrefab photo)
        {
            position = photo.transform.localPosition;
            parentType = photo.startParent;
            key = photo.key;
            keyData = photo.keydata;
        }
    }

    // Stores global photo system state
    [Serializable]
    public class GlobalPhotoData
    {
        // Total photos in system
        public int photoCount;
        // Ordered photo index sequence
        public List<int> sequence = new List<int>();
    }

    // Handles photo data persistence
    public static class PhotoStorageManager
    {
        // Gets path for photo data file
        private static string GetPhotoDataPath(int photoIndex) =>
            Path.Combine(Application.persistentDataPath, $"photoData_{photoIndex}.json");

        // Gets path for global data file
        private static string GetGlobalDataPath() =>
            Path.Combine(Application.persistentDataPath, "global_photo_data.json");

        // Saves photo data to JSON file
        public static void SavePhotoData(PhotoPrefab photo)
        {
            var data = new PhotoData(photo);
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(GetPhotoDataPath(photo.thisNumberIndex), json);
        }

        // Loads photo data from file
        public static PhotoData LoadPhotoData(int photoIndex)
        {
            string path = GetPhotoDataPath(photoIndex);
            if (!File.Exists(path)) return null;

            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<PhotoData>(json);
        }

        // Removes photo data file
        public static void DeletePhotoData(int photoIndex)
        {
            string path = GetPhotoDataPath(photoIndex);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        // Saves global system data
        public static void SaveGlobalData(GlobalPhotoData data)
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(GetGlobalDataPath(), json);
        }

        // Loads global system data
        public static GlobalPhotoData LoadGlobalData()
        {
            string path = GetGlobalDataPath();
            if (!File.Exists(path)) return new GlobalPhotoData();

            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<GlobalPhotoData>(json) ?? new GlobalPhotoData();
        }
    }
}