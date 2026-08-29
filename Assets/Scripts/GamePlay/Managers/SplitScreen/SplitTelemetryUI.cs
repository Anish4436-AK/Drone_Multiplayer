using TMPro;
using UnityEngine;

public class SplitTelemetryUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text droneNameText;
    [SerializeField] private TMP_Text altitudeText;
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private TMP_Text statusText;

    private DroneTelemetry targetDrone;
    private int droneNumber;

    private void Update()
    {
        if (targetDrone == null)
            return;

        altitudeText.text =
            $"Altitude: {targetDrone.Altitude.Value:F1} m";

        speedText.text =
            $"Speed: {targetDrone.Speed.Value:F1} m/s";

        statusText.text =
            targetDrone.IsArmed()
                ? "Status: ARMED"
                : "Status: DISARMED";
    }

    public void SetTargetDrone(DroneTelemetry drone, int number)
    {
        targetDrone = drone;
        droneNumber = number;

        droneNameText.text = $"Drone {droneNumber}";
    }
}