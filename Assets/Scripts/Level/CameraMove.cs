using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = GetDesiredPosition(target);
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        transform.position = smoothedPosition;
    }

    public void SetTarget(Transform newTarget, bool snapToTarget = false)
    {
        target = newTarget;

        if (snapToTarget && target != null)
        {
            transform.position = GetDesiredPosition(target);
        }
    }

    private Vector3 GetDesiredPosition(Transform targetTransform)
    {
        return new Vector3(
            targetTransform.position.x,
            targetTransform.position.y,
            transform.position.z) + offset;
    }
}
