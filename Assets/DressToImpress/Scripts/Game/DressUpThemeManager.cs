using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A single challenge theme — configure these in the DressUpThemeManager Inspector list.
/// </summary>
[System.Serializable]
public class ThemeData
{
    [Tooltip("Short name for this theme (e.g. 'Beach Party')")]
    public string themeName = "Beach Party";

    [Tooltip("Prompt text shown to the player (e.g. 'Dress for a day at the beach!')")]
    [TextArea(2, 4)]
    public string promptText = "Dress for a day at the beach!";

    [Tooltip("Optional inspiration image displayed alongside the prompt")]
    public Sprite inspirationImage;

    [Tooltip("Tags that clothing items must match for bonus scoring (e.g. 'beach', 'casual', 'summer'). " +
             "These must match the theme tags set on individual ClothingItem components.")]
    public string[] themeTags = new string[0];

    /// <summary>Returns true if the given tag appears in this theme's tag list (case-insensitive)</summary>
    public bool HasTag(string tag)
    {
        if (string.IsNullOrEmpty(tag)) return false;
        foreach (string t in themeTags)
        {
            if (string.Equals(t, tag, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}

/// <summary>
/// Manages the list of challenge themes and picks one at the start of each round.
/// Wire onThemeSet to ActionDisplayText.DisplayText to show the prompt on screen.
/// Wire onInspirationImageSet to ActionDisplayImage to show the inspiration image.
/// Common use: Central game manager for the dress-up challenge loop.
/// </summary>
public class DressUpThemeManager : MonoBehaviour
{
    [Header("Themes")]
    [Tooltip("All possible challenge themes. One is picked at random each round.")]
    [SerializeField] private ThemeData[] themes = new ThemeData[]
    {
        new ThemeData
        {
            themeName    = "Beach Party",
            promptText   = "Dress for a day at the beach!",
            themeTags    = new[] { "beach", "casual", "summer" }
        },
        new ThemeData
        {
            themeName    = "Black Tie Gala",
            promptText   = "Look your best at a formal gala!",
            themeTags    = new[] { "formal", "elegant", "black-tie" }
        },
        new ThemeData
        {
            themeName    = "Sporty Vibes",
            promptText   = "Show off your athletic style!",
            themeTags    = new[] { "sporty", "athletic", "casual" }
        }
    };

    [Header("Round Settings")]
    [Tooltip("Pick a theme automatically when the scene starts")]
    [SerializeField] private bool startRoundOnAwake = true;

    [Tooltip("Prevent the same theme appearing twice in a row")]
    [SerializeField] private bool avoidRepeat = true;

    [Header("Events")]
    /// <summary>
    /// Fires when a theme is selected, passing the prompt text.
    /// Wire to ActionDisplayText.DisplayText to show the challenge on screen.
    /// </summary>
    public UnityEvent<string> onThemeSet;

    /// <summary>
    /// Fires when a theme is selected, passing the theme name.
    /// Useful for logging or secondary displays.
    /// </summary>
    public UnityEvent<string> onThemeNameSet;

    /// <summary>
    /// Fires when a theme is selected, passing the inspiration image sprite.
    /// Wire to ActionDisplayImage (if available) to show a visual reference.
    /// </summary>
    public UnityEvent<Sprite> onInspirationImageSet;

    // ── Runtime state ──────────────────────────────────────────────────────────

    private ThemeData currentTheme;
    private int lastThemeIndex = -1;

    /// <summary>The currently active theme (null before StartRound is called)</summary>
    public ThemeData CurrentTheme => currentTheme;

    // ── Unity lifecycle ────────────────────────────────────────────────────────

    private void Start()
    {
        if (startRoundOnAwake)
            StartRound();
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Randomly selects a new theme and fires all theme events.
    /// Call this from a UnityEvent (e.g. GameTimerManager.onTimerEnd → StartRound) to begin a new round.
    /// </summary>
    public void StartRound()
    {
        if (themes == null || themes.Length == 0)
        {
            Debug.LogWarning("[DressUpThemeManager] No themes configured — add at least one ThemeData in the Inspector.", this);
            return;
        }

        int index = PickThemeIndex();
        lastThemeIndex = index;
        currentTheme = themes[index];

        onThemeNameSet.Invoke(currentTheme.themeName);
        onThemeSet.Invoke(currentTheme.promptText);

        if (currentTheme.inspirationImage != null)
            onInspirationImageSet.Invoke(currentTheme.inspirationImage);
    }

    /// <summary>
    /// Returns the name of the currently active theme, or an empty string if none is set.
    /// </summary>
    public string GetCurrentThemeName()
    {
        return currentTheme != null ? currentTheme.themeName : string.Empty;
    }

    /// <summary>
    /// Returns true if the given tag matches any tag in the current theme.
    /// Called by OutfitScorer — students do not need to call this directly.
    /// </summary>
    public bool CurrentThemeHasTag(string tag)
    {
        return currentTheme != null && currentTheme.HasTag(tag);
    }

    // ── Internal ───────────────────────────────────────────────────────────────

    private int PickThemeIndex()
    {
        if (themes.Length == 1) return 0;

        if (!avoidRepeat || lastThemeIndex < 0)
            return Random.Range(0, themes.Length);

        // Pick any index except the last one used
        int index = Random.Range(0, themes.Length - 1);
        if (index >= lastThemeIndex)
            index++;

        return index;
    }
}
