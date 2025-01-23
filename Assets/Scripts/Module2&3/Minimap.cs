using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    public Transform player;
    [SerializeField] private bool enableCameraRotation = true; // Serialized field to enable or disable rotation

    private void LateUpdate()
    {
        Vector3 newPosition = player.position;
        newPosition.y = transform.position.y;
        transform.position = newPosition;

        if (enableCameraRotation)
        {
            transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
        }
    }
}

//original
/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    public Transform player;

    private void LateUpdate()
    {
        Vector3 newPostion = player.position;
        newPostion.y = transform.position.y;
        transform.position = newPostion;

        transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
    }
}
*/