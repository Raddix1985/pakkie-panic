using UnityEngine;

public class ChunkSpawner : MonoBehaviour
{
    [Header("Hazard Generation")]
    [SerializeField] private GameObject potholePrefab;
    [SerializeField] private GameObject taxiPrefab;
    [SerializeField, Range(0f, 1f)] private float spawnChance = 0.8f;
    [SerializeField, Min(1)] private int laneCount = 3;
    [SerializeField, Min(0)] private int maxHazardsPerChunk = 1;
    [SerializeField, Min(0f)] private float edgePadding = 1f;
    [SerializeField] private bool travelAlongX = true;
    [SerializeField] private float potholeSurfaceOffset = 0.03f;
    [SerializeField] private float taxiSurfaceOffset = 0.6f;
    [SerializeField] private float minimumHazardSeparation = 6f;

    private void Start()
    {
        if (maxHazardsPerChunk <= 0 || !TryGetRoadBounds(out Bounds roadBounds))
        {
            return;
        }

        int hazardCount = Random.value <= spawnChance ? 1 : 0;
        if (hazardCount == 0)
        {
            return;
        }

        float minTravel = (travelAlongX ? roadBounds.min.x : roadBounds.min.z) + edgePadding;
        float maxTravel = (travelAlongX ? roadBounds.max.x : roadBounds.max.z) - edgePadding;
        float minLane = (travelAlongX ? roadBounds.min.z : roadBounds.min.x) + edgePadding;
        float maxLane = (travelAlongX ? roadBounds.max.z : roadBounds.max.x) - edgePadding;

        if (minTravel >= maxTravel || minLane >= maxLane)
        {
            return;
        }

        float laneWidth = (maxLane - minLane) / laneCount;
        if (laneWidth <= 0f)
        {
            return;
        }

        // One hazard per chunk keeps the lane readable and prevents impossible walls.
        for (int i = 0; i < hazardCount; i++)
        {
            GameObject hazardPrefab = ChooseHazardPrefab();
            if (hazardPrefab == null)
            {
                continue;
            }

            int lane = Random.Range(0, laneCount);

            float travelPadding = Mathf.Min(
                edgePadding + minimumHazardSeparation * 0.5f,
                Mathf.Max(0.05f, (maxTravel - minTravel) * 0.45f));

            float travelPosition = Random.Range(
                minTravel + travelPadding,
                maxTravel - travelPadding);

            float lanePosition = minLane + laneWidth * (lane + 0.5f);

            float x = travelAlongX ? travelPosition : lanePosition;
            float z = travelAlongX ? lanePosition : travelPosition;
            float y = roadBounds.max.y +
                      (hazardPrefab == potholePrefab ? potholeSurfaceOffset : taxiSurfaceOffset);

            GameObject hazard = Instantiate(
                hazardPrefab,
                new Vector3(x, y, z),
                Quaternion.identity);

            // Ensure spawned hazards can actually be detected by ScooterController.
            if (!hazard.CompareTag("Hazard"))
            {
                hazard.tag = "Hazard";
            }
        }
    }

    private GameObject ChooseHazardPrefab()
    {
        if (potholePrefab == null) return taxiPrefab;
        if (taxiPrefab == null) return potholePrefab;
        return Random.value < 0.7f ? potholePrefab : taxiPrefab;
    }

    private bool TryGetRoadBounds(out Bounds bounds)
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        if (colliders.Length == 0)
        {
            bounds = default;
            return false;
        }

        bounds = colliders[0].bounds;
        for (int i = 1; i < colliders.Length; i++)
        {
            bounds.Encapsulate(colliders[i].bounds);
        }

        return true;
    }
}
