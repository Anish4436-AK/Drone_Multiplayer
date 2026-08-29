using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class SpectatorManager : MonoBehaviour
{
    public static SpectatorManager Instance;

    [Header("References")]
    [SerializeField] private Camera spectatorCamera;
    [SerializeField] private TMP_Dropdown playerDropdown;
    [SerializeField] private GameObject instructorPanel;
    [SerializeField] private TelemetryUI telemetryUI;

    private readonly List<ulong> traineeIds = new();
    private Transform currentTarget;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        bool isHost = NetworkManager.Singleton.IsHost;

        spectatorCamera.gameObject.SetActive(isHost);
        instructorPanel.SetActive(isHost);

        if (!isHost)
            return;

        

        playerDropdown.onValueChanged.AddListener(OnPlayerSelected);
    }


    private void LateUpdate()
    {
        if (currentTarget == null)
            return;

        spectatorCamera.transform.SetPositionAndRotation(
            currentTarget.position,
            currentTarget.rotation);
    }

    void PopulateDropdown()
    {
        playerDropdown.ClearOptions();
        traineeIds.Clear();

        List<string> options = new();

        List<PlayerData> trainees = SessionManager.Instance.GetTrainees();

        for (int i = 0; i < trainees.Count; i++)
        {
            traineeIds.Add(trainees[i].ClientId);

            options.Add("Drone " + (i + 1));
        }

        playerDropdown.AddOptions(options);

        if (options.Count > 0)
        {
            OnPlayerSelected(0);
        }
    }

    void OnPlayerSelected(int index)
    {
        if (index >= traineeIds.Count)
            return;

        ulong clientId = traineeIds[index];

        NetworkObject drone = SpawnManager.Instance.GetDrone(clientId);

        if (drone == null)
            return;

        DroneCameraTarget target = drone.GetComponent<DroneCameraTarget>();

        if (target == null)
            return;

        currentTarget = target.FollowTarget;

        // Update instructor telemetry to selected drone
        DroneTelemetry droneTelemetry =
            drone.GetComponent<DroneTelemetry>();

        if (droneTelemetry != null && telemetryUI != null)
        {
            telemetryUI.SetTargetDrone(droneTelemetry);

            Debug.Log($"Instructor now viewing telemetry: {drone.name}");
        }
    }

    private void OnEnable()
    {
        SpawnManager.OnAllDronesSpawned += PopulateDropdown;
    }

    private void OnDisable()
    {
        SpawnManager.OnAllDronesSpawned -= PopulateDropdown;
    }
}
