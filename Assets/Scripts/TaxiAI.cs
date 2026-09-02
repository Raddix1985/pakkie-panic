using UnityEngine;

public class TaxiAI : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float forwardSpeed = 8f;
    [SerializeField] private bool travelAlongX = true;

    [Header("Lanes")]
    [SerializeField, Min(1)] private int laneCount = 3;
    [SerializeField] private float laneWidth = 3.5f;
    [SerializeField] private float laneCenterOffset = 0f;
    [SerializeField] private float laneSnapSpeed = 14f;
    [SerializeField] private bool randomStartingLane = true;

    private int currentLane;
    private float targetLanePosition;

    private void Start()
    {
        currentLane = randomStartingLane ? Random.Range(0, laneCount) : laneCount / 2;
        targetLanePosition = GetLanePosition(currentLane);

        Vector3 position = transform.position;
        if (travelAlongX)
            position.z = targetLanePosition;
        else
            position.x = targetLanePosition;

        transform.position = position;
    }

    private void Update()
    {
        Vector3 position = transform.position;

        if (travelAlongX)
        {
            position.x += forwardSpeed * Time.deltaTime;
            position.z = Mathf.MoveTowards(
                position.z,
                targetLanePosition,
                laneSnapSpeed * Time.deltaTime);
        }
        else
        {
            position.z += forwardSpeed * Time.deltaTime;
            position.x = Mathf.MoveTowards(
                position.x,
                targetLanePosition,
                laneSnapSpeed * Time.deltaTime);
        }

        transform.position = position;
    }

    public void SetLane(int lane)
    {
        currentLane = Mathf.Clamp(lane, 0, laneCount - 1);
        targetLanePosition = GetLanePosition(currentLane);
    }

    private float GetLanePosition(int lane)
    {
        return laneCenterOffset + (lane - (laneCount - 1) * 0.5f) * laneWidth;
    }
}
