using UnityEngine;
using System.Collections;

// Evitamos usar un namespace si ITimeScalable está en el namespace global (como LocalTimeManager).
public class CrumblingPlatform : MonoBehaviour, ITimeScalable
{
    [Header("Configuración de Caída")]
    [SerializeField] private float fallDelay = 0.15f;   // Cuánto tarda en caer tras pisarse (tiempo normal)
    [SerializeField] private float fallSpeed = 30f;     // Velocidad de caída hacia la lava
    [SerializeField] private float shakeAmount = 0.08f; // Intensidad del temblor
    [SerializeField] private float respawnTime = 3f;    // Opcional: Tiempo en reaparecer
    
    [Header("Visual")]
    [SerializeField] private GameObject platformModel;  // La malla a agitar

    private Vector3 _originalPosition;
    private bool _isTriggered = false;
    
    // Almacena el factor de tiempo actual (1 = normal, 0.2 = cámara lenta, etc)
    private float _timeScale = 1f;

    private Collider[] _colliders;
    private Renderer[] _renderers;

    // Métodos de la interfaz del usuario
    public void SetTimeScale(float scale)
    {
        _timeScale = scale;
    }

    private void Start()
    {
        _originalPosition = transform.position;
        
        // Recolectar visuales y físicas para el respawn
        _colliders = GetComponentsInChildren<Collider>();
        _renderers = GetComponentsInChildren<Renderer>();
        
        if (platformModel == null) platformModel = gameObject;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Se activa si lo pisa el jugador o una caja
        if (!_isTriggered && (other.CompareTag("Player") || other.CompareTag("Box")))
        {
            StartCoroutine(CrumbleSequence());
        }
    }

    private IEnumerator CrumbleSequence()
    {
        _isTriggered = true;

        // FASE 1: Temblor
        float elapsed = 0f;
        while (elapsed < fallDelay)
        {
            // Agitar sutilmente en horizontal
            Vector2 randomShake = Random.insideUnitCircle * shakeAmount;
            platformModel.transform.position = _originalPosition + new Vector3(randomShake.x, 0, randomShake.y);
            
            // Ajustamos el tiempo con el factor local (TimeScale)
            elapsed += Time.deltaTime * _timeScale; 
            yield return null;
        }

        // Realinear antes de caer
        platformModel.transform.position = _originalPosition;

        // FASE 2: Caída a la lava
        elapsed = 0f;
        while (elapsed < 1.5f) // Duración total de la animación de caída
        {
            // Velocidad afectada por _timeScale
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime * _timeScale, Space.World);
            elapsed += Time.deltaTime * _timeScale;
            yield return null;
        }

        // FASE 3: Desactivar bloque
        SetPlatformActive(false);

        // FASE 4: Respawn si se indica
        if (respawnTime > 0)
        {
            float respawnElapsed = 0f;
            while(respawnElapsed < respawnTime)
            {
                respawnElapsed += Time.deltaTime * _timeScale;
                yield return null;
            }
            
            transform.position = _originalPosition;
            platformModel.transform.position = _originalPosition;
            SetPlatformActive(true);
            
            _isTriggered = false;
        }
    }

    private void SetPlatformActive(bool isActive)
    {
        foreach (var col in _colliders) col.enabled = isActive;
        foreach (var ren in _renderers) ren.enabled = isActive;
    }
}
