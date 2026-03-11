using UnityEngine;
using TMPro; // We need this to talk to TextMeshPro
using UnityEngine.UI; // We need this to talk to the Slider

public class UIManager : MonoBehaviour
{
    [Header("UI Connections")]
    public TextMeshProUGUI distanceText;
    public Slider geesMeter;
    public Transform player;

    [Header("Gees Settings")]
    public float maxGees = 100f;
    public float geesFillRate = 5f; // How fast it fills per second of survival

    private float startingZ;
    private float currentGees = 0f;
    private bool isPlayerAlive = true;

    void Start()
    {
        // Remember where the scooter started so we can calculate exact distance
        if (player != null)
        {
            startingZ = player.position.z;
        }

        // Setup the Gees bar
        if (geesMeter != null)
        {
            geesMeter.minValue = 0;
            geesMeter.maxValue = maxGees;
            geesMeter.value = currentGees;
        }
    }

    void Update()
    {
        if (player == null || !isPlayerAlive) return;

        // 1. UPDATE THE DISTANCE SCORE
        float distanceDriven = player.position.z - startingZ;
        distanceText.text = Mathf.FloorToInt(distanceDriven).ToString() + "m";

        // 2. FILL THE GEES METER
        if (currentGees < maxGees)
        {
            currentGees += geesFillRate * Time.deltaTime;

            // THE OVERRIDE: If it gets close, forcefully snap it to exactly 100
            if (currentGees >= maxGees)
            {
                currentGees = maxGees;
                Debug.Log("UI MANAGER: GEES IS 100% FULL! READY TO DASH!");
            }

            geesMeter.value = currentGees;
        }
    }

    // We will call this when the scooter crashes
    public void GameOver()
    {
        isPlayerAlive = false;
        distanceText.color = Color.red; // Paint the score red on death
        Debug.Log("UI MANAGER: Game Over state triggered.");
    }

    // The Scooter will ask this to check if it's allowed to Dash
    public bool IsGeesFull()
    {
        return currentGees >= maxGees;
    }

    // The Scooter will call this to empty the bar after Dashing
    public void SpendGees()
    {
        currentGees = 0f;
        geesMeter.value = currentGees;
    }
}