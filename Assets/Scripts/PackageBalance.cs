using UnityEngine;
using UnityEngine.Events;

public class PackageBalance : MonoBehaviour
{
    [Header("Balance")]
    [SerializeField] private float tiltMultiplier = 0.22f;
    [SerializeField] private float suspensionStiffness = 9f;
    [SerializeField] private float maxTiltAngle = 38f;
    [SerializeField] private float dropDelay = 0.12f;

    [Header("Impact")]
    [SerializeField] private float knockOffImpact = 4.5f;
    [SerializeField] private float upwardImpulse = 1.5f;
    [SerializeField] private float torqueImpulse = 3f;

    [Header("Events")]
    [SerializeField] private UnityEvent onPackageDropped;

    private Transform scooter;
    private Vector3 lastForward;
    private float currentTilt;
    private bool hasDropped;
    private float overTiltTime;

    private void Awake()
    {
        scooter = transform.parent;
    }

    private void Start()
    {
        if (scooter != null)
        {
            lastForward = scooter.forward;
        }
    }

    private void LateUpdate()
    {
        if (hasDropped || scooter == null)
        {
            return;
        }

        Vector3 currentForward = scooter.forward;
        float turnRate = Vector3.SignedAngle(
            lastForward,
            currentForward,
            Vector3.up) / Mathf.Max(Time.deltaTime, 0.0001f);

        lastForward = currentForward;

        float targetTilt = Mathf.Clamp(
            -turnRate * tiltMultiplier,
            -maxTiltAngle,
            maxTiltAngle);

        // SmoothDamp is more fluid than frame-dependent Lerp for this use.
        currentTilt = Mathf.Lerp(
            currentTilt,
            targetTilt,
            1f - Mathf.Exp(-suspensionStiffness * Time.deltaTime));

        transform.localRotation = Quaternion.Euler(
            0f,
            0f,
            currentTilt);

        if (Mathf.Abs(currentTilt) >= maxTiltAngle * 0.92f)
        {
            overTiltTime += Time.deltaTime;

            if (overTiltTime >= dropDelay)
            {
                DropPackage();
            }
        }
        else
        {
            overTiltTime = 0f;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasDropped)
        {
            return;
        }

        // A meaningful side/front impact can knock the package off.
        float impact = collision.relativeVelocity.magnitude;

        if (impact >= knockOffImpact)
        {
            DropPackage(collision.relativeVelocity);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasDropped)
        {
            return;
        }

        if (other.CompareTag("Hazard"))
        {
            DropPackage();
        }
    }

    public void DropPackage()
    {
        DropPackage(Vector3.zero);
    }

    private void DropPackage(Vector3 impactVelocity)
    {
        if (hasDropped)
        {
            return;
        }

        hasDropped = true;

        transform.SetParent(null, true);

        Rigidbody body = GetComponent<Rigidbody>();
        if (body == null)
        {
            body = gameObject.AddComponent<Rigidbody>();
        }

        body.isKinematic = false;
        body.useGravity = true;

        Vector3 impulse = impactVelocity.sqrMagnitude > 0.01f
            ? impactVelocity.normalized * knockOffImpact
            : scooter != null
                ? scooter.forward * 1.5f
                : Vector3.forward;

        impulse += Vector3.up * upwardImpulse;

        body.AddForce(impulse, ForceMode.Impulse);
        body.AddTorque(
            Random.insideUnitSphere * torqueImpulse,
            ForceMode.Impulse);

        onPackageDropped?.Invoke();
    }
}
