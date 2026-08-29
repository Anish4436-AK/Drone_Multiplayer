using UnityEngine;
using UnityEngine.UI;

public class DroneSelectionManager : MonoBehaviour
{
    public static DroneSelectionManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject droneSelectionPanel;

    [Header("Drone Buttons")]
    [SerializeField] private Button drone1Button;
    [SerializeField] private Button drone2Button;
    [SerializeField] private Button drone3Button;

    [Header("Join")]
    [SerializeField] private Button joinButton;

    private int selectedDrone = -1;

    public int SelectedDrone => selectedDrone;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        droneSelectionPanel.SetActive(false);
        joinButton.interactable = false;
    }

    // Called by the MAIN MENU Join Session button
    public void OpenDroneSelection()
    {
        mainMenuPanel.SetActive(false);
        droneSelectionPanel.SetActive(true);

        selectedDrone = -1;
        joinButton.interactable = false;

        Debug.Log("Drone Selection Opened");
    }

    public void SelectDrone(int droneIndex)
    {
        selectedDrone = droneIndex;

        joinButton.interactable = true;

        Debug.Log($"Drone {selectedDrone + 1} Selected");
    }

    // Called by the FINAL Join button
    public void JoinSession()
    {
        if (selectedDrone < 0)
        {
            Debug.LogWarning("Please select a drone first.");
            return;
        }

        Debug.Log($"Joining Session with Drone {selectedDrone + 1}");

        ConnectionManager.Instance.JoinSession(selectedDrone);
    }
}