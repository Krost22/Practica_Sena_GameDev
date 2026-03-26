// LocalTimeAnimator.cs
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class LocalTimeAnimator : MonoBehaviour, ITimeScalable
{
    private Animator anim;
    private float baseSpeed;
    private bool isInitialized;

    void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (isInitialized) return;

        anim = GetComponent<Animator>();
        if (anim != null)
        {
            baseSpeed = anim.speed;
        }
        else
        {
            baseSpeed = 1f;
        }
        isInitialized = true;
    }

    public void SetTimeScale(float scale)
    {
        Initialize(); // Aeguramos que el animador se obtenga, útil si LocalTimeManager llama esto a modelos inactivos
        if (anim != null)
        {
            anim.speed = baseSpeed * Mathf.Max(0f, scale);
        }
    }
}
