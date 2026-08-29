using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerCameraSetup : NetworkBehaviour
{
    public Camera playerCamera;
    public Camera FPV;

    public override void OnNetworkSpawn()
    {
        playerCamera.gameObject.SetActive(IsOwner);
    }
}
