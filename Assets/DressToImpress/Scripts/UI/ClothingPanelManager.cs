using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the bottom clothing panel in the Styling Room. Displays category
/// tabs across the top (HATS, TOPS, etc.) and a scrollable grid of item
/// buttons below. Clicking a tab filters items by that category. Clicking an
/// item button calls <see cref="CharacterDisplay.ToggleItem"/> and updates
/// the button's visual highlight state.
/// </summary>
public class ClothingPanelManager : MonoBehaviour
{
    // ── Serialized fields ──────────────────────────────────────────────────

    [Header("References")]
    [SerializeField] private CharacterDisplay characterDisplay;

    [Header("Prefabs")]
    [Tooltip("Prefab for a category tab button. Must have a Button component and a TMP_Text child named 'Label'.")]
    [SerializeField] private GameObject tabButtonPrefab;

    [Tooltip("Prefab for a clothing item toggle button. Must have a Button component, an Image component for the icon, and a TMP_Text child named 'Label'.")]
    [SerializeField] private GameObject itemButtonPrefab;

    [Header("Containers")]
    [Tooltip("Parent transform where tab buttons are created.")]
    [SerializeField] private Transform tabContainer;

    [Tooltip("Content transform inside the item ScrollRect.")]
    [SerializeField] private Transform itemContainer;

    [Header("Categories")]
    [Tooltip("Which categories get tabs in this panel. Order determines tab order.")]
    [SerializeField] private ClothingCategory[] visibleCategories = new ClothingCategory[]
    {
        ClothingCategory.Hat, ClothingCategory.Top, ClothingCategory.Bottom,
        ClothingCategory.Skirt, ClothingCategory.Dress, ClothingCategory.Shoes,
        ClothingCategory.SocksLeggings, ClothingCategory.Outerwear, ClothingCategory.Accessory
    };

    [Header("Item Source")]
    [Tooltip("Central database of all ClothingItemData assets. If left empty, falls back to Resources.Load<ClothingDatabase>(\"ClothingDatabase\"). Populated by Dress To Impress > Import Clothing Assets.")]
    [SerializeField] private ClothingDatabase database;

    [Tooltip("Optional override: if set, only these items are shown (rather than the full database). Useful for boutique-style scenes that show a curated subset.")]
    [SerializeField] private ClothingItemData[] allClothingItems;

    [Header("Visual")]
    [Tooltip("Label color applied to the active tab.")]
    [SerializeField] private Color activeTabColor = new Color(0.15f, 0.1f, 0.15f);

    [Tooltip("Label color applied to inactive tabs.")]
    [SerializeField] private Color inactiveTabColor = new Color(0.95f, 0.95f, 0.95f);

    [Tooltip("Background color for the active tab.")]
    [SerializeField] private Color activeTabBackground = new Color(1f, 0.85f, 0.4f);

    [Tooltip("Background color for inactive tabs.")]
    [SerializeField] private Color inactiveTabBackground = new Color(0.18f, 0.18f, 0.22f);

    [Tooltip("Color overlay applied to an item button when its item is equipped.")]
    [SerializeField] private Color selectedItemColor = new Color(0.7f, 1f, 0.7f);

    [Tooltip("Default item button color.")]
    [SerializeField] private Color defaultItemColor = Color.white;

    [Header("Events")]
    /// <summary>Fires when the player switches to a different category tab.</summary>
    public UnityEvent<ClothingCategory> onCategoryChanged;

    /// <summary>Fires when the player clicks an item button. Passes the ClothingItemData.</summary>
    public UnityEvent<ClothingItemData> onItemSelected;

    // ── Private runtime state ──────────────────────────────────────────────

    private ClothingCategory _activeCategory;
    private Dictionary<ClothingCategory, List<ClothingItemData>> _itemsByCategory;
    private List<GameObject> _activeTabButtons = new();
    private List<GameObject> _activeItemButtons = new();

    /// <summary>
    /// Parallel list to <see cref="_activeItemButtons"/> — each index holds the
    /// <see cref="ClothingItemData"/> corresponding to the button at the same index.
    /// Used by <see cref="RefreshButtonStates"/> to avoid fragile index look-ups.
    /// </summary>
    private List<ClothingItemData> _activeItemData = new();

    // ── Unity lifecycle ────────────────────────────────────────────────────

    private void Start()
    {
        Initialize();
    }

    // ── Public API ─────────────────────────────────────────────────────────

