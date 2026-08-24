// LocalTimeManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LocalTimeManager : MonoBehaviour
{
    [Header("Objetos a ralentizar (arrástralos aquí)")]
    [Tooltip("Puedes arrastrar GameObjects raíz: se buscarán todos los componentes ITimeScalable en sus hijos.")]
    [SerializeField] private List<GameObject> groupsToSlow = new();

    [Header("Parámetros del efecto")]
    [Tooltip("Factor de tiempo local para las estructuras (1 = normal, 0.2 = muy lento).")]
    [Range(0f, 1f)]
    [SerializeField] private float slowScale = 0.2f;

    [Tooltip("Duración del efecto (segundos).")]
    [Min(0f)] [SerializeField] private float duration = 2f;

    [Tooltip("Cooldown antes de poder usarlo otra vez (segundos).")]
    [Min(0f)] [SerializeField] private float cooldown = 5f;

    [Header("Input")]
    [SerializeField] private InputReader inputReader;

    private readonly HashSet<ITimeScalable> targets = new();
    private bool isActive;
    private bool isCoolingDown;

    public event Action SlowStarted;
    public event Action SlowEnded;
    public event Action CooldownStarted;
    public event Action CooldownEnded;

    public bool IsActive => isActive;
    public bool IsCoolingDown => isCoolingDown;
    public bool CanActivate => !isActive && !isCoolingDown;
    public float RemainingDuration { get; private set; }
    public float RemainingCooldown { get; private set; }
    public float Duration => duration;
    public float Cooldown => cooldown;
    public float SlowScale => slowScale;

    void Awake()
    {
        RefreshTargets();
    }

    void Start()
    {
        if (inputReader == null)
        {
            inputReader = FindAnyObjectByType<InputReader>() as InputReader;
        }
        if (inputReader != null)
        {
            inputReader.SlowMoStarted += OnSlowMoInput;
        }
        else
        {
            Debug.LogWarning("LocalTimeManager: No se encontró InputReader. Slow-mo no funcionará.");
        }
    }

    void OnDestroy()
    {
        StopAllCoroutines();
        if (isActive)
        {
            ResetSlow();
        }
        isActive = false;
        isCoolingDown = false;
        RemainingDuration = 0f;
        RemainingCooldown = 0f;

        if (inputReader != null)
        {
            inputReader.SlowMoStarted -= OnSlowMoInput;
        }
    }

    private void OnSlowMoInput()
    {
        if (CanActivate)
        {
            StartCoroutine(SlowRoutine());
        }
    }

    private bool IsTimerSuspended()
    {
        return GameManager.Instance != null && GameManager.Instance.State != GameManager.GameState.Playing;
    }

    /// <summary> Escanea los GameObjects asignados y registra todos los ITimeScalable en sus hijos. </summary>
    public void RefreshTargets()
    {
        targets.Clear();
        foreach (var go in groupsToSlow)
        {
            if (go == null) continue;
            // Buscar TODOS los MonoBehaviours e intentar castear a ITimeScalable
            var monos = go.GetComponentsInChildren<MonoBehaviour>(includeInactive: true);
            foreach (var mb in monos)
            {
                if (mb is ITimeScalable scalable)
                    targets.Add(scalable);
            }
        }
    }

    private IEnumerator SlowRoutine()
    {
        isActive = true;
        RemainingDuration = duration;
        ApplySlow(slowScale);
        SlowStarted?.Invoke();

        while (RemainingDuration > 0f)
        {
            if (!IsTimerSuspended())
            {
                RemainingDuration = Mathf.Max(0f, RemainingDuration - Time.unscaledDeltaTime);
            }
            yield return null;
        }

        ResetSlow();
        isActive = false;
        SlowEnded?.Invoke();

        if (cooldown > 0f)
        {
            isCoolingDown = true;
            RemainingCooldown = cooldown;
            CooldownStarted?.Invoke();

            while (RemainingCooldown > 0f)
            {
                if (!IsTimerSuspended())
                {
                    RemainingCooldown = Mathf.Max(0f, RemainingCooldown - Time.unscaledDeltaTime);
                }
                yield return null;
            }

            isCoolingDown = false;
            CooldownEnded?.Invoke();
        }
    }

    public void ApplySlow(float scale)
    {
        scale = Mathf.Clamp01(scale);
        foreach (var target in targets)
        {
            if (IsTargetAlive(target)) target.SetTimeScale(scale);
        }
    }

    public void ResetSlow()
    {
        foreach (var target in targets)
        {
            if (IsTargetAlive(target)) target.SetTimeScale(1f);
        }
    }

    private static bool IsTargetAlive(ITimeScalable target)
    {
        return target != null && (!(target is UnityEngine.Object unityObject) || unityObject != null);
    }

    
}
