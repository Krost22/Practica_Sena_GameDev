using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasGroup))]
public class ChronoBar : MonoBehaviour
{
    [SerializeField] private LocalTimeManager timeManager;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private Transform target;
    [SerializeField] private Image fill;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Posicion")]
    [SerializeField] private float height = 1.8f;
    [SerializeField] private float horizontalOffset = 0.75f;
    [SerializeField] private float referenceDistance = 10f;
    [SerializeField] private float baseScale = 0.006f;
    [SerializeField] private float minScale = 0.0035f;
    [SerializeField] private float maxScale = 0.012f;

    [Header("Colores")]
    [SerializeField] private Color readyColor = new(0.3f, 0.95f, 1f, 1f);
    [SerializeField] private Color activeColor = new(1f, 0.78f, 0.2f, 1f);
    [SerializeField] private Color cooldownColor = new(0.45f, 0.65f, 0.8f, 1f);

    private void Awake()
    {
        if (target == null) target = transform.parent;
        if (cameraController == null && target != null) cameraController = target.GetComponent<CameraController>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (timeManager == null) timeManager = FindAnyObjectByType<LocalTimeManager>();

        Canvas canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 100;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void Update()
    {
        if (timeManager == null || fill == null)
        {
            canvasGroup.alpha = 0f;
            return;
        }

        if (timeManager.IsActive)
        {
            fill.fillAmount = timeManager.Duration > 0f
                ? Mathf.Clamp01(timeManager.RemainingDuration / timeManager.Duration)
                : 0f;
            fill.color = activeColor;
            canvasGroup.alpha = 1f;
        }
        else if (timeManager.IsCoolingDown)
        {
            fill.fillAmount = timeManager.Cooldown > 0f
                ? 1f - Mathf.Clamp01(timeManager.RemainingCooldown / timeManager.Cooldown)
                : 1f;
            fill.color = cooldownColor;
            canvasGroup.alpha = 0.9f;
        }
        else
        {
            fill.fillAmount = 1f;
            fill.color = readyColor;
            canvasGroup.alpha = 0.65f;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Camera activeCamera = cameraController != null ? cameraController.GetActiveCamera() : Camera.main;
        if (activeCamera == null) return;

        Transform cameraTransform = activeCamera.transform;
        transform.position = target.position + Vector3.up * height + cameraTransform.right * horizontalOffset;
        transform.rotation = cameraTransform.rotation;

        float distance = Vector3.Distance(cameraTransform.position, transform.position);
        float scale = baseScale * distance / Mathf.Max(0.01f, referenceDistance);
        transform.localScale = Vector3.one * Mathf.Clamp(scale, minScale, maxScale);
    }
}
