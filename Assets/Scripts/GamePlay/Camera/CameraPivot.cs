using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPivot : MonoBehaviour
{
    public Transform Drone;

    private void LateUpdate()
    {
        float yaw = Drone.eulerAngles.y;
        transform.rotation = Quaternion.Euler(0, yaw, 0);
    }
}
