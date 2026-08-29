using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectionManager : MonoBehaviour
{
    public static ConnectionManager Instance { get; private set; }

    private bool callbacksRegistered = false;
    private bool connectionApprovalRegistered = false;
    private Dictionary<ulong, int> clientDroneSelections =
    new Dictionary<ulong, int>();

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

    #region Session

    public void CreateSession()
    {
        Debug.Log("Create Session Requested");

        RegisterCallbacks();
        RegisterConnectionApproval();

        bool success = NetworkManager.Singleton.StartHost();

        Debug.Log($"StartHost : {success}");

        if (!success)
        {
            Debug.LogError("Failed to start Host.");
            return;
        }

        SessionManager.Instance.SetHost(true);

        Debug.Log("Host Started Successfully");

        SceneManager.LoadScene("Lobby");
    }

    public void JoinSession(int selectedDrone)
    {
        Debug.Log($"Join Session Requested - Drone {selectedDrone + 1}");

        SessionManager.Instance.SetSelectedDrone(selectedDrone);

        NetworkManager.Singleton.NetworkConfig.ConnectionData =
            Encoding.UTF8.GetBytes(selectedDrone.ToString());

        RegisterCallbacks();

        bool success = NetworkManager.Singleton.StartClient();

        Debug.Log($"StartClient : {success}");

        if (!success)
        {
            Debug.LogError("Failed to start Client.");
        }
    }

    #endregion

    #region Network Callbacks

    private void RegisterCallbacks()
    {
        if (callbacksRegistered)
            return;

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        callbacksRegistered = true;

        Debug.Log("Network Callbacks Registered");
    }

    private void UnregisterCallbacks()
    {
        if (!callbacksRegistered)
            return;

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        callbacksRegistered = false;
    }

    private void RegisterConnectionApproval()
    {
        if (connectionApprovalRegistered)
            return;

        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;

        connectionApprovalRegistered = true;

        Debug.Log("Connection Approval Registered");
    }

    private void ApprovalCheck(
     NetworkManager.ConnectionApprovalRequest request,
     NetworkManager.ConnectionApprovalResponse response)
    {
        int selectedDrone = -1;

        if (request.Payload != null && request.Payload.Length > 0)
        {
            string droneData = Encoding.UTF8.GetString(request.Payload);

            int.TryParse(droneData, out selectedDrone);
        }

        clientDroneSelections[request.ClientNetworkId] = selectedDrone;

        Debug.Log(
            $"Host received selection from Client " +
            $"{request.ClientNetworkId}: Drone {selectedDrone}");

        response.Approved = true;

        // We manually spawn drones later
        response.CreatePlayerObject = false;
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client Connected : {clientId}");

        PlayerRole role;

        if (NetworkManager.Singleton.IsHost &&
            clientId == NetworkManager.Singleton.LocalClientId)
        {
            role = PlayerRole.Instructor;

            SessionManager.Instance.RegisterPlayer(
                clientId,
                role
            );
        }
        else
        {
            role = PlayerRole.Trainee;

            int selectedDrone = -1;

            clientDroneSelections.TryGetValue(
                clientId,
                out selectedDrone
            );

            SessionManager.Instance.RegisterPlayer(
                clientId,
                role,
                selectedDrone
            );
        }
    }


    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client Disconnected : {clientId}");

        SessionManager.Instance.RemovePlayer(clientId);
    }

    #endregion

    private void OnDestroy()
    {
        UnregisterCallbacks();
    }
}   