using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Evaluates the current outfit against the active judge's theme and produces a style score (0–100).
/// Wire CalculateScore to GameTimerManager.onTimerEnd so scoring happens automatically.
/// Wire onScoreCalculated to GameCollectionManager.SetValue to display the score.
/// Common use: End-of-round scoring, outfit rating, theme matching feedback.
/// </summary>
public class OutfitScorer : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The CharacterDisplay that tracks what clothing items are currently equipped.")]
    [SerializeField] private CharacterDisplay characterDisplay;

    [Tooltip("The JudgeManager that provides the active theme for scoring.")]
    [SerializeField] private JudgeManager judgeManager;

    [Header("Scoring Rules")]
    [Tooltip("Points awarded per equipped item regardless of theme match")]
    [SerializeField] private int basePointsPerItem = 10;

    [Tooltip("Bonus points for each item whose theme tags match the current judge's theme")]
    [SerializeField] private int themeBonusPerMatch = 15;

    [Tooltip("Bonus points if Hat, Top, Bottom, and Shoes are all equipped (complete outfit bonus)")]
    [SerializeField] private int completeOutfitBonus = 10;

    [Header("Score Range")]
    [Tooltip("Minimum possible score (even with an empty or mismatched outfit)")]
    [SerializeField] private int minScore = 0;

    [Tooltip("Maximum possible score — final result is clamped to this value")]
    [SerializeField] private int maxScore = 100;

    [Header("Events")]
    /// <summary>
    /// Fires when scoring is complete, passing the final score (0–100).
    /// Wire to GameCollectionManager.SetValue to display the score.
    /// </summary>
    public UnityEvent<int> onScoreCalculated;

    /// <summary>
    /// Fires when the score is Excellent (75–100 by default).
    /// </summary>
    public UnityEvent onRatingExcellent;

    /// <summary>
    /// Fires when the score is Good (50–74 by default).
    /// </summary>
    public UnityEvent onRatingGood;

    /// <summary>
    /// Fires when the score is Poor (below 50 by default).
    /// </summary>
    public UnityEvent onRatingPoor;

    [Header("Rating Thresholds")]
    [Tooltip("Score at or above this value triggers onRatingExcellent")]
    [SerializeField] private int excellentThreshold = 75;

    [Tooltip("Score at or above this value (but below Excellent) triggers onRatingGood")]
    [SerializeField] private int goodThreshold = 50;

    // ── Cached last score ──────────────────────────────────────────────────────

    private int lastScore = 0;

    /// <summary>The score from the most recent CalculateScore call.</summary>
    public int LastScore => lastScore;

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Calculates the outfit score and fires onScoreCalculated with the result.
    /// Wire this to GameTimerManager.onTimerEnd so it triggers at end of round.
    /// </summary>
    public void CalculateScore()
    {
        if (characterDisplay == null)
        {
            Debug.LogWarning("[OutfitScorer] No CharacterDisplay assigned — assign it in the Inspector.", this);
            FireScore(minScore);
            return;
        }

        List<ClothingItemData> equipped = characterDisplay.GetAllEquippedClothing();

        int score = 0;

        foreach (ClothingItemData item in equipped)
        {
            if (item == null) continue;

            score += basePointsPerItem;

            if (judgeManager != null)
            {
                foreach (string tag in item.GetThemeTags())
                {
                    if (judgeManager.CurrentThemeHasTag(tag))
                    {
                        score += themeBonusPerMatch;
                        break; // only count each item once even if it has multiple matching tags
                    }
                }
            }
        }

        if (IsOutfitComplete())
            score += completeOutfitBonus;

        FireScore(score);
    }

    /// <summary>
    /// Returns the score that would result from the current outfit without firing any events.
    /// Useful for live preview displays.
    /// </summary>
    public int PreviewScore()
    {
        if (characterDisplay == null) return minScore;

        List<ClothingItemData> equipped = characterDisplay.GetAllEquippedClothing();

        int score = 0;

        foreach (ClothingItemData item in equipped)
        {
            if (item == null) continue;

            score += basePointsPerItem;

            if (judgeManager != null)
            {
                foreach (string tag in item.GetThemeTags())
                {
                    if (judgeManager.CurrentThemeHasTag(tag))
                    {
                        score += themeBonusPerMatch;
                        break;
                    }
                }
            }
        }

        if (IsOutfitComplete())
            score += completeOutfitBonus;

        return Mathf.Clamp(score, minScore, maxScore);
    }

    // ── Internal ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns true if Hat, Top, Bottom, and Shoes are all currently equipped.
    /// Used to determine whether the complete outfit bonus should be awarded.
    /// </summary>
    private bool IsOutfitComplete()
    {
        if (characterDisplay == null) return false;

        return characterDisplay.GetEquippedItem(ClothingCategory.Hat)    != null &&
               characterDisplay.GetEquippedItem(ClothingCategory.Top)    != null &&
               characterDisplay.GetEquippedItem(ClothingCategory.Bottom) != null &&
               characterDisplay.GetEquippedItem(ClothingCategory.Shoes)  != null;
    }

    private void FireScore(int rawScore)
    {
        lastScore = Mathf.Clamp(rawScore, minScore, maxScore);
        onScoreCalculated.Invoke(lastScore);

        if (lastScore >= excellentThreshold)
            onRatingExcellent.Invoke();
        else if (lastScore >= goodThreshold)
            onRatingGood.Invoke();
        else
            onRatingPoor.Invoke();
    }
}
