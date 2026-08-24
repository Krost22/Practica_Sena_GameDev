// CooldownUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CooldownUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private LocalTimeManager timeManager;
    [SerializeField] private Image fill;               // Image con Fill (Radial o Horizontal)
    [SerializeField] private TextMeshProUGUI label;    // Opcional: texto "READY / x.xs"
    [SerializeField] private CanvasGroup pulse;        // Opcional: imagen/halo para pulso

    [Header("Colores")]
    [SerializeField] private Color readyColor = Color.cyan;
    [SerializeField] private Color activeColor = Color.yellow;
    [SerializeField] private Color cooldownColor = Color.gray;

    [Header("Pulso al activar")]
    [SerializeField] private float pulseDuration = 0.35f;
    [SerializeField] private float pulseScale = 1.2f;

    private Vector3 _pulseBaseScale;

    void Awake()
    {
        if (pulse) _pulseBaseScale = pulse.transform.localScale;
    }

    void OnEnable()
    {
        if (!timeManager) return;
        timeManager.SlowStarted += OnSlowStarted;
        timeManager.SlowEnded += OnSlowEnded;
        timeManager.CooldownStarted += OnCooldownStarted;
        timeManager.CooldownEnded += OnCooldownEnded;
    }

    void OnDisable()
    {
        if (!timeManager) return;
        timeManager.SlowStarted -= OnSlowStarted;
        timeManager.SlowEnded -= OnSlowEnded;
        timeManager.CooldownStarted -= OnCooldownStarted;
        timeManager.CooldownEnded -= OnCooldownEnded;
    }

    void Update()
    {
        if (!timeManager || !fill) return;

        if (timeManager.IsActive)
        {
            fill.fillAmount = timeManager.Duration > 0f
                ? Mathf.Clamp01(timeManager.RemainingDuration / timeManager.Duration)
                : 0f;
            fill.color = activeColor;
            if (label) label.text = $"¡PODER! {timeManager.RemainingDuration:0.0}s";
        }
        else if (timeManager.IsCoolingDown)
        {
            fill.fillAmount = timeManager.Cooldown > 0f
                ? 1f - Mathf.Clamp01(timeManager.RemainingCooldown / timeManager.Cooldown)
                : 1f;
            fill.color = cooldownColor;
            if (label) label.text = $"{timeManager.RemainingCooldown:0.0}s";
        }
        else
        {
            fill.fillAmount = 1f;
            fill.color = readyColor;
            if (label) label.text = "READY (Q)";
        }
    }

    // —— Eventos ——
    void OnSlowStarted()
    {
        if (pulse) StartCoroutine(PulseOnce());
    }
    void OnSlowEnded() { /* noop */ }
    void OnCooldownStarted() { /* noop */ }
    void OnCooldownEnded() { /* noop */ }

    System.Collections.IEnumerator PulseOnce()
    {
        if (!pulse) yield break;
        pulse.gameObject.SetActive(true);
        pulse.alpha = 1f;

        float t = 0f;
        while (t < pulseDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = t / pulseDuration;
            // scale up then back a poco
            float s = Mathf.Lerp(1f, pulseScale, 1f - (1f - k)*(1f - k));
            pulse.transform.localScale = _pulseBaseScale * s;
            pulse.alpha = 1f - k;
            yield return null;
        }

        pulse.transform.localScale = _pulseBaseScale;
        pulse.alpha = 0f;
        pulse.gameObject.SetActive(false);
    }
}
