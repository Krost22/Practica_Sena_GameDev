using UnityEngine;

/// <summary>
/// Definición de un item del juego (ScriptableObject data-driven).
/// Crear instancias via: Create > Custom > Items > ItemData
/// </summary>
[CreateAssetMenu(fileName = "ItemData", menuName = "Custom/Items/ItemData", order = 0)]
public class ItemData : ScriptableObject
{
    [Header("Identificación")]
    public string itemId = "item_id";
    public string displayName = "Nuevo Item";
    [TextArea] public string description = "";
    public Sprite icon;

    [Header("Tipo")]
    public ItemType type = ItemType.Key;

    [Header("Stack")]
    public bool stackable = false;
    public int maxStack = 1;

    public enum ItemType
    {
        Key,        // Llaves que abren puertas
        Note,       // Notas/lore
        Tool,       // Herramientas (linterna, etc.)
        Consumable, // Curativos, etc.
        Quest       // Item de quest principal
    }
}
