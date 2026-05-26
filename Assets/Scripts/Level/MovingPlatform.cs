using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;

    public float moveSpeed = 1.5f;

    private Vector3 targetPosition;

    private void Start()
    {
        targetPosition = pointA.position;
        transform.position = pointA.position;
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime);
    }

    public void MoveToState(bool isOn)
    {
        targetPosition = isOn ? pointB.position : pointA.position;
    }
}