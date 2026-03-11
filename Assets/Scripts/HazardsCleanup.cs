using UnityEngine;

public class HazardCleanup : MonoBehaviour
{
    private Transform player;

    void Start()
    {
        // Find the scooter automatically the moment the pothole is born
        player = GameObject.Find("Scooter").transform;
    }

    void Update()
    {
        // If the player drives 20 units past this hazard, delete it to save phone memory
        if (player != null && player.position.z > transform.position.z + 20f)
        {
            Destroy(gameObject);
        }
    }
}