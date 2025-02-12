using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoulderSpawner : MonoBehaviour
{
    [SerializeField] private List<Transform> boulders; // List of boulders to manage
    [SerializeField] private float minRespawnTime = 3f; // Minimum time before a boulder respawns
    [SerializeField] private float maxRespawnTime = 8f; // Maximum time before a boulder respawns

    private List<Vector3> initialPositions = new List<Vector3>();
    private List<Quaternion> initialRotations = new List<Quaternion>();

    private void Start()
    {
        // Store initial positions and rotations
        foreach (Transform boulder in boulders)
        {
            initialPositions.Add(boulder.position);
            initialRotations.Add(boulder.rotation);
        }

        // Start respawn coroutine for each boulder
        foreach (Transform boulder in boulders)
        {
            StartCoroutine(RespawnBoulder(boulder));
        }
    }

    private IEnumerator RespawnBoulder(Transform boulder)
    {
        while (true)
        {
            // Wait for a random amount of time between minRespawnTime and maxRespawnTime
            float respawnDelay = Random.Range(minRespawnTime, maxRespawnTime);
            yield return new WaitForSeconds(respawnDelay);

            // Reset the boulder to its initial position and rotation
            int index = boulders.IndexOf(boulder);
            boulder.position = initialPositions[index];
            boulder.rotation = initialRotations[index];

            // Reset velocity if boulders have Rigidbodies
            Rigidbody rb = boulder.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
}
