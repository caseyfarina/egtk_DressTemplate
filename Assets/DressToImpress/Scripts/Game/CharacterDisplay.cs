using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Core rendering system for the 2D dress-up character. Manages one child
/// SpriteRenderer per <see cref="ClothingCategory"/>, all parented under this
/// GameObject (or an optional <see cref="layerRoot"/>). Equipping a
/// <see cref="ClothingItemData"/> sets the correct sprite on the right renderer
/// and positions it using the canvas pixel coordinates embedded in the asset.
/// </summary>
public class CharacterDisplay : MonoBehaviour
{
    // ── Serialized fields ──────────────────────────────────────────────────

    [Header("Canvas Settings")]
    [SerializeField] private float pixelsPerUnit = 100f;
    [SerializeField] private int canvasWidth = 2048;
    [SerializeField] private int canvasHeight = 2048;

    [Header("Layer Root")]
    [Tooltip("Parent transform for all generated sprite renderers. Leave empty to use this transform.")]
    [SerializeField] private Transform layerRoot;

    [Header("Character Creation Data")]
    [Tooltip("All body type options (BodyBase category)")]
    [SerializeField] private ClothingItemData[] bodyTypes;
    [SerializeField] private ClothingItemData[] frontHairs;
    [SerializeField] private ClothingItemData[] backHairs;
    [SerializeField] private ClothingItemData[] eyes;
    [SerializeField] private ClothingItemData[] eyebrows;
    [SerializeField] private ClothingItemData[] mouths;
    [SerializeField] private ClothingItemData[] ears;
    [SerializeField] private ClothingItemData[] noses;

    [Header("Events")]
    /// <summary>Fires when any item is equipped. Passes the equipped ClothingItemData.</summary>
    public UnityEvent<ClothingItemData> onItemEquipped;

    /// <summary>Fires when a category is cleared. Passes the category that was unequipped.</summary>
    public UnityEvent<ClothingCategory> onItemUnequipped;

    // ── Sorting order constants ────────────────────────────────────────────

    private static readonly Dictionary<ClothingCategory, int> SortingOrders
        = new Dictionary<ClothingCategory, int>
    {
        { ClothingCategory.BackHair,      0  },
        { ClothingCategory.BodyBase,      10 },
        { ClothingCategory.SocksLeggings, 15 },
        { ClothingCategory.Bottom,        20 },
        { ClothingCategory.Skirt,         20 },
        { ClothingCategory.Shoes,         25 },
        //{ ClothingCategory.Inner,         35 },
        { ClothingCategory.Top,           30 },
        { ClothingCategory.Dress,         30 },
        { ClothingCategory.Outerwear,     35 },
        { ClothingCategory.Ears,          40 },
        { ClothingCategory.Eyes,          42 },
        { ClothingCategory.Eyebrows,      44 },
        { ClothingCategory.Mouth,         46 },
        { ClothingCategory.Nose,          48 },
        { ClothingCategory.FrontHair,     50 },
        { ClothingCategory.Hat,           55 },
        { ClothingCategory.Accessory,     60 },
    };

    // ── Private runtime state ──────────────────────────────────────────────

    private Dictionary<ClothingCategory, SpriteRenderer> _layerRenderers;
    private Dictionary<ClothingCategory, ClothingItemData> _equippedItems;

    // ── Unity lifecycle ────────────────────────────────────────────────────

    private void Awake()
    {
        InitializeLayers();
    }

    private void Start()
    {
        ApplyProfile(CharacterProfile.Instance);
    }

    // ── Initialisation ─────────────────────────────────────────────────────

