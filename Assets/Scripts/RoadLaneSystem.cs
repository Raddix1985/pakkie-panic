using UnityEngine;

public class RoadLaneSystem : MonoBehaviour
{
    public int laneCount = 3;

    public float GetLaneZ(int laneIndex)
    {
        float laneWidth = 2f; // Adjust this value based on your road design
        return (laneIndex - (laneCount - 1) / 2f) * laneWidth;
    }
}