    /// <summary>
    /// Builds the category tab buttons and populates the item dictionary.
    /// Called automatically on Start. Safe to call again to rebuild after
    /// <see cref="SetAvailableItems"/>.
    /// </summary>
    public void Initialize()
    {
        if (!ValidateReferences()) return;

        ClearTabButtons();
        ClearItemButtons();

        // Build category → items lookup. When allClothingItems is non-empty
        // it acts as a curated override. Otherwise pull every item in the
        // visible categories from the central ClothingDatabase.
        _itemsByCategory = new Dictionary<ClothingCategory, List<ClothingItemData>>();

        if (allClothingItems != null && allClothingItems.Length > 0)
        {
            foreach (ClothingItemData item in allClothingItems)
            {
                if (item == null) continue;

                bool isVisible = false;
                foreach (ClothingCategory vc in visibleCategories)
                {
                    if (item.Category == vc) { isVisible = true; break; }
                }
                if (!isVisible) continue;

                if (!_itemsByCategory.ContainsKey(item.Category))
                    _itemsByCategory[item.Category] = new List<ClothingItemData>();

                _itemsByCategory[item.Category].Add(item);
            }
        }
        else
        {
            ClothingDatabase db = database != null ? database : ClothingDatabase.Default;
            if (db == null)
            {
                Debug.LogWarning("[ClothingPanelManager] No ClothingDatabase assigned and none found at Resources/ClothingDatabase. Run 'Dress To Impress > Import Clothing Assets' to create it.");
            }
            else
            {
                foreach (ClothingCategory vc in visibleCategories)
                {
                    ClothingItemData[] items = db.GetByCategory(vc);
                    if (items.Length == 0) continue;

                    var list = new List<ClothingItemData>(items.Length);
                    foreach (ClothingItemData item in items)
                        if (item != null) list.Add(item);

                    if (list.Count > 0)
                        _itemsByCategory[vc] = list;
                }
            }
        }

        // Create tab buttons
        for (int i = 0; i < visibleCategories.Length; i++)
        {
            ClothingCategory cat = visibleCategories[i]; // captured per iteration

            GameObject tabGO = Instantiate(tabButtonPrefab, tabContainer);
            tabGO.name = "Tab_" + cat.ToString();

            TMP_Text label = FindChildTMPText(tabGO, "Label");
            if (label != null)
                label.text = cat.ToString();
            else
                Debug.LogWarning($"[ClothingPanelManager] Tab prefab has no TMP_Text child named 'Label'. Tab for '{cat}' will have no text.");

            Button btn = tabGO.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => ShowCategory(cat));
            else
                Debug.LogWarning($"[ClothingPanelManager] Tab prefab has no Button component. Tab for '{cat}' will not be clickable.");

            _activeTabButtons.Add(tabGO);
        }

