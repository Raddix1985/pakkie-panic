using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ScooterController : MonoBehaviour
{
    [Header("Forward Movement")]
    [SerializeField] private float forwardSpeed = 10f;
    [SerializeField] private float acceleration = 18f;
    [SerializeField] private float maxForwardSpeed = 14f;
    [SerializeField] private float brakeMultiplier = 0.45f;

    [Header("Three Lane Movement")]
    [SerializeField, Min(1)] private int laneCount = 3;
    [SerializeField] private float laneWidth = 3.5f;
    [SerializeField] private float laneCenterOffset = 0f;
    [SerializeField] private float laneChangeSpeed = 18f;
    [SerializeField] private float laneChangeResponse = 22f;
    [SerializeField] private bool travelAlongX = true;

    [Header("Dash")]
    [SerializeField] private float dashMultiplier = 2f;
    [SerializeField] private float dashDuration = 3f;

    [Header("Gameplay Events")]
    [SerializeField] private UnityEvent onHazardHit;
    [SerializeField] private UnityEvent onDropZoneReached;
    [SerializeField] private UnityEvent onDashRequested;
    [SerializeField] private UnityEvent onDashStarted;
    [SerializeField] private UnityEvent onDashEnded;

    private bool isDashing;
    private bool canMove = true;
    private float currentSpeed;
    private int currentLane;
    private float targetLanePosition;
    private float touchStartX;
    private bool touchWasActive;

    private void Start()
    {
        currentSpeed = forwardSpeed;
        currentLane = laneCount / 2;
        targetLanePosition = GetLanePosition(currentLane);

        SnapToLaneImmediately();
    }

    private void Update()
    {
        if (!canMove)
        {
            return;
        }

        ReadLaneInput();
        UpdateForwardMovement();
        UpdateLaneMovement();
    }

    private void ReadLaneInput()
    {
        // Keyboard: one press = one lane change.
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ChangeLane(-1);
        }

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            ChangeLane(1);
        }

        // Touch/mouse: horizontal swipe = lane change.
        bool touchActive = Input.touchCount > 0;

        if (touchActive)
        {
            Touch touch = Input.GetTouch(0);

            if (!touchWasActive && touch.phase == TouchPhase.Began)
            {
                touchStartX = touch.position.x;
            }

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                float delta = touch.position.x - touchStartX;
                if (Mathf.Abs(delta) >= Screen.width * 0.12f)
                {
                    ChangeLane(delta > 0f ? 1 : -1);
                }
            }

            touchWasActive = true;
            return;
        }

        touchWasActive = false;

        // Mouse drag is useful for editor testing.
        if (Input.GetMouseButtonDown(0))
        {
            touchStartX = Input.mousePosition.x;
        }

        if (Input.GetMouseButtonUp(0))
        {
            float delta = Input.mousePosition.x - touchStartX;
            if (Mathf.Abs(delta) >= Screen.width * 0.12f)
            {
                ChangeLane(delta > 0f ? 1 : -1);
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) && !isDashing)
        {
            onDashRequested?.Invoke();
        }
    }

    private void UpdateForwardMovement()
    {
        float targetSpeed = forwardSpeed;

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            targetSpeed *= brakeMultiplier;
        }

        if (isDashing)
        {
            targetSpeed *= dashMultiplier;
        }

        currentSpeed = Mathf.MoveTowards(
            currentSpeed,
            targetSpeed,
            acceleration * Time.deltaTime);

        Vector3 movement = travelAlongX ? Vector3.right : Vector3.forward;
        transform.position += movement * currentSpeed * Time.deltaTime;
    }

    private void UpdateLaneMovement()
    {
        Vector3 position = transform.position;

        float axis = travelAlongX ? position.z : position.x;
        float newAxis = Mathf.SmoothDamp(
            axis,
            targetLanePosition,
            ref laneVelocity,
            1f / Mathf.Max(laneChangeResponse, 0.01f),
            laneChangeSpeed,
            Time.deltaTime);

        if (travelAlongX)
            position.z = newAxis;
        else
            position.x = newAxis;

        transform.position = position;
    }

    private float laneVelocity;

    public void ChangeLane(int direction)
    {
        int newLane = Mathf.Clamp(
            currentLane + direction,
            0,
            laneCount - 1);

        if (newLane == currentLane)
        {
            return;
        }

        currentLane = newLane;
        targetLanePosition = GetLanePosition(currentLane);
    }

    public void SetLane(int lane)
    {
        currentLane = Mathf.Clamp(lane, 0, laneCount - 1);
        targetLanePosition = GetLanePosition(currentLane);
    }

    private float GetLanePosition(int lane)
    {
        return laneCenterOffset +
               (lane - (laneCount - 1) * 0.5f) * laneWidth;
    }

    private void SnapToLaneImmediately()
    {
        Vector3 position = transform.position;

        if (travelAlongX)
            position.z = targetLanePosition;
        else
            position.x = targetLanePosition;

        transform.position = position;
    }

    public void StartDash()
    {
        if (canMove && !isDashing)
        {
            StartCoroutine(DashRoutine());
        }
    }

    public void Stop()
    {
        canMove = false;
        StopAllCoroutines();
        isDashing = false;
        currentSpeed = 0f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hazard"))
        {
            if (isDashing)
            {
                Destroy(other.gameObject);
                return;
            }

            Stop();
            onHazardHit?.Invoke();
            return;
        }

        if (other.CompareTag("DropZone"))
        {
            Stop();
            onDropZoneReached?.Invoke();
            Destroy(other.gameObject);
        }
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;
        onDashStarted?.Invoke();

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
        onDashEnded?.Invoke();
    }
}
