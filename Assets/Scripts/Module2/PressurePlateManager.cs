using System.Collections.Generic;
using UnityEngine;

public class PressurePlateManager : MonoBehaviour
{
    public static PressurePlateManager Instance;

    [System.Serializable]
    public class LevelData
    {
        public int level; // Level number
        public List<GroupData> groups = new List<GroupData>(); // Groups of pressure plates
        public GameObject finalDoor; // Optional final door for the level (shared door)
        public List<GameObject> activeObjects; // For Level 4: Objects to enable (e.g., platforms)
    }

    [System.Serializable]
    public class GroupData
    {
        public int groupId; // Group ID
        public List<PressurePlate> plates = new List<PressurePlate>();
        public GameObject intermediateDoor; // Optional intermediate door for this group
    }

    public List<LevelData> levels = new List<LevelData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterPlate(PressurePlate plate)
    {
        foreach (var level in levels)
        {
            if (level.level == plate.level)
            {
                foreach (var group in level.groups)
                {
                    if (group.groupId == plate.groupId)
                    {
                        group.plates.Add(plate);
                        return;
                    }
                }
            }
        }
    }

    public void PlateActivated(PressurePlate activatedPlate)
    {
        foreach (var level in levels)
        {
            if (level.level == activatedPlate.level)
            {
                // Check if all plates in a group are activated (intermediate logic)
                foreach (var group in level.groups)
                {
                    if (group.groupId == activatedPlate.groupId)
                    {
                        if (AreAllGroupPlatesActivated(group.plates))
                        {
                            if (group.intermediateDoor != null)
                            {
                                group.intermediateDoor.SetActive(false); // Open intermediate door
                                Debug.Log($"Intermediate door for group {group.groupId} has opened.");
                            }
                        }
                    }
                }

                // Level 4 logic: Enable platforms if all plates in the level are activated
                if (level.level == 4 && AreAllLevelPlatesActivated(level))
                {
                    ActivateObjects(level.activeObjects);
                }
            }
        }

        // Check if all plates across all levels are activated to open the final door
        if (AreAllPlatesActivated())
        {
            OpenFinalDoor();
        }
    }

    private bool AreAllGroupPlatesActivated(List<PressurePlate> plates)
    {
        foreach (var plate in plates)
        {
            if (!plate.IsActivated())
                return false;
        }
        return true;
    }

    private bool AreAllLevelPlatesActivated(LevelData level)
    {
        foreach (var group in level.groups)
        {
            foreach (var plate in group.plates)
            {
                if (!plate.IsActivated())
                    return false;
            }
        }
        return true;
    }

    private bool AreAllPlatesActivated()
    {
        foreach (var level in levels)
        {
            foreach (var group in level.groups)
            {
                foreach (var plate in group.plates)
                {
                    if (!plate.IsActivated())
                        return false;
                }
            }
        }
        return true;
    }

    private void ActivateObjects(List<GameObject> objects)
    {
        foreach (var obj in objects)
        {
            if (obj != null)
            {
                obj.SetActive(true); // Enable platforms or other objects
                Debug.Log($"{obj.name} has been activated.");
            }
        }
    }

    private void OpenFinalDoor()
    {
        foreach (var level in levels)
        {
            if (level.finalDoor != null)
            {
                level.finalDoor.SetActive(false); // Open the shared final door
                Debug.Log("Final door has been opened!");
            }
        }
    }
}
