using UnityEngine;
using UnityEngine.Events;

public class PlayerGameplayState : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Gees")]
    [SerializeField] private float maxGees = 100f;
    [SerializeField] private float geesFillRate = 5f;

    [Header("Events")]
    [SerializeField] private UnityEvent onDashAuthorized;

    public float CurrentGees { get; private set; }
    public float MaxGees => maxGees;
    public float DistanceTravelled { get; private set; }
    public bool IsRunActive { get; private set; } = true;
    public bool IsDashAvailable => IsRunActive && CurrentGees >= MaxGees;

    public UnityEvent<float> OnGeesChanged = new UnityEvent<float>();
    public UnityEvent<bool> OnDashAvailabilityChanged = new UnityEvent<bool>();
    public UnityEvent<float> OnDistanceChanged = new UnityEvent<float>();

    private Vector3 lastPlayerPosition;

    private void Awake()
    {
        if (player == null)
        {
            player = transform;
        }

        lastPlayerPosition = player.position;
        CurrentGees = 0f;
        DistanceTravelled = 0f;

        RaiseGeesEvents();
        OnDistanceChanged.Invoke(DistanceTravelled);
    }

    private void Update()
    {
        if (!IsRunActive || player == null)
        {
            return;
        }

        float distanceDelta = Vector3.Distance(player.position, lastPlayerPosition);

        if (distanceDelta > 0f)
        {
            DistanceTravelled += distanceDelta;
            lastPlayerPosition = player.position;
            OnDistanceChanged.Invoke(DistanceTravelled);
        }
        else
        {
            lastPlayerPosition = player.position;
        }

        SetGees(CurrentGees + geesFillRate * Time.deltaTime);
    }

    public void RequestDash()
    {
        if (!IsRunActive || !IsDashAvailable)
        {
            return;
        }

        SetGees(0f);
        onDashAuthorized?.Invoke();
    }

    public void GameOver()
    {
        if (!IsRunActive)
        {
            return;
        }

        IsRunActive = false;
    }

    public void DeliveryCompleted()
    {
        IsRunActive = false;
    }

    public void ResetRun()
    {
        IsRunActive = true;
        DistanceTravelled = 0f;
        CurrentGees = 0f;

        if (player != null)
        {
            lastPlayerPosition = player.position;
        }

        RaiseGeesEvents();
        OnDistanceChanged.Invoke(DistanceTravelled);
    }

    private void SetGees(float value)
    {
        float previousGees = CurrentGees;
        bool previousDashAvailable = IsDashAvailable;

        CurrentGees = Mathf.Clamp(value, 0f, MaxGees);

        if (!Mathf.Approximately(previousGees, CurrentGees))
        {
            OnGeesChanged.Invoke(CurrentGees);
        }

        bool dashAvailable = IsDashAvailable;

        if (previousDashAvailable != dashAvailable)
        {
            OnDashAvailabilityChanged.Invoke(dashAvailable);
        }
    }

    private void RaiseGeesEvents()
    {
        OnGeesChanged.Invoke(CurrentGees);
        OnDashAvailabilityChanged.Invoke(IsDashAvailable);
    }
}

