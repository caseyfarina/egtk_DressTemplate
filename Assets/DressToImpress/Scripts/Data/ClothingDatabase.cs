using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central registry of every <see cref="ClothingItemData"/> asset in the
/// project. Populated by <c>ClothingImporter</c> and consumed by
/// <c>CharacterDisplay</c>, <c>CharacterCreator</c>, and
/// <c>ClothingPanelManager</c>. Replaces the per-component, per-category
/// arrays that previously had to be drag-and-dropped after every import.
///
/// <para>
/// Place this asset at <c>Assets/DressToImpress/Resources/ClothingDatabase.asset</c>
/// so components can find it via <see cref="Default"/> without manual wiring.
/// </para>
/// </summary>
[CreateAssetMenu(fileName = "ClothingDatabase", menuName = "Dress To Impress/Clothing Database")]
public class ClothingDatabase : ScriptableObject
{
    /// <summary>
    /// Resources path used by <see cref="Default"/>. Must match the actual
    /// asset location (relative to a <c>Resources</c> folder, no extension).
    /// </summary>
    public const string ResourcesPath = "ClothingDatabase";

    [SerializeField] private List<ClothingItemData> allItems = new List<ClothingItemData>();

    private Dictionary<ClothingCategory, ClothingItemData[]> _byCategory;

    /// <summary>Every item in the database, in insertion order.</summary>
    public IReadOnlyList<ClothingItemData> All => allItems;

    /// <summary>
    /// Returns all items belonging to <paramref name="category"/>, sorted by
    /// <see cref="ClothingItemData.GroupName"/> then
    /// <see cref="ClothingItemData.ColorVariantIndex"/> for stable ordering.
    /// Returns an empty array if no items exist for that category.
    /// </summary>
    public ClothingItemData[] GetByCategory(ClothingCategory category)
    {
        EnsureIndex();
        return _byCategory.TryGetValue(category, out ClothingItemData[] arr)
            ? arr
            : System.Array.Empty<ClothingItemData>();
    }

    /// <summary>
    /// Forces the category index to be rebuilt next time it is queried. Call
    /// this after editing <see cref="allItems"/> from editor code.
    /// </summary>
    public void InvalidateIndex()
    {
        _byCategory = null;
    }

    private void OnEnable()
    {
        // Force a rebuild on domain reload / first access.
        _byCategory = null;
    }

    private void EnsureIndex()
    {
        if (_byCategory != null) return;

        var buckets = new Dictionary<ClothingCategory, List<ClothingItemData>>();
        if (allItems != null)
        {
            foreach (ClothingItemData item in allItems)
            {
                if (item == null) continue;
                if (!buckets.TryGetValue(item.Category, out List<ClothingItemData> list))
                {
                    list = new List<ClothingItemData>();
                    buckets[item.Category] = list;
                }
                list.Add(item);
            }
        }

        _byCategory = new Dictionary<ClothingCategory, ClothingItemData[]>();
        foreach (var kvp in buckets)
        {
            kvp.Value.Sort(CompareByGroupAndColor);
            _byCategory[kvp.Key] = kvp.Value.ToArray();
        }
    }

    private static int CompareByGroupAndColor(ClothingItemData a, ClothingItemData b)
    {
        int byGroup = string.Compare(a.GroupName, b.GroupName, System.StringComparison.OrdinalIgnoreCase);
        if (byGroup != 0) return byGroup;
        return a.ColorVariantIndex.CompareTo(b.ColorVariantIndex);
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editor-only: replaces the entire item list. Used by <c>ClothingImporter</c>
    /// to rebuild the database after every scan.
    /// </summary>
    public void EditorReplaceAll(IEnumerable<ClothingItemData> items)
    {
        allItems.Clear();
        if (items != null)
        {
            foreach (ClothingItemData item in items)
                if (item != null) allItems.Add(item);
        }
        InvalidateIndex();
    }
#endif

    // ── Default-instance access ─────────────────────────────────────────────

    private static ClothingDatabase _default;

    /// <summary>
    /// Lazily-loaded default database instance, fetched via
    /// <c>Resources.Load&lt;ClothingDatabase&gt;("ClothingDatabase")</c>. Returns
    /// null if no asset exists at <c>Assets/DressToImpress/Resources/ClothingDatabase.asset</c>.
    /// Components without an explicit database reference fall back to this.
    /// </summary>
    public static ClothingDatabase Default
    {
        get
        {
            if (_default == null)
                _default = Resources.Load<ClothingDatabase>(ResourcesPath);
            return _default;
        }
    }

    /// <summary>
    /// Editor + runtime helper: clears the cached <see cref="Default"/> reference
    /// so the next access reloads from disk. Call after re-creating the asset.
    /// </summary>
    public static void ClearDefaultCache()
    {
        _default = null;
    }
}
