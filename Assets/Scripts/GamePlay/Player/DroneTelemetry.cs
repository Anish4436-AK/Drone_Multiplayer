using Unity.Netcode;
using UnityEngine;

public class DroneTelemetry : NetworkBehaviour
{
    private Rigidbody rb;
    private DroneMotor droneMotor;

    public NetworkVariable<float> Altitude =
        new NetworkVariable<float>(
            0f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

    public NetworkVariable<float> Speed =
        new NetworkVariable<float>(
            0f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        droneMotor = GetComponent<DroneMotor>();
    }

    private void Update()
    {
        // Only owner calculates telemetry
        if (!IsOwner)
            return;

        Altitude.Value = transform.position.y;

        if (rb != null)
        {
            Speed.Value = rb.velocity.magnitude;
        }
    }

    public bool IsArmed()
    {
        if (droneMotor == null)
            return false;

        return droneMotor.isArmed.Value;
    }
}