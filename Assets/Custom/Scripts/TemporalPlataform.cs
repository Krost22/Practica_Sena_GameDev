using System.Collections;
using UnityEngine;

public class TemporalPlataform : MonoBehaviour
{
    public float tiempoActivo = 3f;         // Tiempo que la plataforma está totalmente visible
    public float tiempoInactivo = 2f;       // Tiempo que la plataforma permanece desactivada
    public float tiempoDesvanecimiento = 1f; // Tiempo que tarda en desaparecer (fade out)
    public float tiempoAparicion = 1f;       // Tiempo que tarda en aparecer (fade in)

    private MeshRenderer meshRenderer;
    private Collider col;
    private Material mat;
    private float alpha = 1f;

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
            yield return new WaitForSeconds(tiempoActivo);

            // 3. DESVANECER (Fade Out)
            yield return StartCoroutine(DesvanecerPlataforma());
            meshRenderer.enabled = false;
            col.enabled = false;

            // 4. Permanecer inactiva por 'tiempoInactivo'
            yield return new WaitForSeconds(tiempoInactivo);
        }
    }

    IEnumerator AparecerPlataforma()
    {
        float t = 0f;
        while (t < tiempoAparicion)
        {
            t += Time.deltaTime;
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
            t += Time.deltaTime;
            alpha = Mathf.Lerp(1f, 0f, t / tiempoDesvanecimiento);
            mat.SetFloat("_Transparency", alpha);
            yield return null;
        }
    }
}
