using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A clothing slot on the character that accepts drag-and-dropped ClothingItems of the matching category.
/// Place this component on each body-part child of the character (e.g. Slot_Hat, Slot_Top).
/// Requires a Collider2D set to Is Trigger so dragged items can overlap it.
/// The SpriteRenderer on this GameObject displays the currently equipped item.
/// Common use: Hat slot, Top slot, Bottom slot, Shoes slot, Accessory slot.
/// </summary>
public class DressUpSlot : MonoBehaviour
{
    [Header("Slot Settings")]
    [Tooltip("Which clothing category this slot accepts")]
    [SerializeField] private ClothingCategory acceptedCategory = ClothingCategory.Top;

    [Tooltip("Sprite shown when no item is equipped (leave empty to hide the slot sprite)")]
    [SerializeField] private Sprite emptySprite;

    [Header("Drop Feedback")]
    [Tooltip("Scale punch played when an item is successfully equipped")]
    [SerializeField] private float equipPunchStrength = 0.15f;

    [Tooltip("Duration of the equip punch animation")]
    [SerializeField] private float equipPunchDuration = 0.3f;

    [Header("Events")]
    /// <summary>
    /// Fires when a clothing item is successfully equipped to this slot
    /// </summary>
    public UnityEvent onItemEquipped;

    /// <summary>
    /// Fires when the currently equipped item is removed
    /// </summary>
    public UnityEvent onItemRemoved;

    /// <summary>
    /// Fires when the slot becomes empty (after remove, or at start if no item equipped)
    /// </summary>
    public UnityEvent onSlotEmpty;

    // ── Runtime state ──────────────────────────────────────────────────────────

    private ClothingItem equippedItem = null;
    private SpriteRenderer slotRenderer;
    private ClothingItem pendingItem = null;  // item overlapping the trigger while being dragged

    /// <summary>The category of clothing this slot accepts</summary>
    public ClothingCategory AcceptedCategory => acceptedCategory;

    /// <summary>Returns true if a clothing item is currently equipped</summary>
    public bool IsOccupied => equippedItem != null;

    /// <summary>Reference to the currently equipped ClothingItem (null if empty)</summary>
    public ClothingItem EquippedItem => equippedItem;

    private void Awake()
    {
        slotRenderer = GetComponent<SpriteRenderer>();

        if (GetComponent<Collider2D>() == null)
            Debug.LogWarning($"[DressUpSlot] '{gameObject.name}' needs a Collider2D set to Is Trigger.", this);
    }

    private void Start()
    {
        ShowEmptySprite();
    }

    // ── Trigger detection ──────────────────────────────────────────────────────

    // Track which draggable item is overlapping this slot
    private void OnTriggerEnter2D(Collider2D other)
    {
        ClothingItem item = other.GetComponent<ClothingItem>();
        if (item == null) return;
        if (item.Category != acceptedCategory) return;

        InputSpriteDrag drag = other.GetComponent<InputSpriteDrag>();
        if (drag == null || !drag.IsDragging) return;

        pendingItem = item;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        ClothingItem item = other.GetComponent<ClothingItem>();
        if (item != null && item == pendingItem)
            pendingItem = null;
    }

    private void Update()
    {
        // When the pending item stops dragging while still inside this trigger, equip it
        if (pendingItem == null) return;

        InputSpriteDrag drag = pendingItem.GetComponent<InputSpriteDrag>();
        if (drag == null) return;

        if (!drag.IsDragging)
        {
            EquipItem(pendingItem, drag);
            pendingItem = null;
        }
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Equips a clothing item to this slot. Unequips any previously equipped item first.
    /// Students can also call this directly from a UnityEvent if needed.
    /// </summary>
    public void Equip(ClothingItem item)
    {
        InputSpriteDrag drag = item != null ? item.GetComponent<InputSpriteDrag>() : null;
        EquipItem(item, drag);
    }

    /// <summary>
    /// Removes the currently equipped item and returns it to its origin position.
    /// Wire to a click event on the slot to let players unequip by clicking.
    /// </summary>
    public void Unequip()
    {
        if (equippedItem == null) return;

        ClothingItem removed = equippedItem;
        equippedItem = null;

        removed.OnUnequip();

        // Re-enable the item's sprite (it was hidden when equipped) then return it
        SpriteRenderer itemRenderer = removed.GetComponent<SpriteRenderer>();
        if (itemRenderer != null)
            itemRenderer.enabled = true;

        InputSpriteDrag drag = removed.GetComponent<InputSpriteDrag>();
        if (drag != null)
            drag.ReturnToOrigin();

        ShowEmptySprite();
        onItemRemoved.Invoke();
        onSlotEmpty.Invoke();
    }

    /// <summary>
    /// Removes the equipped item silently (no return animation) — useful for scene resets.
    /// </summary>
    public void UnequipSilent()
    {
        if (equippedItem == null) return;

        ClothingItem removed = equippedItem;
        equippedItem = null;
        removed.OnUnequip();

        SpriteRenderer itemRenderer = removed.GetComponent<SpriteRenderer>();
        if (itemRenderer != null)
            itemRenderer.enabled = true;

        InputSpriteDrag drag = removed.GetComponent<InputSpriteDrag>();
        drag?.ReturnToOrigin();

        ShowEmptySprite();
        onItemRemoved.Invoke();
    }

    // ── Internal ───────────────────────────────────────────────────────────────

    private void EquipItem(ClothingItem item, InputSpriteDrag drag)
    {
        if (item == null) return;

        // Unequip whatever is already here
        if (equippedItem != null && equippedItem != item)
            Unequip();

        equippedItem = item;

        // Tell the drag component it was accepted — prevents return-to-origin
        drag?.NotifyDropped();

        // Mirror the clothing item's sprite onto this slot's renderer
        SpriteRenderer itemRenderer = item.GetComponent<SpriteRenderer>();
        if (slotRenderer != null && itemRenderer != null)
            slotRenderer.sprite = itemRenderer.sprite;

        // Hide the item's own sprite so only the slot shows it
        if (itemRenderer != null)
            itemRenderer.enabled = false;

        item.OnEquip();

        // Pop animation
        transform.DOKill();
        transform.DOPunchScale(Vector3.one * equipPunchStrength, equipPunchDuration, 6, 0.5f);

        onItemEquipped.Invoke();
    }

    private void ShowEmptySprite()
    {
        if (slotRenderer != null)
            slotRenderer.sprite = emptySprite;
    }
}
