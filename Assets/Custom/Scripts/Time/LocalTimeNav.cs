// LocalTimeNav.cs
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class LocalTimeNav : MonoBehaviour, ITimeScalable
{
    private NavMeshAgent agent;
    private float baseSpeed, baseAccel, baseAngular;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        baseSpeed   = agent.speed;
        baseAccel   = agent.acceleration;
        baseAngular = agent.angularSpeed;
    }

    public void SetTimeScale(float scale)
    {
        scale = Mathf.Max(0f, scale);
        agent.speed        = baseSpeed   * scale;
        agent.acceleration = baseAccel   * scale;
        agent.angularSpeed = baseAngular * scale;
    }
}
