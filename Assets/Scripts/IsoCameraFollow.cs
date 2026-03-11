using UnityEngine;

public class IsoCameraFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(-10f, 15f, -10f);
    public float smoothSpeed = 5f;

    void Start()
    {
        // Snap to the player immediately on start
        if (player != null)
        {
            transform.position = player.position + offset;
            transform.rotation = Quaternion.Euler(30f, 45f, 0f); // Lock the classic 3/4 angle
        }
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Smoothly glide to follow the player's position, but NEVER change rotation
        Vector3 targetPosition = player.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }
}