using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class LANDiscovery : MonoBehaviour
{
    public static LANDiscovery Instance { get; private set; }

    [SerializeField] private int discoveryPort = 47777;

    private UdpClient udpClient;
    private bool isRunning;
    private bool isHost;

    private const string DiscoveryMessage = "DRONE_TRAINING_HOST";

    // IPs discovered on the background receive thread
    private readonly Queue<string> discoveredHosts = new();

    public event Action<string> OnHostFound;

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

    private void Update()
    {
        // Process discovered hosts on Unity's main thread
        lock (discoveredHosts)
        {
            while (discoveredHosts.Count > 0)
            {
                string hostIP = discoveredHosts.Dequeue();

                //Debug.Log($"LAN Discovery: Host found at {hostIP}");

                OnHostFound?.Invoke(hostIP);
            }
        }
    }

    // =========================================================
    // HOST
    // =========================================================

    public void StartHostDiscovery()
    {
        StopDiscovery();

        isHost = true;

        try
        {
            udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;

            udpClient.Client.SetSocketOption(
                SocketOptionLevel.Socket,
                SocketOptionName.ReuseAddress,
                true);

            udpClient.Client.Bind(
                new IPEndPoint(IPAddress.Any, discoveryPort));

            isRunning = true;

            Debug.Log(
                $"LAN Discovery: Host listening on port {discoveryPort}");

            // Listen for client discovery requests
            udpClient.BeginReceive(
                ReceiveCallback,
                null);

            // Announce host
            SendAnnouncement();
        }
        catch (Exception e)
        {
            Debug.LogError(
                $"LAN Discovery Host Error: {e.Message}");

            StopDiscovery();
        }
    }

    private void SendAnnouncement()
    {
        if (!isRunning || udpClient == null)
            return;

        try
        {
            byte[] data =
                Encoding.UTF8.GetBytes(DiscoveryMessage);

            IPEndPoint broadcastEndpoint =
                new IPEndPoint(
                    IPAddress.Broadcast,
                    discoveryPort);

            udpClient.Send(
                data,
                data.Length,
                broadcastEndpoint);

            Debug.Log("LAN Discovery: Host announcement sent.");
        }
        catch (Exception e)
        {
            Debug.LogError(
                $"LAN Discovery Announcement Error: {e.Message}");
        }
    }

    // =========================================================
    // CLIENT
    // =========================================================

    public void StartClientDiscovery()
    {
        StopDiscovery();

        isHost = false;

        try
        {
            udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;

            udpClient.Client.SetSocketOption(
                SocketOptionLevel.Socket,
                SocketOptionName.ReuseAddress,
                true);

            udpClient.Client.Bind(
                new IPEndPoint(IPAddress.Any, 0));

            isRunning = true;

            Debug.Log(
                "LAN Discovery: Searching for Instructor...");

            udpClient.BeginReceive(
                ReceiveCallback,
                null);

            SendDiscoveryRequest();
        }
        catch (Exception e)
        {
            Debug.LogError(
                $"LAN Discovery Client Error: {e.Message}");

            StopDiscovery();
        }
    }

    private void SendDiscoveryRequest()
    {
        if (!isRunning || udpClient == null)
            return;

        try
        {
            byte[] data =
                Encoding.UTF8.GetBytes(DiscoveryMessage);

            IPEndPoint broadcastEndpoint =
                new IPEndPoint(
                    IPAddress.Broadcast,
                    discoveryPort);

            udpClient.Send(
                data,
                data.Length,
                broadcastEndpoint);

            Debug.Log(
                "LAN Discovery: Discovery request sent.");
        }
        catch (Exception e)
        {
            Debug.LogError(
                $"LAN Discovery Request Error: {e.Message}");
        }
    }

    // =========================================================
    // RECEIVE
    // =========================================================

    private void ReceiveCallback(IAsyncResult result)
    {
        if (!isRunning || udpClient == null)
            return;

        try
        {
            IPEndPoint remoteEndpoint =
                new IPEndPoint(IPAddress.Any, 0);

            byte[] data =
                udpClient.EndReceive(
                    result,
                    ref remoteEndpoint);

            string message =
                Encoding.UTF8.GetString(data);

            if (message == DiscoveryMessage)
            {
                string remoteIP =
                    remoteEndpoint.Address.ToString();

                if (isHost)
                {
                    // A trainee is looking for us.
                    SendResponse(remoteEndpoint);
                }
                else
                {
                    // We found the instructor.
                    lock (discoveredHosts)
                    {
                        if (!discoveredHosts.Contains(remoteIP))
                            discoveredHosts.Enqueue(remoteIP);
                    }
                }
            }

            if (isRunning && udpClient != null)
            {
                udpClient.BeginReceive(
                    ReceiveCallback,
                    null);
            }
        }
        catch (ObjectDisposedException)
        {
            // Socket was closed intentionally.
        }
        catch (Exception e)
        {
            if (isRunning)
            {
                Debug.LogError(
                    $"LAN Discovery Receive Error: {e.Message}");
            }
        }
    }

    private void SendResponse(IPEndPoint endpoint)
    {
        if (!isRunning || udpClient == null)
            return;

        try
        {
            byte[] data =
                Encoding.UTF8.GetBytes(DiscoveryMessage);

            udpClient.Send(
                data,
                data.Length,
                endpoint);

            Debug.Log(
                $"LAN Discovery: Responded to {endpoint.Address}");
        }
        catch (Exception e)
        {
            Debug.LogError(
                $"LAN Discovery Response Error: {e.Message}");
        }
    }

    // =========================================================
    // STOP
    // =========================================================

    public void StopDiscovery()
    {
        isRunning = false;

        if (udpClient != null)
        {
            try
            {
                udpClient.Close();
            }
            catch
            {
                // Ignore socket close errors
            }

            udpClient = null;
        }

        lock (discoveredHosts)
        {
            discoveredHosts.Clear();
        }
    }

    private void OnDestroy()
    {
        StopDiscovery();
    }
}