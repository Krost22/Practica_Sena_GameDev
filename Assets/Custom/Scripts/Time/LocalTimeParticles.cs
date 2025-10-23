// LocalTimeParticles.cs
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class LocalTimeParticles : MonoBehaviour, ITimeScalable
{
    private ParticleSystem ps;
    private ParticleSystem.MainModule main;
    private float baseSim;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        main = ps.main;
        baseSim = main.simulationSpeed;
    }

    public void SetTimeScale(float scale)
    {
        main.simulationSpeed = baseSim * Mathf.Max(0f, scale);
    }
}
