using UnityEngine;

/// <summary>
/// ScriptableObject asset that stores all data for a single clothing item,
/// including its sprite, canvas position, category, and scoring metadata.
/// Create via Assets > Create > Dress To Impress > Clothing Item Data.
/// </summary>
[CreateAssetMenu(fileName = "NewClothingItem", menuName = "Dress To Impress/Clothing Item Data")]
public class ClothingItemData : ScriptableObject
{
    /// <summary>Display name shown in the wardrobe UI.</summary>
    [SerializeField] private string itemName;

    /// <summary>The sprite rendered on the character for this item.</summary>
    [SerializeField] private Sprite sprite;

    /// <summary>Which clothing layer slot this item occupies.</summary>
    [SerializeField] private ClothingCategory category;

    /// <summary>
    /// Horizontal pixel position of the item's top-left corner
    /// in the 2048×2048 Krita source canvas.
    /// </summary>
    [SerializeField] private int canvasX;

    /// <summary>
    /// Vertical pixel position of the item's top-left corner
    /// in the 2048×2048 Krita source canvas (Y increases downward).
    /// </summary>
    [SerializeField] private int canvasY;

    /// <summary>
    /// Index of the color variant for this item within its group.
    /// Defaults to 1 (the first color variant).
    /// </summary>
    [SerializeField] private int colorVariantIndex = 1;

    /// <summary>
    /// Shared base name that groups color variants of the same item shape
    /// together (e.g. "beret1"). Color suffix is separate.
    /// </summary>
    [SerializeField] private string groupName;

    /// <summary>
    /// Base style score awarded when this item contributes to an outfit.
    /// Higher values indicate rarer or more fashion-forward items.
    /// </summary>
    [SerializeField] private int styleScore = 10;

    /// <summary>
    /// Theme tags used by judges to evaluate outfit compatibility
    /// (e.g. "Gothic", "Glam", "Casual").
    /// </summary>
    [SerializeField] private string[] themeTags;

    // ── Public read-only properties ────────────────────────────────────────

    /// <summary>Display name shown in the wardrobe UI.</summary>
    public string ItemName => itemName;

    /// <summary>The sprite rendered on the character for this item.</summary>
    public Sprite Sprite => sprite;

    /// <summary>Which clothing layer slot this item occupies.</summary>
    public ClothingCategory Category => category;

    /// <summary>Horizontal top-left canvas pixel coordinate (Krita space).</summary>
    public int CanvasX => canvasX;

    /// <summary>Vertical top-left canvas pixel coordinate (Krita space, Y-down).</summary>
    public int CanvasY => canvasY;

    /// <summary>Color variant index within the item's group.</summary>
    public int ColorVariantIndex => colorVariantIndex;

    /// <summary>Shared base name grouping color variants of this item shape.</summary>
    public string GroupName => groupName;

    /// <summary>Base style score contributed by this item to an outfit.</summary>
    public int StyleScore => styleScore;

    // ── Public methods ─────────────────────────────────────────────────────

    /// <summary>
    /// Converts the stored canvas pixel position to a Unity world-space position.
    /// The canvas is 2048×2048. World origin is the canvas centre.
    /// spritePixelWidth/Height are the actual pixel dimensions of the sprite.
    /// </summary>
    /// <param name="pixelsPerUnit">The Pixels Per Unit value set on the sprite import settings.</param>
    /// <param name="canvasWidth">Total width of the source canvas in pixels (typically 2048).</param>
    /// <param name="canvasHeight">Total height of the source canvas in pixels (typically 2048).</param>
    /// <param name="spritePixelWidth">Pixel width of this sprite's texture rect.</param>
    /// <param name="spritePixelHeight">Pixel height of this sprite's texture rect.</param>
    /// <returns>World-space position for the sprite's centre pivot.</returns>
    public Vector2 GetWorldPosition(float pixelsPerUnit, int canvasWidth, int canvasHeight,
                                     int spritePixelWidth, int spritePixelHeight)
    {
        float pivotX = (canvasX + spritePixelWidth * 0.5f) / pixelsPerUnit
                       - (canvasWidth / pixelsPerUnit) * 0.5f;

        float pivotY = -((canvasY + spritePixelHeight * 0.5f) / pixelsPerUnit)
                       + (canvasHeight / pixelsPerUnit) * 0.5f;

        return new Vector2(pivotX, pivotY);
    }

    /// <summary>Returns true if this item has the given theme tag (case-insensitive).</summary>
    /// <param name="tag">The tag to search for.</param>
    /// <returns>True if the tag is present in this item's theme tags.</returns>
    public bool HasThemeTag(string tag)
    {
        if (themeTags == null || tag == null) return false;
        foreach (string t in themeTags)
        {
            if (string.Equals(t, tag, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    /// <summary>Returns a copy of the theme tags array.</summary>
    /// <returns>
    /// A new array containing all theme tags, or an empty array if none are defined.
    /// </returns>
    public string[] GetThemeTags()
    {
        if (themeTags == null) return new string[0];
        string[] copy = new string[themeTags.Length];
        System.Array.Copy(themeTags, copy, themeTags.Length);
        return copy;
    }
}
