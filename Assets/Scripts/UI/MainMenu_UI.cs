using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    public void OnCreateSessionClicked()
    {
        ConnectionManager.Instance.CreateSession();
    }

    //public void OnJoinSessionClicked()
    //{
    //    ConnectionManager.Instance.JoinSession();
    //}
}