using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Sistema de vidas/daño del jugador.
/// Se integra con RecoverySystem (al caer a lava) y GameManager (game over).
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private bool invincibleAfterDamage = true;
    [SerializeField] private float invincibilityDuration = 1.5f;

    [Header("Feedback Visual")]
    [SerializeField] private Renderer[] playerRenderers;
    [SerializeField] private float flashSpeed = 10f;

    [Header("Eventos")]
    public UnityEvent OnDamage;
    public UnityEvent OnDeath;

    private bool isInvincible = false;
    private float invincibilityTimer = 0f;

    void Update()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
                SetRenderersVisible(true);
            }
            else
            {
                // Parpadeo durante invencibilidad
                bool flash = Mathf.Sin(Time.time * flashSpeed) > 0;
                SetRenderersVisible(flash);
            }
        }
    }

    public void TakeDamage()
    {
        if (isInvincible) return;

        OnDamage?.Invoke();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoseLife();

            if (GameManager.Instance.CurrentLives <= 0)
            {
                OnDeath?.Invoke();
                return;
            }
        }

        if (invincibleAfterDamage)
        {
            isInvincible = true;
            invincibilityTimer = invincibilityDuration;
        }
    }

    public void Die()
    {
        OnDeath?.Invoke();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoseLife();
        }
    }

    private void SetRenderersVisible(bool visible)
    {
        if (playerRenderers == null) return;
        foreach (var r in playerRenderers)
        {
            if (r != null) r.enabled = visible;
        }
    }
}
