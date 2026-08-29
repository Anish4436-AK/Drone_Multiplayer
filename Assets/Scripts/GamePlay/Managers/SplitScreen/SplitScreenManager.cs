using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SplitScreenManager : MonoBehaviour
{
    [Header("SplitDividers")]
    [SerializeField] private GameObject splitDivider;
    [SerializeField] private GameObject splitDividerFor3;
    [SerializeField] private GameObject splitDividerFor4;

    [Header("Instructor Camera")]
    [SerializeField] private Camera mainInstructorCamera;

    [Header("Split Telemetry")]
    [SerializeField] private SplitTelemetryUI telemetryPanelPrefab;
    [SerializeField] private Transform telemetryParent;

    private readonly List<SplitTelemetryUI> activeTelemetryPanels = new();

    [SerializeField] private GameObject instructorTelemetryPanel;

    private readonly List<Camera> droneCameras = new();

    private bool splitViewActive = false;

    private void Update()
    {
        if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Input.GetKeyDown(KeyCode.S))
        {
            if (splitViewActive)
            {
                DisableSplitView();
                Debug.Log("SplitView Disable...");
            }
            else
            {
                EnableSplitView();
                Debug.Log("SplitView Disable...");
            }
        }
    }
     
    public void EnableSplitView()
    {
        if (splitViewActive)
            return;

        droneCameras.Clear();

        CameraSwitch.SplitViewActive = true;

        if (SpawnManager.Instance == null)
        {
            Debug.LogError("SpawnManager Instance is NULL!");
            CameraSwitch.SplitViewActive = false;
            return;
        }

        List<NetworkObject> drones = SpawnManager.Instance.GetAllDrones();

        Debug.Log($"Split View: Found {drones.Count} drones");

        foreach (NetworkObject drone in drones)
        {
            if (drone == null)
                continue;

            CameraSwitch cameraSwitch =
                drone.GetComponent<CameraSwitch>();

            if (cameraSwitch == null)
            {
                Debug.LogError(
                    $"CameraSwitch NOT found on {drone.name}"
                );
                continue;
            }

            if (cameraSwitch.camera1 == null)
            {
                Debug.LogError(
                    $"FollowCamera is NULL on {drone.name}"
                );
                continue;
            }

            Camera cam = cameraSwitch.camera1;

            // Make absolutely sure the Camera GameObject is active
            cam.gameObject.SetActive(true);

            // Enable camera
            cam.enabled = true;

            // Render to main display
            cam.targetDisplay = 0;

            // Make sure it is not rendering to a RenderTexture
            cam.targetTexture = null;

            Debug.Log(
                $"CAMERA CHECK -> " +
                $"Name: {cam.name} | " +
                $"GameObject Active: {cam.gameObject.activeInHierarchy} | " +
                $"Camera Enabled: {cam.enabled} | " +
                $"Display: {cam.targetDisplay}"
            );

            // Disable FPV camera
            if (cameraSwitch.camera2 != null)
            {
                cameraSwitch.camera2.enabled = false;
            }

            droneCameras.Add(cam);

            Debug.Log(
                $"Split Camera Added: {cam.name}, Enabled: {cam.enabled}"
            );
        }

        if (droneCameras.Count == 0)
        {
            Debug.LogError(
                "SPLIT VIEW FAILED: No drone cameras found!"
            );

            CameraSwitch.SplitViewActive = false;
            return;
        }

        // Disable instructor camera only AFTER drone cameras are ready
        if (mainInstructorCamera != null)
        {
            mainInstructorCamera.enabled = false;
        }

        SetupSplitLayout();
        SetupSplitTelemetry();
        //SetupDividers();

        // Show divider only when 2 or more drone cameras exist
        if (splitDivider != null)
        {
            splitDivider.SetActive(droneCameras.Count == 2);
            splitDividerFor3.SetActive(droneCameras.Count == 3);
            splitDividerFor4.SetActive(droneCameras.Count == 4);
        }

        splitViewActive = true;

        Debug.Log(
            $"Split View Enabled with {droneCameras.Count} cameras"
        );

        if (instructorTelemetryPanel != null)
        {
            instructorTelemetryPanel.SetActive(false);
        }
    }


    public void DisableSplitView()
    {
        if (!splitViewActive)
            return;

        splitViewActive = false;

        if (splitDivider != null)
        {
            splitDivider.SetActive(false);
            splitDividerFor3.SetActive(false);
            splitDividerFor4.SetActive(false);
        }

        CameraSwitch.SplitViewActive = false;

        foreach (NetworkObject drone in SpawnManager.Instance.GetAllDrones())
        {
            if (drone == null)
                continue;

            CameraSwitch cameraSwitch =
                drone.GetComponent<CameraSwitch>();

            if (cameraSwitch != null)
            {
                cameraSwitch.DisableSplitCamera();
            }
        }

        droneCameras.Clear();

        if (mainInstructorCamera != null)
        {
            mainInstructorCamera.enabled = true;
        }

        Debug.Log("Split View Disabled");

        if (instructorTelemetryPanel != null)
        {
            instructorTelemetryPanel.SetActive(true);
        }

        ClearSplitTelemetry();
    }


    private void SetupSplitLayout()
    {
        int count = Mathf.Min(droneCameras.Count, 4);

        // First reset all camera rectangles
        foreach (Camera cam in droneCameras)
        {
            cam.rect = new Rect(0, 0, 1, 1);
        }

        if (count == 1)
        {
            droneCameras[0].rect =
                new Rect(0, 0, 1, 1);
        }
        else if (count == 2)
        {
            // Left
            droneCameras[0].rect =
                new Rect(0, 0, 0.5f, 1);

            // Right
            droneCameras[1].rect =
                new Rect(0.5f, 0, 0.5f, 1);
        }
        else if (count == 3)
        {
            droneCameras[0].rect =
                new Rect(0, 0.5f, 0.5f, 0.5f);

            droneCameras[1].rect =
                new Rect(0.5f, 0.5f, 0.5f, 0.5f);

            droneCameras[2].rect =
                new Rect(0, 0, 1, 0.5f);
        }
        else if (count == 4)
        {
            droneCameras[0].rect =
                new Rect(0, 0.5f, 0.5f, 0.5f);

            droneCameras[1].rect =
                new Rect(0.5f, 0.5f, 0.5f, 0.5f);

            droneCameras[2].rect =
                new Rect(0, 0, 0.5f, 0.5f);

            droneCameras[3].rect =
                new Rect(0.5f, 0, 0.5f, 0.5f);
        }
    }

    private void SetupSplitTelemetry()
    {
        // Remove old telemetry panels
        ClearSplitTelemetry();

        List<NetworkObject> drones = SpawnManager.Instance.GetAllDrones();

        for (int i = 0; i < drones.Count && i < 4; i++)
        {
            NetworkObject drone = drones[i];

            if (drone == null)
                continue;

            DroneTelemetry droneTelemetry =
                drone.GetComponent<DroneTelemetry>();

            if (droneTelemetry == null)
                continue;

            // Create a copy of our disabled template
            SplitTelemetryUI panel = Instantiate(
                telemetryPanelPrefab,
                telemetryParent);

            panel.gameObject.SetActive(true);

            // Connect this panel to the correct drone
            panel.SetTargetDrone(droneTelemetry, i + 1);

            // Position panel according to split screen layout
            SetupTelemetryPosition(
                panel.GetComponent<RectTransform>(),
                i,
                drones.Count);

            activeTelemetryPanels.Add(panel);
        }
    }

    private void ClearSplitTelemetry()
    {
        foreach (SplitTelemetryUI panel in activeTelemetryPanels)
        {
            if (panel != null)
            {
                Destroy(panel.gameObject);
            }
        }

        activeTelemetryPanels.Clear();
    }

    private void SetupTelemetryPosition(RectTransform panel,int index,int total)
    {
        panel.sizeDelta = new Vector2(280f, 130f);
        panel.pivot = new Vector2(0f, 1f);
        panel.anchorMin = Vector2.zero;
        panel.anchorMax = Vector2.zero;
        panel.pivot = new Vector2(0, 1);

        if (total == 1)
        {
            panel.anchorMin = new Vector2(0, 1);
            panel.anchorMax = new Vector2(0, 1);
            panel.anchoredPosition = new Vector2(20, -20);
        }
        else if (total == 2)
        {
            // Left screen
            if (index == 0)
            {
                panel.anchorMin = new Vector2(0, 1);
                panel.anchorMax = new Vector2(0, 1);
                panel.anchoredPosition = new Vector2(20, -20);
            }
            // Right screen
            else
            {
                panel.anchorMin = new Vector2(0.5f, 1);
                panel.anchorMax = new Vector2(0.5f, 1);
                panel.anchoredPosition = new Vector2(20, -20);
            }
        }
        else if (total == 3)
        {
            if (index == 0)
            {
                panel.anchorMin = new Vector2(0, 1);
                panel.anchorMax = new Vector2(0, 1);
                panel.anchoredPosition = new Vector2(20, -20);
            }
            else if (index == 1)
            {
                panel.anchorMin = new Vector2(0.5f, 1);
                panel.anchorMax = new Vector2(0.5f, 1);
                panel.anchoredPosition = new Vector2(20, -20);
            }
            else
            {
                panel.anchorMin = new Vector2(0, 0.5f);
                panel.anchorMax = new Vector2(0, 0.5f);
                panel.anchoredPosition = new Vector2(20, -20);
            }
        }
        else if (total >= 4)
        {
            Vector2[] anchors =
            {
            new Vector2(0, 1),      // Top Left
            new Vector2(0.5f, 1),   // Top Right
            new Vector2(0, 0.5f),   // Bottom Left
            new Vector2(0.5f, 0.5f) // Bottom Right
        };

            panel.anchorMin = anchors[index];
            panel.anchorMax = anchors[index];
            panel.anchoredPosition = new Vector2(20, -20);
        }
    }
}