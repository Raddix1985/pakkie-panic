using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("World Generation")]
    [SerializeField] private GameObject roadPrefab;
    [SerializeField] private Transform player;
    [SerializeField, Min(1)] private int chunksAhead = 4;
    [SerializeField, Min(0)] private int chunksBehind = 1;
    [SerializeField] private bool travelAlongX = true;
    [SerializeField] private float roadCenterOffset = 0f;

    private readonly Queue<GameObject> activeChunks = new Queue<GameObject>();

    private float chunkLength;
    private float nextChunkStart;
    private float prefabLocalMinTravel;
    private float prefabLocalCenterLane;
    private Vector3 travelDirection;
    private Vector3 laneDirection;

    private void Start()
    {
        if (roadPrefab == null || player == null ||
            !TryGetRoadBounds(roadPrefab, out Bounds roadBounds))
        {
            Debug.LogError(
                "LevelManager requires a road prefab with an enabled collider and a player transform.",
                this);
            enabled = false;
            return;
        }

        travelDirection = travelAlongX ? Vector3.right : Vector3.forward;
        laneDirection = travelAlongX ? Vector3.forward : Vector3.right;

        chunkLength = travelAlongX ? roadBounds.size.x : roadBounds.size.z;

        if (chunkLength <= Mathf.Epsilon)
        {
            Debug.LogError("Road prefab has no usable length.", this);
            enabled = false;
            return;
        }

        prefabLocalMinTravel = travelAlongX ? roadBounds.min.x : roadBounds.min.z;
        prefabLocalCenterLane = travelAlongX ? roadBounds.center.z : roadBounds.center.x;

        float playerTravel = Vector3.Dot(player.position, travelDirection);

        // The first generated chunk begins behind the player.
        nextChunkStart =
            Mathf.Floor(playerTravel / chunkLength) * chunkLength -
            chunksBehind * chunkLength;

        int initialChunkCount = chunksBehind + chunksAhead + 1;

        for (int i = 0; i < initialChunkCount; i++)
        {
            SpawnChunk();
        }
    }

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        float playerTravel = Vector3.Dot(player.position, travelDirection);
        float targetEnd = playerTravel + chunksAhead * chunkLength;

        // Keep adding road whenever the player approaches the generated end.
        while (nextChunkStart < targetEnd)
        {
            SpawnChunk();
        }

        // Remove only chunks safely behind the player.
        while (activeChunks.Count > chunksBehind + chunksAhead + 1)
        {
            Destroy(activeChunks.Dequeue());
        }
    }

    private void SpawnChunk()
    {
        GameObject chunk = Instantiate(roadPrefab);

        float travelPosition = nextChunkStart - prefabLocalMinTravel;

        //float playerLane = Vector3.Dot(player.position, laneDirection);
        float lanePosition = roadCenterOffset;
            //playerLane - prefabLocalCenterLane + roadCenterOffset;

        Vector3 position = Vector3.zero;

        if (travelAlongX)
        {
            position.x = travelPosition;
            position.z = lanePosition;
        }
        else
        {
            position.z = travelPosition;
            position.x = lanePosition;
        }

        chunk.transform.position = position;
        chunk.transform.rotation = Quaternion.identity;

        activeChunks.Enqueue(chunk);
        nextChunkStart += chunkLength;
    }

    private static bool TryGetRoadBounds(GameObject road, out Bounds bounds)
    {
        Collider[] colliders = road.GetComponentsInChildren<Collider>();

        if (colliders.Length == 0)
        {
            bounds = default;
            return false;
        }

        // Get bounds in prefab local space so pivot placement does not
        // cause cumulative gaps/overlaps between chunks.
        Transform root = road.transform;

        bool initialized = false;
        bounds = default;

        foreach (Collider collider in colliders)
        {
            Bounds worldBounds = collider.bounds;
            Vector3[] corners =
            {
                new Vector3(worldBounds.min.x, worldBounds.min.y, worldBounds.min.z),
                new Vector3(worldBounds.min.x, worldBounds.min.y, worldBounds.max.z),
                new Vector3(worldBounds.min.x, worldBounds.max.y, worldBounds.min.z),
                new Vector3(worldBounds.min.x, worldBounds.max.y, worldBounds.max.z),
                new Vector3(worldBounds.max.x, worldBounds.min.y, worldBounds.min.z),
                new Vector3(worldBounds.max.x, worldBounds.min.y, worldBounds.max.z),
                new Vector3(worldBounds.max.x, worldBounds.max.y, worldBounds.min.z),
                new Vector3(worldBounds.max.x, worldBounds.max.y, worldBounds.max.z)
            };

            foreach (Vector3 corner in corners)
            {
                Vector3 local = root.InverseTransformPoint(corner);

                if (!initialized)
                {
                    bounds = new Bounds(local, Vector3.zero);
                    initialized = true;
                }
                else
                {
                    bounds.Encapsulate(local);
                }
            }
        }

        return initialized;
    }
}
