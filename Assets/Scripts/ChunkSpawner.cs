using UnityEngine;

public class ChunkSpawner : MonoBehaviour
{
    [Header("Hazard Generation")]
    public GameObject potholePrefab;
    public GameObject taxiPrefab; // The new dynamic hazard

    [Tooltip("Chance to spawn a hazard (0 = never, 1 = always)")]
    [Range(0f, 1f)]
    public float spawnChance = 0.7f;

    //Lane coordinates for the 3-lane road (centered perfectly on 0)
    private float[] lanes = { -3.5f, 0f, 3.5f };

    void Start()
    {
        // When this road chunk is born, roll the dice to see if we spawn a hazard
        if (Random.value <= spawnChance)
        {
            SpawnHazard();
        }
    }

    void SpawnHazard()
    {
        // 1. Pick a random lane (0, 1, 2, or 3)
        int randomLane = Random.Range(0, 3);
        float randomZ = Random.Range(-20f, 20f);

        // 2. Roll the dice: 70% chance it's a Pothole, 30% chance it's a Taxi
        GameObject hazardToSpawn = (Random.value > 0.3f) ? potholePrefab : taxiPrefab;

        // 3. Calculate the correct Y-height so the Taxi isn't buried and the Pothole doesn't float
        float spawnHeight = (hazardToSpawn == potholePrefab) ? 0.52f : 1.5f;

        // 4. Calculate the exact 3D position in the world
        Vector3 finalSpawnPosition = new Vector3(lanes[randomLane], spawnHeight, transform.position.z + randomZ);

        // 5. Drop it onto the highway
        Instantiate(hazardToSpawn, finalSpawnPosition, Quaternion.identity);
    }
}