using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Clothing category — must match the category expected by the target DressUpSlot.
/// </summary>
public enum ClothingCategory
{
    Hat,
    Top,
    Bottom,
    Shoes,
    Accessory
}

/// <summary>
/// Data component attached to each clothing sprite in the scene.
/// Defines the item's category, base style score, and theme tags used by OutfitScorer.
/// Pair with InputSpriteDrag so the player can drag it onto a DressUpSlot.
/// Common use: Every wearable item in the dress-up panel.
/// </summary>
public class ClothingItem : MonoBehaviour
{
    [Header("Item Identity")]
    [Tooltip("Display name shown in UI feedback (e.g. 'Red Beach Hat')")]
    [SerializeField] private string itemName = "New Item";

    [Tooltip("Which character slot this item fits into")]
    [SerializeField] private ClothingCategory category = ClothingCategory.Top;

    [Header("Scoring")]
    [Tooltip("Base style points this item contributes when equipped (0–30 recommended)")]
    [SerializeField] private int styleScore = 10;

    [Tooltip("Theme tags that match this item to challenge prompts (e.g. 'beach', 'formal', 'sporty'). " +
             "When a tag matches the active theme OutfitScorer awards bonus points.")]
    [SerializeField] private string[] themeTags = new string[0];

    [Header("Events")]
    /// <summary>
    /// Fires when this item is equipped to a DressUpSlot
    /// </summary>
    public UnityEvent onEquipped;

    /// <summary>
    /// Fires when this item is removed from a DressUpSlot
    /// </summary>
    public UnityEvent onUnequipped;

    // ── Runtime state ──────────────────────────────────────────────────────────

    private bool isEquipped = false;

    /// <summary>The display name of this clothing item</summary>
    public string ItemName => itemName;

    /// <summary>Which slot category this item belongs to</summary>
    public ClothingCategory Category => category;

    /// <summary>Base style score contributed when equipped</summary>
    public int StyleScore => styleScore;

    /// <summary>Returns true while this item is equipped to a slot</summary>
    public bool IsEquipped => isEquipped;

    /// <summary>
    /// Returns true if this item has the given theme tag (case-insensitive).
    /// Called by OutfitScorer when calculating the theme bonus.
    /// </summary>
    public bool HasThemeTag(string tag)
    {
        if (string.IsNullOrEmpty(tag)) return false;
        foreach (string t in themeTags)
        {
            if (string.Equals(t, tag, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Returns all theme tags on this item (read-only copy).
    /// </summary>
    public string[] GetThemeTags()
    {
        return (string[])themeTags.Clone();
    }

    // ── Called by DressUpSlot ──────────────────────────────────────────────────

    /// <summary>
    /// Marks this item as equipped and fires onEquipped.
    /// Called automatically by DressUpSlot — students do not need to call this directly.
    /// </summary>
    public void OnEquip()
    {
        isEquipped = true;
        onEquipped.Invoke();
    }

    /// <summary>
    /// Marks this item as unequipped and fires onUnequipped.
    /// Called automatically by DressUpSlot — students do not need to call this directly.
    /// </summary>
    public void OnUnequip()
    {
        isEquipped = false;
        onUnequipped.Invoke();
    }
}