    private void InitializeLayers()
    {
        _layerRenderers = new Dictionary<ClothingCategory, SpriteRenderer>();
        _equippedItems  = new Dictionary<ClothingCategory, ClothingItemData>();

        Transform root = layerRoot != null ? layerRoot : transform;

        foreach (ClothingCategory category in System.Enum.GetValues(typeof(ClothingCategory)))
        {
            GameObject layerGO = new GameObject("Layer_" + category.ToString());
            layerGO.transform.SetParent(root, false);

            SpriteRenderer sr = layerGO.AddComponent<SpriteRenderer>();

            if (SortingOrders.TryGetValue(category, out int order))
                sr.sortingOrder = order;
            else
                sr.sortingOrder = 0;

            _layerRenderers[category] = sr;
        }
    }

    // ── Canvas-to-world helper ─────────────────────────────────────────────

    private Vector2 CanvasToWorld(int canvasX, int canvasY, int spritePixelW, int spritePixelH)
    {
        float ppu    = pixelsPerUnit;
        float pivotX = (canvasX + spritePixelW * 0.5f) / ppu - (canvasWidth  / ppu) * 0.5f;
        float pivotY = -((canvasY + spritePixelH * 0.5f) / ppu) + (canvasHeight / ppu) * 0.5f;
        return new Vector2(pivotX, pivotY);
    }

    // ── Private helpers ────────────────────────────────────────────────────

    private bool CheckInitialized(string callerName)
    {
        if (_layerRenderers == null)
        {
            Debug.LogWarning($"[CharacterDisplay] {callerName} called before Awake — layers not yet initialized.");
            return false;
        }
        return true;
    }

    /// <summary>
    /// Equips an item onto the character layer, positioning the sprite using the
    /// canvas pixel coordinates stored in the asset. Returns without action if the
    /// renderer for the item's category does not exist.
    /// </summary>
    private void EquipItemInternal(ClothingItemData item)
    {
        if (item == null) return;

        if (!_layerRenderers.TryGetValue(item.Category, out SpriteRenderer sr))
        {
            Debug.LogWarning($"[CharacterDisplay] No renderer found for category {item.Category}.");
            return;
        }

        sr.sprite = item.Sprite;

        if (item.Sprite != null)
        {
            int w = item.Sprite.texture.width;
            int h = item.Sprite.texture.height;
            Vector2 worldPos = item.GetWorldPosition(pixelsPerUnit, canvasWidth, canvasHeight, w, h);
            sr.transform.localPosition = new Vector3(worldPos.x, worldPos.y, 0f);
        }
        else
        {
            Debug.LogWarning($"[CharacterDisplay] Item '{item.ItemName}' has no sprite — position defaulting to zero.");
            sr.transform.localPosition = Vector3.zero;
        }

        _equippedItems[item.Category] = item;
    }

    private void EquipFromArray(ClothingItemData[] array, int index)
    {
        if (array == null || array.Length == 0) return;
        int clamped = Mathf.Clamp(index, 0, array.Length - 1);
        ClothingItemData item = array[clamped];
        if (item == null) return;
        EquipItemInternal(item);
        onItemEquipped?.Invoke(item);
    }

    // ── Public API — core equip/unequip ───────────────────────────────────

    /// <summary>
    /// Equips an item, placing its sprite on the correct layer at the correct
    /// canvas-derived position. Handles Dress/Top/Bottom/Skirt exclusivity
    /// automatically. Does nothing if <paramref name="item"/> is null.
    /// </summary>
    /// <param name="item">The clothing item to equip.</param>
    public void EquipItem(ClothingItemData item)
    {
        if (item == null) return;
        if (!CheckInitialized(nameof(EquipItem))) return;

        // Dress/Top/Bottom/Skirt exclusivity
        if (item.Category == ClothingCategory.Dress)
        {
            SilentUnequip(ClothingCategory.Top);
            //SilentUnequip(ClothingCategory.Inner);
            SilentUnequip(ClothingCategory.Bottom);
            SilentUnequip(ClothingCategory.Skirt);
        }
        else if (item.Category == ClothingCategory.Top   ||
                 //item.Category == ClothingCategory.Inner  ||
                 item.Category == ClothingCategory.Bottom ||
                 item.Category == ClothingCategory.Skirt)
        {
            SilentUnequip(ClothingCategory.Dress);
        }

        EquipItemInternal(item);
        onItemEquipped?.Invoke(item);
    }

