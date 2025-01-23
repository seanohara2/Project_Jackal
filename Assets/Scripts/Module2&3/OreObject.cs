using UnityEngine;

public class OreObject : MonoBehaviour
{
    public void OnPhotoCaptured()
    {
        FindObjectOfType<PlayerStatsManager2>()?.CaptureOrePhoto(gameObject);
    }
}
