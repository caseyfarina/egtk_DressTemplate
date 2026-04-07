using UnityEngine;

/// <summary>
/// ScriptableObject asset that defines a judge's personality, style preferences,
/// dialogue reactions, and money rewards for each <see cref="OutfitRating"/>.
/// Create via Assets > Create > Dress To Impress > Judge Data.
/// </summary>
[CreateAssetMenu(fileName = "NewJudge", menuName = "Dress To Impress/Judge Data")]
public class JudgeData : ScriptableObject
{
    // ── Identity ───────────────────────────────────────────────────────────

    /// <summary>The judge's display name shown in the scoring UI.</summary>
    [SerializeField] private string judgeName;

    /// <summary>
    /// A short style label that characterises this judge's aesthetic
    /// (e.g. "Bold", "Gothic", "Glam").
    /// </summary>
    [SerializeField] private string styleTag;

    /// <summary>Portrait sprite displayed alongside the judge's dialogue.</summary>
    [SerializeField] private Sprite avatarSprite;

    /// <summary>Accent colour used for UI elements associated with this judge.</summary>
    [SerializeField] private Color accentColor = Color.white;

    /// <summary>Flavour text describing what the judge is looking for today.</summary>
    [TextArea(2, 4)]
    [SerializeField] private string promptText;

    /// <summary>
    /// Theme tags the judge values in an outfit
    /// (e.g. "Gothic", "Lace", "Dark"). Matched case-insensitively against
    /// <see cref="ClothingItemData.HasThemeTag"/>.
    /// </summary>
    [SerializeField] private string[] themeTags;

    // ── Dialogue ───────────────────────────────────────────────────────────

    /// <summary>
    /// One-to-three reaction lines spoken when the outfit receives an
    /// <see cref="OutfitRating.Excellent"/> score.
    /// </summary>
    [TextArea(1, 3)]
    [SerializeField] private string[] dialogueExcellent;

    /// <summary>
    /// One-to-three reaction lines spoken when the outfit receives a
    /// <see cref="OutfitRating.Good"/> score.
    /// </summary>
    [TextArea(1, 3)]
    [SerializeField] private string[] dialogueGood;

    /// <summary>
    /// One-to-three reaction lines spoken when the outfit receives a
    /// <see cref="OutfitRating.Poor"/> score.
    /// </summary>
    [TextArea(1, 3)]
    [SerializeField] private string[] dialoguePoor;

    // ── Rewards ────────────────────────────────────────────────────────────

    /// <summary>Money awarded when the player earns an Excellent rating.</summary>
    [SerializeField] private int moneyRewardExcellent = 200;

    /// <summary>Money awarded when the player earns a Good rating.</summary>
    [SerializeField] private int moneyRewardGood = 100;

    /// <summary>Money awarded when the player earns a Poor rating.</summary>
    [SerializeField] private int moneyRewardPoor = 25;

    // ── Public read-only properties ────────────────────────────────────────

    /// <summary>The judge's display name.</summary>
    public string JudgeName => judgeName;

    /// <summary>Short label characterising this judge's aesthetic.</summary>
    public string StyleTag => styleTag;

    /// <summary>Portrait sprite displayed alongside dialogue.</summary>
    public Sprite AvatarSprite => avatarSprite;

    /// <summary>Accent colour used in UI elements for this judge.</summary>
    public Color AccentColor => accentColor;

    /// <summary>Flavour text describing what the judge wants to see today.</summary>
    public string PromptText => promptText;

    // ── Public methods ─────────────────────────────────────────────────────

    /// <summary>Returns the dialogue lines for the given outfit rating.</summary>
    /// <param name="rating">The rating the player's outfit received.</param>
    /// <returns>
    /// The configured dialogue array for that rating, or a single-element
    /// fallback array containing <c>"..."</c> if none are configured.
    /// Never null, never empty.
    /// </returns>
    public string[] GetDialogueForRating(OutfitRating rating)
    {
        string[] lines = rating switch
        {
            OutfitRating.Excellent => dialogueExcellent,
            OutfitRating.Good      => dialogueGood,
            OutfitRating.Poor      => dialoguePoor,
            _                      => null,
        };

        return (lines != null && lines.Length > 0) ? lines : new string[] { "..." };
    }

    /// <summary>Returns the money reward for the given outfit rating.</summary>
    /// <param name="rating">The rating the player's outfit received.</param>
    /// <returns>The configured reward amount for that rating.</returns>
    public int GetRewardForRating(OutfitRating rating)
    {
        return rating switch
        {
            OutfitRating.Excellent => moneyRewardExcellent,
            OutfitRating.Good      => moneyRewardGood,
            OutfitRating.Poor      => moneyRewardPoor,
            _                      => 0,
        };
    }

    /// <summary>Returns true if this judge's theme tags include the given tag (case-insensitive).</summary>
    /// <param name="tag">The tag to search for.</param>
    /// <returns>True if the tag is present in this judge's theme tags.</returns>
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
}