    /// <summary>
    /// Removes the item from the given category layer, showing nothing.
    /// Fires <see cref="onItemUnequipped"/>.
    /// </summary>
    /// <param name="category">The clothing category to clear.</param>
    public void UnequipCategory(ClothingCategory category)
    {
        if (!CheckInitialized(nameof(UnequipCategory))) return;

        if (_layerRenderers.TryGetValue(category, out SpriteRenderer sr))
            sr.sprite = null;

        _equippedItems.Remove(category);
        onItemUnequipped?.Invoke(category);
    }

    /// <summary>
    /// If this exact item is currently equipped, unequips it.
    /// Otherwise equips it, replacing any previous item in that category.
    /// </summary>
    /// <param name="item">The item to toggle.</param>
    public void ToggleItem(ClothingItemData item)
    {
        if (item == null) return;
        if (!CheckInitialized(nameof(ToggleItem))) return;

        if (_equippedItems.TryGetValue(item.Category, out ClothingItemData current) && current == item)
            UnequipCategory(item.Category);
        else
            EquipItem(item);
    }

    /// <summary>
    /// Unequips all clothing layers (Hat through Accessory). Leaves face features
    /// and hair layers unchanged.
    /// </summary>
    public void UnequipAllClothing()
    {
        if (!CheckInitialized(nameof(UnequipAllClothing))) return;

        UnequipCategory(ClothingCategory.Hat);
        UnequipCategory(ClothingCategory.Top);
        //UnequipCategory(ClothingCategory.Inner);
        UnequipCategory(ClothingCategory.Bottom);
        UnequipCategory(ClothingCategory.Skirt);
        UnequipCategory(ClothingCategory.Dress);
        UnequipCategory(ClothingCategory.Shoes);
        UnequipCategory(ClothingCategory.SocksLeggings);
        UnequipCategory(ClothingCategory.Outerwear);
        UnequipCategory(ClothingCategory.Accessory);
    }

    /// <summary>
    /// Returns the <see cref="ClothingItemData"/> currently equipped in the given
    /// category, or null if that layer is empty.
    /// </summary>
    /// <param name="category">The category to query.</param>
    public ClothingItemData GetEquippedItem(ClothingCategory category)
    {
        if (_equippedItems == null) return null;
        _equippedItems.TryGetValue(category, out ClothingItemData item);
        return item;
    }

    /// <summary>
    /// Returns all currently equipped clothing items (Hat through Accessory only;
    /// face features and hair are excluded).
    /// </summary>
    /// <returns>A new list containing only clothing-slot items.</returns>
    public List<ClothingItemData> GetAllEquippedClothing()
    {
        var result = new List<ClothingItemData>();
        if (_equippedItems == null) return result;

        ClothingCategory[] clothingCategories =
        {
            ClothingCategory.Hat,
            ClothingCategory.Top,
            //ClothingCategory.Inner,
            ClothingCategory.Bottom,
            ClothingCategory.Skirt,
            ClothingCategory.Dress,
            ClothingCategory.Shoes,
            ClothingCategory.SocksLeggings,
            ClothingCategory.Outerwear,
            ClothingCategory.Accessory,
        };

        foreach (ClothingCategory cat in clothingCategories)
        {
            if (_equippedItems.TryGetValue(cat, out ClothingItemData item) && item != null)
                result.Add(item);
        }

        return result;
    }

    /// <summary>Returns true if this exact item is currently equipped.</summary>
    /// <param name="item">The item to check.</param>
    public bool IsEquipped(ClothingItemData item)
    {
        if (item == null || _equippedItems == null) return false;
        return _equippedItems.TryGetValue(item.Category, out ClothingItemData current) && current == item;
    }

    // ── Public API — character creation ───────────────────────────────────

