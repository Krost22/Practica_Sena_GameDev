using UnityEngine;

public class RecoverySystem : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject Keyitem;
    [SerializeField] private ParticleSystem RecoveryEffect;
    [SerializeField] private AudioSource RecoveryAudio;
    [SerializeField] private GameObject recoveryPlayer;
    [SerializeField] private GameObject recoveryKeyitem;

    [Header("Penalización")]
    [SerializeField] private bool loseLifeOnRecovery = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Penalización: perder una vida al caer (si PlayerHealth está presente)
            if (loseLifeOnRecovery)
            {
                PlayerHealth health = player.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.TakeDamage();
                }
            }

            // Para teletransportar un objeto con CharacterController, primero hay que apagarlo
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            // Se asinga la nueva posición
            player.transform.position = recoveryPlayer.transform.position;

            // Se vuelve a encender
            if (cc != null) cc.enabled = true;

            OnRecoveryVFX();
        }
        if (other.CompareTag("Box"))
        {
            Keyitem.transform.position = recoveryKeyitem.transform.position;
        }
    }

//Al revivir el jugador, se activa el efecto de recuperación
    private void OnRecoveryVFX()
    {
        if (RecoveryEffect != null)
        {
            RecoveryEffect.Play();
        }

        if (RecoveryAudio != null)
        {
            // Genera un pitch aleatorio entre 1.30 y 2.0
            RecoveryAudio.pitch = Random.Range(1.30f, 2.0f);
            RecoveryAudio.Play();
        }
    }

}
