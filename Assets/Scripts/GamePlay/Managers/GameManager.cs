using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    private void Start()
    {
        if (!IsServer)
            return;

        SpawnManager.Instance.SpawnAllPlayers();
    }
}