    /// <summary>
    /// Applies all selections from the given <see cref="CharacterProfile"/> to the
    /// character display using the serialized data arrays on this component.
    /// Index values are clamped to valid array bounds. Does nothing if
    /// <paramref name="profile"/> is null.
    /// </summary>
    /// <param name="profile">The profile whose index values should be applied.</param>
    public void ApplyProfile(CharacterProfile profile)
    {
        if (profile == null) return;
        if (!CheckInitialized(nameof(ApplyProfile))) return;

        EquipFromArray(bodyTypes,  profile.BodyTypeIndex);
        EquipFromArray(frontHairs, profile.FrontHairIndex);
        EquipFromArray(backHairs,  profile.BackHairIndex);
        EquipFromArray(eyes,       profile.EyesIndex);
        EquipFromArray(eyebrows,   profile.EyebrowsIndex);
        EquipFromArray(mouths,     profile.MouthIndex);
        EquipFromArray(ears,       profile.EarsIndex);
        EquipFromArray(noses,      profile.NoseIndex);
    }

    /// <summary>
    /// Sets the body base layer (body type and skin-tone composite sprite).
    /// </summary>
    /// <param name="bodyData">The body base item to equip.</param>
    public void SetBodyType(ClothingItemData bodyData)
    {
        if (!CheckInitialized(nameof(SetBodyType))) return;
        if (bodyData == null) return;
        EquipItemInternal(bodyData);
        onItemEquipped?.Invoke(bodyData);
    }

    /// <summary>
    /// Sets a facial feature layer (Eyes, Eyebrows, Mouth, Ears, or Nose).
    /// Logs a warning if <paramref name="featureCategory"/> is not a facial
    /// feature category.
    /// </summary>
    /// <param name="featureCategory">Must be one of: Eyes, Eyebrows, Mouth, Ears, Nose.</param>
    /// <param name="data">The item to place on that layer.</param>
    public void SetFacialFeature(ClothingCategory featureCategory, ClothingItemData data)
    {
        if (!CheckInitialized(nameof(SetFacialFeature))) return;
        if (data == null) return;

        if (featureCategory != ClothingCategory.Eyes      &&
            featureCategory != ClothingCategory.Eyebrows  &&
            featureCategory != ClothingCategory.Mouth     &&
            featureCategory != ClothingCategory.Ears      &&
            featureCategory != ClothingCategory.Nose)
        {
            Debug.LogWarning($"[CharacterDisplay] SetFacialFeature called with non-facial category '{featureCategory}'. Use EquipItem for clothing.");
            return;
        }

        EquipItemInternal(data);
        onItemEquipped?.Invoke(data);
    }

    /// <summary>
    /// Sets the front or back hair layer.
    /// Logs a warning if <paramref name="hairCategory"/> is not a hair category.
    /// </summary>
    /// <param name="hairCategory">Must be FrontHair or BackHair.</param>
    /// <param name="data">The hair item to equip.</param>
    public void SetHair(ClothingCategory hairCategory, ClothingItemData data)
    {
        if (!CheckInitialized(nameof(SetHair))) return;
        if (data == null) return;

        if (hairCategory != ClothingCategory.FrontHair && hairCategory != ClothingCategory.BackHair)
        {
            Debug.LogWarning($"[CharacterDisplay] SetHair called with non-hair category '{hairCategory}'. Use EquipItem for clothing.");
            return;
        }

        EquipItemInternal(data);
        onItemEquipped?.Invoke(data);
    }

    // ── Private helpers ────────────────────────────────────────────────────

    /// <summary>
    /// Clears a category layer without firing events. Used internally for
    /// Dress/Top/Bottom/Skirt exclusivity enforcement.
    /// </summary>
    private void SilentUnequip(ClothingCategory category)
    {
        if (_layerRenderers.TryGetValue(category, out SpriteRenderer sr))
            sr.sprite = null;
        _equippedItems.Remove(category);
    }
}