        // Show first category by default
        if (visibleCategories.Length > 0)
            ShowCategory(visibleCategories[0]);
    }

    /// <summary>
    /// Switches the panel to show items for the given category.
    /// Fires <see cref="onCategoryChanged"/>.
    /// </summary>
    /// <param name="category">The category whose items should be displayed.</param>
    public void ShowCategory(ClothingCategory category)
    {
        _activeCategory = category;

        // Update tab highlight colors (label + background) for clear contrast
        // regardless of how the surrounding panel has been re-themed.
        for (int i = 0; i < _activeTabButtons.Count; i++)
        {
            if (_activeTabButtons[i] == null) continue;

            bool isActive = (visibleCategories[i] == category);

            TMP_Text label = _activeTabButtons[i].GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.color = isActive ? activeTabColor : inactiveTabColor;

            Image bg = _activeTabButtons[i].GetComponent<Image>();
            if (bg != null)
                bg.color = isActive ? activeTabBackground : inactiveTabBackground;
        }

        // Rebuild item buttons for this category
        ClearItemButtons();

        if (_itemsByCategory != null && _itemsByCategory.TryGetValue(category, out List<ClothingItemData> items))
        {
            for (int i = 0; i < items.Count; i++)
            {
                ClothingItemData item = items[i]; // captured per iteration

                GameObject btnGO = Instantiate(itemButtonPrefab, itemContainer);
                btnGO.name = "Item_" + item.ItemName;

                // Set label text
                TMP_Text label = FindChildTMPText(btnGO, "Label");
                if (label != null)
                    label.text = item.ItemName;

                // Set icon sprite on a child Image (not the root button Image)
                if (item.Sprite != null)
                {
                    Image iconImage = FindChildImage(btnGO);
                    if (iconImage != null)
                    {
                        iconImage.sprite = item.Sprite;
                        iconImage.preserveAspect = true;
                    }
                }

                // Wire click listener
                Button btn = btnGO.GetComponent<Button>();
                if (btn != null)
                    btn.onClick.AddListener(() => OnItemButtonClicked(item));
                else
                    Debug.LogWarning($"[ClothingPanelManager] Item button prefab has no Button component. Item '{item.ItemName}' will not be clickable.");

                _activeItemButtons.Add(btnGO);
                _activeItemData.Add(item);
            }
        }

        RefreshButtonStates();
        onCategoryChanged?.Invoke(category);
    }

    /// <summary>
    /// Called when a clothing item button is clicked. Toggles the item on the
    /// character and refreshes button highlight states.
    /// </summary>
    /// <param name="item">The item whose button was clicked.</param>
    public void OnItemButtonClicked(ClothingItemData item)
    {
        if (characterDisplay == null)
        {
            Debug.LogWarning("[ClothingPanelManager] OnItemButtonClicked: characterDisplay is null.");
            return;
        }

        characterDisplay.ToggleItem(item);
        RefreshButtonStates();
        onItemSelected?.Invoke(item);
    }

    /// <summary>
    /// Updates button highlight colors to match the current equipped state of
    /// the <see cref="CharacterDisplay"/>. Uses the parallel
    /// <c>_activeItemData</c> list to map each button to its item safely.
    /// </summary>
    public void RefreshButtonStates()
    {
        if (characterDisplay == null) return;

        for (int i = 0; i < _activeItemButtons.Count; i++)
        {
            if (_activeItemButtons[i] == null) continue;
            if (i >= _activeItemData.Count) break;

            ClothingItemData item = _activeItemData[i];
            bool equipped = (item != null) && characterDisplay.IsEquipped(item);

            Image rootImage = _activeItemButtons[i].GetComponent<Image>();
            if (rootImage != null)
                rootImage.color = equipped ? selectedItemColor : defaultItemColor;
        }
    }

    /// <summary>
    /// Replaces the available item list and rebuilds the panel. Call this from
    /// a BoutiqueManager when new items are unlocked.
    /// </summary>
    /// <param name="items">The full new set of available clothing items.</param>
    public void SetAvailableItems(ClothingItemData[] items)
    {
        allClothingItems = items;
        Initialize();
    }

    /// <summary>
    /// Enables or disables all item and tab button interactions. Use this to
    /// lock the panel during outfit submission or other locked states.
    /// </summary>
    /// <param name="interactable">True to re-enable, false to disable.</param>
    public void SetInteractable(bool interactable)
    {
        foreach (GameObject btnGO in _activeItemButtons)
        {
            if (btnGO == null) continue;
            Button btn = btnGO.GetComponent<Button>();
            if (btn != null) btn.interactable = interactable;
        }

        foreach (GameObject tabGO in _activeTabButtons)
        {
            if (tabGO == null) continue;
            Button btn = tabGO.GetComponent<Button>();
            if (btn != null) btn.interactable = interactable;
        }
    }

    // ── Private helpers ────────────────────────────────────────────────────

    /// <summary>
    /// Validates that all required references are assigned. Logs warnings for
    /// anything missing and returns false if the critical setup cannot proceed.
    /// </summary>
    private bool ValidateReferences()
    {
        bool ok = true;

        if (characterDisplay == null)
        {
            Debug.LogWarning("[ClothingPanelManager] characterDisplay is not assigned. Item equipping will not work.");
            // Non-fatal: panel can still build and display buttons
        }

        if (tabButtonPrefab == null)
        {
            Debug.LogWarning("[ClothingPanelManager] tabButtonPrefab is not assigned. Cannot build tabs.");
            ok = false;
        }

        if (itemButtonPrefab == null)
        {
            Debug.LogWarning("[ClothingPanelManager] itemButtonPrefab is not assigned. Cannot build item buttons.");
            ok = false;
        }

        if (tabContainer == null)
        {
            Debug.LogWarning("[ClothingPanelManager] tabContainer is not assigned. Cannot parent tab buttons.");
            ok = false;
        }

        if (itemContainer == null)
        {
            Debug.LogWarning("[ClothingPanelManager] itemContainer is not assigned. Cannot parent item buttons.");
            ok = false;
        }

        return ok;
    }

    /// <summary>
    /// Destroys all instantiated tab buttons and clears the tracking list.
    /// </summary>
    private void ClearTabButtons()
    {
        foreach (GameObject go in _activeTabButtons)
        {
            if (go != null) Destroy(go);
        }
        _activeTabButtons.Clear();
    }

    /// <summary>
    /// Destroys all instantiated item buttons and clears both tracking lists.
    /// </summary>
    private void ClearItemButtons()
    {
        foreach (GameObject go in _activeItemButtons)
        {
            if (go != null) Destroy(go);
        }
        _activeItemButtons.Clear();
        _activeItemData.Clear();
    }

    /// <summary>
    /// Finds a <see cref="TMP_Text"/> child of <paramref name="root"/> whose
    /// GameObject is named <paramref name="childName"/>. Returns null if not found.
    /// </summary>
    private TMP_Text FindChildTMPText(GameObject root, string childName)
    {
        TMP_Text[] candidates = root.GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text t in candidates)
        {
            if (t.gameObject.name == childName)
                return t;
        }
        return null;
    }

    /// <summary>
    /// Returns the first <see cref="Image"/> component found on a child of
    /// <paramref name="root"/> (i.e. not on the root itself). Used to locate
    /// the icon image inside an item button prefab without interfering with the
    /// button's own background Image.
    /// </summary>
    private Image FindChildImage(GameObject root)
    {
        Image[] candidates = root.GetComponentsInChildren<Image>(true);
        foreach (Image img in candidates)
        {
            if (img.gameObject != root)
                return img;
        }
        return null;
    }
}
