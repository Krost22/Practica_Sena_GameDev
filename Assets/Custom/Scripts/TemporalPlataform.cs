using System.Collections;
using UnityEngine;

// Implementa ITimeScalable para que el slow-mo afecte su ciclo (consistencia con CrumblingPlatform)
public class TemporalPlataform : MonoBehaviour, ITimeScalable
{
    public float tiempoActivo = 3f;         // Tiempo que la plataforma está totalmente visible
    public float tiempoInactivo = 2f;       // Tiempo que la plataforma permanece desactivada
    public float tiempoDesvanecimiento = 1f; // Tiempo que tarda en desaparecer (fade out)
    public float tiempoAparicion = 1f;       // Tiempo que tarda en aparecer (fade in)

    private MeshRenderer meshRenderer;
    private Collider col;
    private Material mat;
    private float alpha = 1f;

    // Factor de tiempo local (1 = normal, 0.2 = slow-mo)
    private float _timeScale = 1f;

    public void SetTimeScale(float scale)
    {
        _timeScale = scale;
    }

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        col = GetComponent<Collider>();
        // Asume que el material con el shader está asignado
        mat = meshRenderer.material;

        StartCoroutine(CicloPlataforma());
    }

    IEnumerator CicloPlataforma()
    {
        while (true)
        {
            // 1. APARECER (Fade In)
            meshRenderer.enabled = true;
            col.enabled = true;
            yield return StartCoroutine(AparecerPlataforma());

            // 2. Permanecer totalmente visible por 'tiempoActivo'
            yield return StartCoroutine(WaitScaled(tiempoActivo));

            // 3. DESVANECER (Fade Out)
            yield return StartCoroutine(DesvanecerPlataforma());
            meshRenderer.enabled = false;
            col.enabled = false;

            // 4. Permanecer inactiva por 'tiempoInactivo'
            yield return StartCoroutine(WaitScaled(tiempoInactivo));
        }
    }

    // Espera respetando el timeScale local (slow-mo)
    IEnumerator WaitScaled(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime * _timeScale;
            yield return null;
        }
    }

    IEnumerator AparecerPlataforma()
    {
        float t = 0f;
        while (t < tiempoAparicion)
        {
            t += Time.deltaTime * _timeScale;
            alpha = Mathf.Lerp(0f, 1f, t / tiempoAparicion);
            mat.SetFloat("_Transparency", alpha);
            yield return null;
        }
    }

    IEnumerator DesvanecerPlataforma()
    {
        float t = 0f;
        while (t < tiempoDesvanecimiento)
        {
            t += Time.deltaTime * _timeScale;
            alpha = Mathf.Lerp(1f, 0f, t / tiempoDesvanecimiento);
            mat.SetFloat("_Transparency", alpha);
            yield return null;
        }
    }
}
