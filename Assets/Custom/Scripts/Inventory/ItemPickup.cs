using UnityEngine;

/// <summary>
/// Componente para items recogibles en el mundo.
/// Al interactuar (E) o al tocar, añade el item al InventorySystem.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ItemPickup : MonoBehaviour
{
    [Header("Item")]
    [SerializeField] private ItemData itemData;
    [SerializeField] private int quantity = 1;

    [Header("Modo de recogida")]
    [SerializeField] private bool pickupOnTouch = false; // true = al caminar encima; false = requiere Interact
    [SerializeField] private string playerTag = "Player";

    [Header("Feedback")]
    [SerializeField] private GameObject pickupEffect;
    [SerializeField] private AudioClip pickupSound;

    [Header("Input")]
    [SerializeField] private InputReader inputReader;

    private bool playerInRange = false;

    void Start()
    {
        if (inputReader == null)
        {
            inputReader = FindAnyObjectByType<InputReader>() as InputReader;
        }
        if (inputReader != null && !pickupOnTouch)
        {
            inputReader.InteractStarted += OnInteract;
        }
    }

    void OnDestroy()
    {
        if (inputReader != null)
        {
            inputReader.InteractStarted -= OnInteract;
        }
    }

    private void OnInteract()
    {
        if (playerInRange)
        {
            Pickup();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (pickupOnTouch)
            {
                Pickup();
            }
            else
            {
                playerInRange = true;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
        }
    }

    private void Pickup()
    {
        if (itemData == null)
        {
            Debug.LogWarning($"[ItemPickup] {gameObject.name} no tiene ItemData asignado.");
            return;
        }

        if (InventorySystem.Instance != null)
        {
            bool added = InventorySystem.Instance.AddItem(itemData, quantity);
            if (added)
            {
                if (pickupEffect != null)
                {
                    Instantiate(pickupEffect, transform.position, Quaternion.identity);
                }
                if (pickupSound != null && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX3D(pickupSound, transform.position);
                }
                Destroy(gameObject);
            }
        }
        else
        {
            Debug.LogWarning("[ItemPickup] No hay InventorySystem en la escena.");
        }
    }
}
