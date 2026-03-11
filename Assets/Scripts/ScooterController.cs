using UnityEngine;
using System.Collections;

public class ScooterController : MonoBehaviour
{
    [Header("Engine Specs")]
    public float acceleration = 15f; // Auto-gas speed
    public float turnSpeed = 150f;
    public float brakeMultiplier = 0.5f; // Reverses at half speed when holding both sides

    [Header("Components")]
    public PackageBalance packageScript;
    public UIManager uiManager;

    private bool isDashing = false;
    private float normalSpeed;

    void Start()
    {
        normalSpeed = acceleration;
    }

    void Update()
    {
        // THE DASH TRIGGER (Still works on Spacebar for PC)
        if (Input.GetKeyDown(KeyCode.Space) && !isDashing)
        {
            if (uiManager != null && uiManager.IsGeesFull())
            {
                StartCoroutine(DashRoutine());
            }
        }

        // 1. READ INPUT (Hybrid PC & Mobile)
        bool pressingLeft = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
        bool pressingRight = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);

        // Read Mouse Clicks (for Editor testing)
        if (Input.GetMouseButton(0))
        {
            if (Input.mousePosition.x < Screen.width / 2f) pressingLeft = true;
            if (Input.mousePosition.x > Screen.width / 2f) pressingRight = true;
        }

        // Read Actual Mobile Touches (Allows multi-touch braking)
        if (Input.touchCount > 0)
        {
            pressingLeft = false; pressingRight = false; // Reset to purely read fingers
            foreach (Touch touch in Input.touches)
            {
                if (touch.position.x < Screen.width / 2f) pressingLeft = true;
                if (touch.position.x > Screen.width / 2f) pressingRight = true;
            }
        }

        // 2. THE ENGINE LOGIC
        float currentSpeed = acceleration;
        float turnAmount = 0f;

        if (pressingLeft && pressingRight)
        {
            // SLAM THE BRAKES (Hold both thumbs / A+D)
            currentSpeed = -acceleration * brakeMultiplier;
        }
        else if (pressingLeft)
        {
            turnAmount = -1f;
        }
        else if (pressingRight)
        {
            turnAmount = 1f;
        }

        // 3. APPLY MOVEMENT (Auto-Gas)
        transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);

        // 4. APPLY STEERING
        if (currentSpeed != 0)
        {
            // If reversing, flip the steering direction so it feels natural
            float directionModifier = currentSpeed > 0 ? 1f : -1f;
            transform.Rotate(Vector3.up * turnAmount * turnSpeed * directionModifier * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hazard"))
        {
            if (isDashing)
            {
                Debug.Log("GEES SMASH! Obliterated a " + other.gameObject.name);
                Destroy(other.gameObject);
                return;
            }

            Debug.LogError("YOH! Hit a hazard! Run is over!");
            acceleration = 0; // Kill the auto-gas
            if (packageScript != null) packageScript.DropPackage();
            if (uiManager != null) uiManager.GameOver();
        }
        void OnTriggerEnter(Collider other)
        {
            // 1. THE FAIL STATE (Hitting a Taxi/Pothole)
            if (other.CompareTag("Hazard"))
            {
                if (isDashing)
                {
                    Debug.Log("GEES SMASH! Obliterated a " + other.gameObject.name);
                    Destroy(other.gameObject);
                    return;
                }

                Debug.LogError("YOH! Hit a hazard! Run is over!");
                acceleration = 0;
                if (packageScript != null) packageScript.DropPackage();
                if (uiManager != null) uiManager.GameOver();
            }

            // 2. THE WIN STATE (Reaching the Destination)
            if (other.CompareTag("DropZone"))
            {
                Debug.Log("LAKKA! Delivery Successful! The Koesisters are hot and fresh!");

                // Hit the brakes
                acceleration = 0;

                // Destroy the zone so we don't trigger it twice
                Destroy(other.gameObject);

                // (Optional) If your UIManager has a Win Game screen, we will call it here later!
            }
        }
    }

    IEnumerator DashRoutine()
    {
        isDashing = true;
        acceleration = normalSpeed * 2f;
        uiManager.SpendGees();
        yield return new WaitForSeconds(3f);
        acceleration = normalSpeed;
        isDashing = false;
    }
}