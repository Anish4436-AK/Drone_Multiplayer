using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpawnManager : NetworkBehaviour
{
    public static SpawnManager Instance { get; private set; }
    public static event Action OnAllDronesSpawned;

    [Header("Drone Prefabs")]
    [SerializeField] private NetworkObject q6DronePrefab;
    [SerializeField] private NetworkObject gaudaDronePrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    private Dictionary<ulong, NetworkObject> spawnedDrones = new();

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnAllPlayers()
    {
        Debug.Log($"SpawnAllPlayers called. IsServer: {IsServer}");

        if (!IsServer)
            return;

        List<PlayerData> trainees =
            SessionManager.Instance.GetTrainees();

        for (int i = 0; i < trainees.Count; i++)
        {
            PlayerData trainee = trainees[i];

            if (i >= spawnPoints.Length)
            {
                Debug.LogError("Not enough spawn points!");
                break;
            }

            NetworkObject selectedDronePrefab = GetDronePrefab(
                trainee.DroneSelected
            );

            if (selectedDronePrefab == null)
            {
                Debug.LogError(
                    $"Invalid drone selection for Client " +
                    $"{trainee.ClientId}: {trainee.DroneSelected}");

                continue;
            }

            Debug.Log(
                $"Spawning Client {trainee.ClientId} | " +
                $"Drone: {trainee.DroneSelected} | " +
                $"Spawn Point: {spawnPoints[i].name}");

            NetworkObject drone = Instantiate(
                selectedDronePrefab,
                spawnPoints[i].position,
                spawnPoints[i].rotation
            );

            drone.SpawnWithOwnership(trainee.ClientId);

            spawnedDrones.Add(trainee.ClientId, drone);
        }

        OnAllDronesSpawned?.Invoke();
    }

    private NetworkObject GetDronePrefab(int droneSelected)
    {
        switch (droneSelected)
        {
            case 0:
                return q6DronePrefab;

            case 1:
                return gaudaDronePrefab;

            default:
                return null;
        }
    }

    public NetworkObject GetDrone(ulong clientId)
    {
        spawnedDrones.TryGetValue(
            clientId,
            out NetworkObject drone
        );

        return drone;
    }
      
    public List<NetworkObject> GetAllDrones()
    {
        return new List<NetworkObject>(spawnedDrones.Values);
    }
}