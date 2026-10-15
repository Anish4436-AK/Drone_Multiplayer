using System;
using System.Collections.Generic;
using UnityEngine;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }
    public event Action OnPlayersChanged;
    public bool IsSessionStarted { get; private set; }
    public bool IsHost { get; private set; }

    public int SelectedDrone { get; private set; } = -1;

    // Stores every connected participant (Instructor + Trainees)
    private readonly Dictionary<ulong, PlayerData> connectedPlayers = new();


    public int S1ObstacleCount { get; private set; } = 3;
    public int S2ObstacleCount { get; private set; } = 5;
    public int S3ObstacleCount { get; private set; } = 5;

    public Difficulty SelectedDifficulty { get; private set; } = Difficulty.Easy;

    public Terrain SelectedTerrain { get; private set; } = Terrain.Coastal;
       
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetHost(bool isHost)
    {
        IsHost = isHost;
    }

    public void SetSelectedDrone(int selectedDrone)
    {
        SelectedDrone = selectedDrone;

        Debug.Log($"Selected Drone Saved: {SelectedDrone}");
    }
    public void RegisterPlayer(
    ulong clientId,
    PlayerRole role,
    int selectedDrone = -1)
{
    PlayerData playerData = new PlayerData
    {
        ClientId = clientId,
        Role = role,
        DroneSelected = selectedDrone
    };

    connectedPlayers[clientId] = playerData;

    Debug.Log(
        $"Player Registered - Client {clientId} " +
        $"({role}) - Drone: {selectedDrone}");

    OnPlayersChanged?.Invoke();
}

    public void RemovePlayer(ulong clientId)
    {
        if (connectedPlayers.Remove(clientId))
        {
            Debug.Log($"Player Removed : Client {clientId}");
        }
        OnPlayersChanged?.Invoke();
    }

    public PlayerData GetPlayer(ulong clientId)
    {
        connectedPlayers.TryGetValue(clientId, out PlayerData player);
        return player;
    }

    public IReadOnlyDictionary<ulong, PlayerData> GetAllPlayers()
    {
        return connectedPlayers;
    }

    // Returns only trainees (used by the Lobby UI)
    public List<PlayerData> GetTrainees()
    {
        List<PlayerData> trainees = new();

        foreach (PlayerData player in connectedPlayers.Values)
        {
            if (player.Role == PlayerRole.Trainee)
            {
                trainees.Add(player);

            }
        }

        return trainees;
    }

    public void StartSession()
    {
        IsSessionStarted = true;
        Debug.Log("Session Started");
    }

    public void EndSession()
    {
        IsSessionStarted = false;
        connectedPlayers.Clear();

        Debug.Log("Session Ended");
    }

    public enum Difficulty
    {
        Easy, Hard
    }

    public enum Terrain
    {
        Coastal, Desert
    }
    public void SetGameSettings(int s1, int s2, int s3, Difficulty difficulty, Terrain terrain)
    {
        S1ObstacleCount = s1;
        S2ObstacleCount = s2;
        S3ObstacleCount = s3;
        SelectedDifficulty = difficulty;
        SelectedTerrain = terrain;
    }
}