using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("World Generation")]
    public GameObject roadPrefab; // The template we just made
    public Transform player; // To track how far we've driven
    public float chunkLength = 50f; // Must match the Z-scale of our RoadChunk
    public int chunksOnScreen = 5; // How many road pieces exist at once

    private float spawnZ = 0f;
    private Queue<GameObject> activeChunks = new Queue<GameObject>();

    void Start()
    {
        // When the game boots, spawn the first 5 chunks to build the starting runway
        for (int i = 0; i < chunksOnScreen; i++)
        {
            SpawnChunk();
        }
    }

    void Update()
    {
        // THE BULLETPROOF TREADMILL
        // We no longer guess based on past chunks. 
        // We demand that the end of the road (spawnZ) is ALWAYS at least 
        // 200 units ahead of the player. 
        // A 'while' loop guarantees it builds road instantly, even if the game lags.

        while (player.position.z + 200f > spawnZ)
        {
            SpawnChunk();
            DeleteOldestChunk();
        }
    }

    void SpawnChunk()
    {
        // Create a new road chunk exactly at the spawnZ marker
        GameObject chunk = Instantiate(roadPrefab, new Vector3(0, 0, spawnZ), Quaternion.identity);

        // Add it to our memory queue
        activeChunks.Enqueue(chunk);

        // Move the spawn marker forward 50 units for the next piece
        spawnZ += chunkLength;
    }

    void DeleteOldestChunk()
    {
        // Remove the oldest chunk from memory so the phone doesn't explode
        GameObject oldChunk = activeChunks.Dequeue();
        Destroy(oldChunk);
    }
}