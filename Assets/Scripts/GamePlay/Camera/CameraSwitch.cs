using Unity.Netcode;
using UnityEngine;

public class CameraSwitch : NetworkBehaviour
{
    public static bool SplitViewActive = false;

    public Camera camera1;
    public Camera camera2;

    void Start()
    {
        if (!IsOwner)
        {
            camera1.enabled = false;
            camera2.enabled = false;
            return;
        }

        camera1.enabled = true;
        camera2.enabled = false;
    }

    void Update()
    {
        // Instructor split view is controlling the cameras
        if (SplitViewActive)
            return;

        if (!IsOwner)
            return;

        if (Input.GetKeyDown(KeyCode.C))
        {
            camera1.enabled = !camera1.enabled;
            camera2.enabled = !camera2.enabled;
        }
    }

    public void DisableSplitCamera()
    {
        if (!IsOwner)
        {
            camera1.enabled = false;
            camera2.enabled = false;
        }
    }
}