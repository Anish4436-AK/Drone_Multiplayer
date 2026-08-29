using TMPro;
using UnityEngine;

public class TelemetryUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text altitudeText;
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private TMP_Text statusText;

    private DroneTelemetry targetDrone;

    private void Update()
    {
        // If no target exists, trainee finds their own drone
        if (targetDrone == null)
        {
            FindLocalDrone();
            return;
        }

        UpdateTelemetry();
    }

    private void FindLocalDrone()
    {
        DroneTelemetry[] drones =
            FindObjectsOfType<DroneTelemetry>();

        foreach (DroneTelemetry drone in drones)
        {
            if (drone.IsOwner)
            {
                targetDrone = drone;

                Debug.Log($"Telemetry connected to: {drone.name}");

                break;
            }
        }
    }

    private void UpdateTelemetry()
    {
        altitudeText.text =
            $"Altitude: {targetDrone.Altitude.Value:F1} m";

        speedText.text =
            $"Speed: {targetDrone.Speed.Value:F1} m/s";

        statusText.text =
            targetDrone.IsArmed()
                ? "Status: ARMED"
                : "Status: DISARMED";
    }

    // Instructor will call this when changing spectated drone
    public void SetTargetDrone(DroneTelemetry drone)
    {
        targetDrone = drone;

        Debug.Log($"Instructor telemetry now watching: {drone.name}");
    }
}