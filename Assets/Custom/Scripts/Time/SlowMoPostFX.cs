using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SlowMoPostFX : MonoBehaviour
{
    [SerializeField] private LocalTimeManager timeManager;
    [SerializeField] private Volume volume;

    [Header("Targets (máximos en slow)")]
    [SerializeField] private float bloomMax = 1.2f;
    [SerializeField] private float vignetteMax = 0.35f;
    [SerializeField] private float chromaMax = 0.3f;
    [SerializeField] private float rampUp = 0.12f;
    [SerializeField] private float rampDown = 0.2f;

    float bloomBase, vignetteBase, chromaBase;
    Bloom bloom; Vignette vig; ChromaticAberration chr;

    void Awake()
    {
        if (volume && volume.profile)
        {
            volume.profile.TryGet(out bloom);
            volume.profile.TryGet(out vig);
            volume.profile.TryGet(out chr);

            if (bloom)   bloomBase   = bloom.intensity.value;
            if (vig)     vignetteBase= vig.intensity.value;
            if (chr)     chromaBase  = chr.intensity.value;
        }
    }

    void OnEnable()
    {
        if (!timeManager) return;
        timeManager.SlowStarted += KickIn;
        timeManager.SlowEnded += KickOut;
    }
    void OnDisable()
    {
        if (!timeManager) return;
        timeManager.SlowStarted -= KickIn;
        timeManager.SlowEnded -= KickOut;
    }

    void KickIn()  { StopAllCoroutines(); StartCoroutine(LerpTo(bloomMax, vignetteMax, chromaMax, rampUp)); }
    void KickOut() { StopAllCoroutines(); StartCoroutine(LerpTo(bloomBase, vignetteBase, chromaBase, rampDown)); }

    System.Collections.IEnumerator LerpTo(float b, float v, float c, float time)
    {
        if (!volume) yield break;
        float t = 0f;
        float b0 = bloom ? bloom.intensity.value : 0f;
        float v0 = vig ? vig.intensity.value : 0f;
        float c0 = chr ? chr.intensity.value : 0f;
        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            float k = t / time;
            if (bloom) bloom.intensity.value = Mathf.Lerp(b0, b, k);
            if (vig)   vig.intensity.value   = Mathf.Lerp(v0, v, k);
            if (chr)   chr.intensity.value   = Mathf.Lerp(c0, c, k);
            yield return null;
        }
        if (bloom) bloom.intensity.value = b;
        if (vig)   vig.intensity.value   = v;
        if (chr)   chr.intensity.value   = c;
    }
}

