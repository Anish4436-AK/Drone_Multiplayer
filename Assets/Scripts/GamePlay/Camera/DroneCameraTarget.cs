using UnityEngine;

public class DroneCameraTarget : MonoBehaviour
{
    [SerializeField] private Transform followTarget;

    public Transform FollowTarget => followTarget;
}