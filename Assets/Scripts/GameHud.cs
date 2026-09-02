using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameHud : MonoBehaviour
{
    [Header("Gameplay")]
    [SerializeField] private Transform player;
    [SerializeField] private PlayerGameplayState gameplayState;
    [SerializeField] private float deliveryDistance = 38f;

    private TextMeshProUGUI distanceLabel;
    private TextMeshProUGUI geesLabel;

    private void Awake()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            enabled = false;
            return;
        }

        if (gameplayState == null && player != null)
        {
            gameplayState = player.GetComponent<PlayerGameplayState>();
        }

        if (gameplayState == null)
        {
            gameplayState = FindFirstObjectByType<PlayerGameplayState>();
        }

        ConfigureScaler();
        BuildHud();

        if (player != null)
        {
            CreateDeliveryBeacon();
        }
    }

    private void OnEnable()
    {
        SubscribeToGameplayState();
    }

    private void Start()
    {
        // Awake may have resolved gameplayState after OnEnable.
        SubscribeToGameplayState();
        RefreshFromState();
    }

    private void OnDisable()
    {
        UnsubscribeFromGameplayState();
    }

    private void SubscribeToGameplayState()
    {
        if (gameplayState == null)
        {
            return;
        }

        gameplayState.OnDistanceChanged.AddListener(UpdateDistance);
        gameplayState.OnGeesChanged.AddListener(UpdateGees);
    }

    private void UnsubscribeFromGameplayState()
    {
        if (gameplayState == null)
        {
            return;
        }

        gameplayState.OnDistanceChanged.RemoveListener(UpdateDistance);
        gameplayState.OnGeesChanged.RemoveListener(UpdateGees);
    }

    private void RefreshFromState()
    {
        if (gameplayState == null)
        {
            return;
        }

        UpdateDistance(gameplayState.DistanceTravelled);
        UpdateGees(gameplayState.CurrentGees);
    }

    private void UpdateDistance(float distance)
    {
        if (distanceLabel != null)
        {
            distanceLabel.text = $"{Mathf.FloorToInt(distance)} m";
        }
    }

    private void UpdateGees(float gees)
    {
        if (geesLabel != null && gameplayState != null)
        {
            float percentage = gameplayState.MaxGees <= 0f
                ? 0f
                : (gees / gameplayState.MaxGees) * 100f;

            geesLabel.text = $"GEES {Mathf.FloorToInt(percentage)}%";
        }
    }

    private void ConfigureScaler()
    {
        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler == null) return;

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
    }

    private void BuildHud()
    {
        CreatePanel("TopBar", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -42f), new Vector2(720f, 72f), new Color(0.035f, 0.055f, 0.075f, 0.88f));
        CreateText("Pakkie Panic", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -42f), new Vector2(600f, 56f), 36, TextAlignmentOptions.Center, Color.white, FontStyles.Bold);

        distanceLabel = CreateText("0 m", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(74f, -72f), new Vector2(240f, 50f), 30, TextAlignmentOptions.Left, Color.white, FontStyles.Bold);
        CreatePanel("GeesPanel", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-135f, -72f), new Vector2(235f, 50f), new Color(0.055f, 0.12f, 0.085f, 0.88f));
        geesLabel = CreateText("GEES 0%", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-135f, -72f), new Vector2(220f, 42f), 22, TextAlignmentOptions.Center, new Color(0.45f, 1f, 0.55f), FontStyles.Bold);

        CreatePanel("DeliveryPrompt", new Vector2(0.5f, 0.74f), new Vector2(0.5f, 0.74f), Vector2.zero, new Vector2(290f, 58f), new Color(0.05f, 0.28f, 0.12f, 0.9f));
        CreateText("DELIVER HERE", new Vector2(0.5f, 0.74f), new Vector2(0.5f, 0.74f), Vector2.zero, new Vector2(280f, 50f), 24, TextAlignmentOptions.Center, Color.white, FontStyles.Bold);

        CreateControl("SWIPE LEFT", "Change lane", new Vector2(0f, 0f), new Vector2(180f, 145f));
        CreateControl("SWIPE RIGHT", "Change lane", new Vector2(1f, 0f), new Vector2(-180f, 145f));
        CreatePanel("ActionHint", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 108f), new Vector2(310f, 44f), new Color(0.02f, 0.025f, 0.04f, 0.82f));
        CreateText("TAP: BRAKE / BOOST", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 108f), new Vector2(300f, 36f), 18, TextAlignmentOptions.Center, Color.white, FontStyles.Bold);

        CreatePanel("BottomBar", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 38f), new Vector2(660f, 32f), new Color(0.035f, 0.05f, 0.07f, 0.88f));
        CreatePanel("Progress", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-85f, 38f), new Vector2(420f, 20f), new Color(0.18f, 0.72f, 0.32f, 0.95f));
        CreateText("DELIVERY RUN", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 38f), new Vector2(620f, 30f), 16, TextAlignmentOptions.Right, Color.white, FontStyles.Bold);
    }

    private void CreateControl(string title, string subtitle, Vector2 anchor, Vector2 position)
    {
        CreatePanel(title, anchor, anchor, position, new Vector2(260f, 96f), new Color(0.04f, 0.055f, 0.08f, 0.82f));
        CreateText(title, anchor, anchor, position + new Vector2(0f, 14f), new Vector2(240f, 34f), 22, TextAlignmentOptions.Center, Color.white, FontStyles.Bold);
        CreateText(subtitle, anchor, anchor, position + new Vector2(0f, -21f), new Vector2(240f, 28f), 17, TextAlignmentOptions.Center, new Color(0.78f, 0.84f, 0.88f), FontStyles.Normal);
    }

    private void CreateDeliveryBeacon()
    {
        Vector3 forward = Vector3.ProjectOnPlane(player.forward, Vector3.up).normalized;
        GameObject beacon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        beacon.name = "Delivery Beacon";
        beacon.tag = "DropZone";
        beacon.transform.position = player.position + forward * deliveryDistance + Vector3.up * 0.06f;
        beacon.transform.localScale = new Vector3(1.6f, 0.06f, 1.6f);

        Collider beaconCollider = beacon.GetComponent<Collider>();
        beaconCollider.isTrigger = true;

        Light beaconLight = beacon.AddComponent<Light>();
        beaconLight.color = new Color(0.25f, 1f, 0.42f);
        beaconLight.range = 8f;
        beaconLight.intensity = 4f;

        Renderer renderer = beacon.GetComponent<Renderer>();
        renderer.material.color = new Color(0.16f, 0.95f, 0.34f, 0.8f);
    }

    private Image CreatePanel(string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(transform, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = panel.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private TextMeshProUGUI CreateText(string value, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size, float fontSize, TextAlignmentOptions alignment, Color color, FontStyles style)
    {
        GameObject label = new GameObject(value, typeof(RectTransform), typeof(TextMeshProUGUI));
        label.transform.SetParent(transform, false);

        RectTransform rect = label.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TextMeshProUGUI text = label.GetComponent<TextMeshProUGUI>();
        text.font = TMP_Settings.defaultFontAsset;
        text.text = value;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.fontStyle = style;
        text.raycastTarget = false;
        return text;
    }
}
