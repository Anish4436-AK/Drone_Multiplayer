using UnityEngine;

public class SplitCameraFollow : MonoBehaviour
{
    private Transform target;
    private Vector3 offset;
    private float followSpeed;

    public void Initialize(
        Transform newTarget,
        Vector3 newOffset,
        float newFollowSpeed)
    {
        target = newTarget;
        offset = newOffset;
        followSpeed = newFollowSpeed;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desiredPosition =
            target.TransformPoint(offset);

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );

        transform.LookAt(target);
    }
}