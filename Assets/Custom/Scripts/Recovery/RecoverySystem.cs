using UnityEngine;

public class RecoverySystem : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject Keyitem;
    [SerializeField] private ParticleSystem RecoveryEffect;
    [SerializeField] private AudioSource RecoveryAudio;
    [SerializeField] private GameObject recoveryPlayer;
    [SerializeField] private GameObject recoveryKeyitem;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            //Se coloca el jugador en la posición de recuperación
            player.transform.position = recoveryPlayer.transform.position;
            OnRecoveryVFX();
        }
        if (other.gameObject == Keyitem)
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
