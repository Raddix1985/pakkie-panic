using UnityEngine;

public class PackageBalance : MonoBehaviour
{
    [Header("True Centrifugal Physics")]
    [Tooltip("How strongly the turn translates into a leaning angle. Higher = leans more.")]
    public float tiltMultiplier = 0.4f;

    [Tooltip("How fast the box reacts to the turn and snaps back when driving straight.")]
    public float suspensionStiffness = 5f;

    [Tooltip("The exact angle where the run ends.")]
    public float maxTiltAngle = 45f;

    private Vector3 lastForward;
    private float currentTilt = 0f;
    private bool hasDropped = false;

    void Start()
    {
        if (transform.parent != null)
        {
            lastForward = transform.parent.forward;
        }
    }

    void Update()
    {
        if (hasDropped || transform.parent == null) return;

        // 1. MEASURE DEGREES PER SECOND (The exact speed of your turn)
        Vector3 currentForward = transform.parent.forward;
        float turnAngle = Vector3.SignedAngle(lastForward, currentForward, Vector3.up);
        lastForward = currentForward;

        // Prevent math errors if Time.deltaTime is incredibly small
        if (Time.deltaTime == 0) return;

        float turnRate = turnAngle / Time.deltaTime;

        // 2. CALCULATE TRUE CENTRIFUGAL LEAN
        // If you turn right (+), centrifugal force pushes the target lean left (-)
        float targetTilt = -turnRate * tiltMultiplier;

        // 3. APPLY SUSPENSION (Smooth Lerp)
        // The box fights to reach the target lean, and fights to return to 0 when driving straight
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * suspensionStiffness);

        // 4. VISUAL UPDATE
        transform.localRotation = Quaternion.Euler(0, 0, currentTilt);

        // 5. THE KILL SCREEN
        if (Mathf.Abs(currentTilt) >= maxTiltAngle)
        {
            DropPackage();
        }
    }

    public void DropPackage()
    {
        if (hasDropped) return;

        hasDropped = true;
        Debug.LogWarning("YOH! Package Dropped! The Gees is gone!");

        transform.parent = null;

        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.AddForce(Vector3.up * 5f, ForceMode.Impulse);
        rb.AddTorque(new Vector3(10f, 10f, 10f));
    }
}