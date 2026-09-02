using UnityEngine;

public class IsoCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float followDistance = 14f;
    [SerializeField] private float height = 11f;
    [SerializeField] private float sideOffset = 8f;
    [SerializeField] private float lookAheadDistance = 18f;
    [SerializeField] private float positionSmoothTime = 0.18f;
    [SerializeField] private float rotationSpeed = 8f;

    private Vector3 velocity;

    private void Start()
    {
        if (player != null)
        {
            SnapToPlayer();
        }
    }

    private void LateUpdate()
    {
        if (player == null)
        {
            return;
        }

        Vector3 forward = Vector3.ProjectOnPlane(player.forward, Vector3.up).normalized;
        if (forward.sqrMagnitude < Mathf.Epsilon)
        {
            forward = Vector3.right;
        }

        Vector3 right = Vector3.Cross(Vector3.up, forward);
        Vector3 desiredPosition = player.position - forward * followDistance + right * sideOffset + Vector3.up * height;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, positionSmoothTime);

        Vector3 target = player.position + forward * lookAheadDistance + Vector3.up * 1.5f;
        Quaternion desiredRotation = Quaternion.LookRotation(target - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSpeed * Time.deltaTime);
    }

    private void SnapToPlayer()
    {
        Vector3 forward = Vector3.ProjectOnPlane(player.forward, Vector3.up).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        transform.position = player.position - forward * followDistance + right * sideOffset + Vector3.up * height;
        transform.LookAt(player.position + forward * lookAheadDistance + Vector3.up * 1.5f);
    }
}
