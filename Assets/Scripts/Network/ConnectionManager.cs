using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectionManager : MonoBehaviour
{
    public static ConnectionManager Instance { get; private set; }

    [Header("LAN Settings")]
    [SerializeField] private ushort port = 7777;
    private int pendingSelectedDrone = -1;
    private bool callbacksRegistered = false;
    private bool waitingForHost = false;

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

    private void Start()
    {
        if (LANDiscovery.Instance != null)
        {
            LANDiscovery.Instance.OnHostFound += OnHostFound;
        }
    }

    // =========================================================
    // CREATE SESSION - INSTRUCTOR
    // =========================================================

    public void CreateSession()
    {
        Debug.Log("Create Session Requested");

        RegisterCallbacks();

        SetupConnectionApproval();

        UnityTransport transport =
            NetworkManager.Singleton
                .GetComponent<UnityTransport>();

        transport.SetConnectionData(
            "0.0.0.0",
            port,
            "0.0.0.0");

        bool success =
            NetworkManager.Singleton.StartHost();

        Debug.Log($"StartHost : {success}");

        if (!success)
        {
            Debug.LogError("Failed to start Host.");
            return;
        }

        SessionManager.Instance.SetHost(true);

        if (LANDiscovery.Instance != null)
            LANDiscovery.Instance.StartHostDiscovery();

        SceneManager.LoadScene("Lobby");
    }

    // =========================================================
    // JOIN SESSION - TRAINEE
    // =========================================================

    public void JoinSession(int selectedDrone)
    {
        Debug.Log(
            $"Join Session Requested - Drone: {selectedDrone}");

        // Store the selection so it survives until
        // LAN discovery finds the Host.
        pendingSelectedDrone = selectedDrone;

        // Make sure the selection is valid.
        if (pendingSelectedDrone < 0)
        {
            Debug.LogError(
                "Join cancelled: No drone has been selected.");

            return;
        }

        SessionManager.Instance.SetSelectedDrone(
            pendingSelectedDrone);

        RegisterCallbacks();

        if (LANDiscovery.Instance == null)
        {
            Debug.LogError("LANDiscovery Instance is NULL!");
            return;
        }

        waitingForHost = true;

        Debug.Log(
            $"Searching for Instructor... " +
            $"Selected Drone = {pendingSelectedDrone}");

        LANDiscovery.Instance.StartClientDiscovery();
    }

    // =========================================================
    // HOST FOUND
    // =========================================================

    private void OnHostFound(string hostIP)
    {
        if (!waitingForHost)
            return;

        waitingForHost = false;

        Debug.Log(
            $"Instructor found automatically at {hostIP}");

        if (pendingSelectedDrone < 0)
        {
            Debug.LogError(
                "Cannot connect because selected drone is -1.");

            return;
        }

        // -------------------------------------------------
        // SEND DRONE SELECTION TO HOST
        // -------------------------------------------------

        NetworkManager.Singleton.NetworkConfig.ConnectionData =
            System.Text.Encoding.UTF8.GetBytes(
                pendingSelectedDrone.ToString());

        Debug.Log(
            $"Sending Drone Selection to Host: " +
            $"{pendingSelectedDrone}");

        // -------------------------------------------------
        // CONFIGURE UNITY TRANSPORT
        // -------------------------------------------------

        Unity.Netcode.Transports.UTP.UnityTransport transport =
            NetworkManager.Singleton
                .GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();

        transport.SetConnectionData(
            hostIP,
            port);

        Debug.Log(
            $"Connecting to Instructor: {hostIP}:{port}");

        // -------------------------------------------------
        // START CLIENT
        // -------------------------------------------------

        bool success =
            NetworkManager.Singleton.StartClient();

        Debug.Log(
            $"StartClient : {success}");

        if (!success)
        {
            Debug.LogError(
                "Failed to start Client.");

            waitingForHost = false;
        }
    }

    // =========================================================
    // CALLBACKS
    // =========================================================

    private void RegisterCallbacks()
    {
        if (callbacksRegistered)
            return;

        NetworkManager.Singleton.OnClientConnectedCallback +=
            OnClientConnected;

        NetworkManager.Singleton.OnClientDisconnectCallback +=
            OnClientDisconnected;

        callbacksRegistered = true;

        Debug.Log("Network Callbacks Registered");
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client Connected : {clientId}");

        // Only the Host registers itself as Instructor here.
        if (NetworkManager.Singleton.IsHost &&
            clientId == NetworkManager.Singleton.LocalClientId)
        {
            SessionManager.Instance.RegisterPlayer(
                clientId,
                PlayerRole.Instructor,
                -1);
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log(
            $"Client Disconnected : {clientId}");

        SessionManager.Instance.RemovePlayer(
            clientId);
    }

    private void OnDestroy()
    {
        if (LANDiscovery.Instance != null)
        {
            LANDiscovery.Instance.OnHostFound -=
                OnHostFound;
        }

        if (!callbacksRegistered)
            return;

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -=
                OnClientConnected;

            NetworkManager.Singleton.OnClientDisconnectCallback -=
                OnClientDisconnected;
        }

        callbacksRegistered = false;
    }

    private void SetupConnectionApproval()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback =
            ApprovalCheck;
    }

    private void ApprovalCheck(
      NetworkManager.ConnectionApprovalRequest request,
      NetworkManager.ConnectionApprovalResponse response)
    {
        // Host does not need a drone.
        if (request.ClientNetworkId ==
            NetworkManager.Singleton.LocalClientId)
        {
            response.Approved = true;
            response.CreatePlayerObject = false;
            response.Pending = false;

            Debug.Log(
                "Connection Approval: Host connection approved.");

            return;
        }

        int selectedDrone = -1;

        if (request.Payload != null &&
            request.Payload.Length > 0)
        {
            string droneData =
                System.Text.Encoding.UTF8.GetString(
                    request.Payload);

            int.TryParse(
                droneData,
                out selectedDrone);
        }

        Debug.Log(
            $"Connection Approval - " +
            $"Client {request.ClientNetworkId} " +
            $"Drone: {selectedDrone}");

        if (selectedDrone < 0 || selectedDrone > 1)
        {
            response.Approved = false;
            response.CreatePlayerObject = false;
            response.Pending = false;
            response.Reason = "Invalid drone selection.";

            Debug.LogError(
                $"Invalid drone selection for Client " +
                $"{request.ClientNetworkId}: {selectedDrone}");

            return;
        }

        SessionManager.Instance.RegisterPlayer(
            request.ClientNetworkId,
            PlayerRole.Trainee,
            selectedDrone);

        response.Approved = true;
        response.CreatePlayerObject = false;
        response.Pending = false;

        Debug.Log(
            $"Client {request.ClientNetworkId} " +
            $"approved with Drone {selectedDrone}");
    }
}