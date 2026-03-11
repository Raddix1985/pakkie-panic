using UnityEngine;

public class TaxiAI : MonoBehaviour
{
    [Header("Engine Specs")]
    public float forwardSpeed = 10f; // Slower than the player's 20f so you catch up to it
    public float sideSnappiness = 8f; // Taxis swerve hard
    public float laneChangeInterval = 2f; // How often it thinks about swerving

    // The 4-lane coordinates
    private float[] lanes = { -6f, -2f, 2f, 6f };
    private int currentLane;
    private float timer;

    void Start()
    {
        // 1. Figure out which lane it spawned in so it doesn't instantly teleport
        float closestDist = float.MaxValue;
        for (int i = 0; i < lanes.Length; i++)
        {
            float dist = Mathf.Abs(transform.position.x - lanes[i]);
            if (dist < closestDist)
            {
                closestDist = dist;
                currentLane = i;
            }
        }

        // Add a little randomness so they don't all swerve at the exact same time
        timer = laneChangeInterval + Random.Range(0f, 1f);
    }

    void Update()
    {
        Vector3 newPos = transform.position;

        // 1. Drive forward constantly
        newPos.z += forwardSpeed * Time.deltaTime;

        // 2. The AI Decision Engine
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            timer = laneChangeInterval + Random.Range(0f, 1f); // Reset timer

            // 50/50 chance to swerve left or right
            if (Random.value > 0.5f)
            {
                if (currentLane < 3) currentLane++; // Swerve Right
            }
            else
            {
                if (currentLane > 0) currentLane--; // Swerve Left
            }
        }

        // 3. Execute the swerve
        newPos.x = Mathf.Lerp(transform.position.x, lanes[currentLane], sideSnappiness * Time.deltaTime);
        transform.position = newPos;
    }
}