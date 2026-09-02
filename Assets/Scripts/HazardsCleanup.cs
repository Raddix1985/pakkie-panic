using UnityEngine;

public class HazardsCleanup : MonoBehaviour
{
    [SerializeField] private float lifetime = 30f;

    private void OnEnable()
    {
        if (CompareTag("Hazard"))
        {
            Destroy(gameObject, lifetime);
        }
    }
}
