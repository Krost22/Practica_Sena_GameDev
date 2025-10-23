// LocalTimeAnimator.cs
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class LocalTimeAnimator : MonoBehaviour, ITimeScalable
{
    private Animator anim;
    private float baseSpeed;

    void Awake()
    {
        anim = GetComponent<Animator>();
        baseSpeed = anim.speed;
    }

    public void SetTimeScale(float scale)
    {
        anim.speed = baseSpeed * Mathf.Max(0f, scale);
    }
}
