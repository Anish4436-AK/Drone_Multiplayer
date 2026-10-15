using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement; 

public class LobbyManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text connectedPlayersText;
    [SerializeField] private TMP_Text playerCountText;

    [Header("Game Settings")]
    [SerializeField] private TMP_InputField s1InputField;
    [SerializeField] private TMP_InputField s2InputField;
    [SerializeField] private TMP_InputField s3InputField;
    [SerializeField] private Button easyButton;
    [SerializeField] private Button hardButton;
    [SerializeField] private Button startSessionButton;
    [SerializeField] private Button leaveButton;

    [Header("Terrains")]
    [SerializeField] private Button coastalButton;
    [SerializeField] private Button desertButton;
        
    [SerializeField] private GameObject InstructorPanel;
    [SerializeField] private GameObject TraineePanel;

    private SessionManager.Difficulty selectedDifficulty = SessionManager.Difficulty.Easy;
    private SessionManager.Terrain selectedTerrain = SessionManager.Terrain.Coastal;
    private void Start()
    {
        if (SessionManager.Instance.IsHost)
        {
            InstructorPanel.gameObject.SetActive(true);
            TraineePanel.gameObject.SetActive(false);
            RefreshLobby();
        }
        else
        {
            TraineePanel.gameObject.SetActive(true);
            InstructorPanel.gameObject.SetActive(false);
        }
        // Default values
        s1InputField.text = SessionManager.Instance.S1ObstacleCount.ToString();
        s2InputField.text = SessionManager.Instance.S2ObstacleCount.ToString();
        s3InputField.text = SessionManager.Instance.S3ObstacleCount.ToString();

        // Difficulty buttons
        easyButton.onClick.AddListener(() => SelectDifficulty(SessionManager.Difficulty.Easy));
        hardButton.onClick.AddListener(() => SelectDifficulty(SessionManager.Difficulty.Hard));

        //Terrain buttons
        coastalButton.onClick.AddListener(() => SelectTerrain(SessionManager.Terrain.Coastal));
        desertButton.onClick.AddListener(() => SelectTerrain(SessionManager.Terrain.Desert));

    }

    public void RefreshLobby()
    {
        if (!SessionManager.Instance.IsHost)
            return;

        var trainees = SessionManager.Instance.GetTrainees();

        StringBuilder builder = new StringBuilder();

        foreach (PlayerData player in trainees)
        {
            builder.AppendLine($"Client {player.ClientId}");
        }

        connectedPlayersText.text = builder.ToString();
        playerCountText.text = $"{trainees.Count}";
    }
    private void OnEnable()
    {
        SessionManager.Instance.OnPlayersChanged += RefreshLobby;
    }

    private void OnDisable()
    {
        if (SessionManager.Instance != null)
            SessionManager.Instance.OnPlayersChanged -= RefreshLobby;
    }

    private void SelectDifficulty(SessionManager.Difficulty difficulty)
    {
        selectedDifficulty = difficulty;

        Debug.Log($"Selected Difficulty : {difficulty}");
                
    }

    private void SelectTerrain(SessionManager.Terrain terrain)
    {
        selectedTerrain = terrain;
    }

    public void StartSession()
    {
        if (!SessionManager.Instance.IsHost)
            return;

        int s1 = 5;
        int s2 = 10;
        int s3 = 15;

        int.TryParse(s1InputField.text, out s1);
        int.TryParse(s2InputField.text, out s2);
        int.TryParse(s3InputField.text, out s3);

        SessionManager.Instance.SetGameSettings(s1, s2, s3, selectedDifficulty, selectedTerrain);

       // Debug.Log($"Settings -> S1:{s1}  S2:{s2}  S3:{s3}  Difficulty:{selectedDifficulty}");

        SessionManager.Instance.StartSession();

        if (SessionManager.Instance.SelectedTerrain==SessionManager.Terrain.Coastal)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Coastal", LoadSceneMode.Single);
        }
        else if(SessionManager.Instance.SelectedTerrain == SessionManager.Terrain.Desert)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Desert", LoadSceneMode.Single);
        }
    }
}