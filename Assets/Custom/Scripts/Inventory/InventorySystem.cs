using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Sistema de inventario simple (singleton no persistente por nivel).
/// Gestiona items recogidos por el jugador. Notifica via UnityEvent al cambiar.
/// </summary>
public class InventorySystem : MonoBehaviour
{
    [System.Serializable]
    public class InventorySlot
    {
        public ItemData item;
        public int quantity;

        public InventorySlot(ItemData item, int quantity)
        {
            this.item = item;
            this.quantity = quantity;
        }
    }

    [Header("Inventario")]
    [SerializeField] private List<InventorySlot> slots = new List<InventorySlot>();
    [SerializeField] private int maxSlots = 12;

    [Header("Eventos")]
    public UnityEvent OnInventoryChanged;

    public static InventorySystem Instance { get; private set; }

    public List<InventorySlot> Slots => slots;
    public int SlotCount => slots.Count;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public bool AddItem(ItemData item, int quantity = 1)
    {
        if (item == null) return false;

        // Si es stackable, buscar slot existente
        if (item.stackable)
        {
            foreach (var slot in slots)
            {
                if (slot.item == item && slot.quantity < item.maxStack)
                {
                    slot.quantity = Mathf.Min(slot.quantity + quantity, item.maxStack);
                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }
        }

        // Crear slot nuevo si hay espacio
        if (slots.Count < maxSlots)
        {
            slots.Add(new InventorySlot(item, quantity));
            OnInventoryChanged?.Invoke();
            return true;
        }

        Debug.LogWarning($"[InventorySystem] Inventario lleno. No se pudo recoger: {item.displayName}");
        return false;
    }

    public bool RemoveItem(ItemData item, int quantity = 1)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].item == item)
            {
                slots[i].quantity -= quantity;
                if (slots[i].quantity <= 0)
                {
                    slots.RemoveAt(i);
                }
                OnInventoryChanged?.Invoke();
                return true;
            }
        }
        return false;
    }

    public bool HasItem(ItemData item)
    {
        foreach (var slot in slots)
        {
            if (slot.item == item) return true;
        }
        return false;
    }

    public bool HasItemById(string itemId)
    {
        foreach (var slot in slots)
        {
            if (slot.item != null && slot.item.itemId == itemId) return true;
        }
        return false;
    }

    public int GetQuantity(ItemData item)
    {
        foreach (var slot in slots)
        {
            if (slot.item == item) return slot.quantity;
        }
        return 0;
    }

    public void Clear()
    {
        slots.Clear();
        OnInventoryChanged?.Invoke();
    }
}
