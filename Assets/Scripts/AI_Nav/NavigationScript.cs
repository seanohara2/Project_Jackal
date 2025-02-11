using UnityEngine;
using UnityEngine.AI;

public class NavigationScript : MonoBehaviour
{
    public Transform player;  // Reference to the player
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            // Set the destination to the player's position
            agent.SetDestination(player.position);
        }
        else
        {
            Debug.LogWarning("Agent is not on a NavMesh!");
        }
    }
}
