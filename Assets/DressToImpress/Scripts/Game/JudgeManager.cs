using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages the sequence of judges across rounds. Picks the next judge, presents
/// their prompt at round start, and reacts with dialogue and money awards after
/// an outfit is scored. Also provides bridge methods so <c>OutfitScorer</c> can
/// query theme information without depending directly on <see cref="JudgeData"/>.
/// </summary>
public class JudgeManager : MonoBehaviour
{
    // ── Serialized fields ──────────────────────────────────────────────────

    [Header("Judges")]
    [Tooltip("All available judges. Assign JudgeData ScriptableObjects here.")]
    [SerializeField] private JudgeData[] judges;

    [Tooltip("Randomise the judge order at the start of each game.")]
    [SerializeField] private bool shuffleJudges = true;

    [Tooltip("Prevent the same judge appearing twice in a row.")]
    [SerializeField] private bool avoidRepeat = true;

    [Tooltip("How many rounds before the game ends. 0 = unlimited.")]
    [SerializeField] private int maxRounds = 0;

    [Header("Score Thresholds")]
    [Tooltip("Score at or above this value counts as Excellent.")]
    [SerializeField] private int excellentThreshold = 75;

    [Tooltip("Score at or above this value (but below Excellent) counts as Good.")]
    [SerializeField] private int goodThreshold = 50;

    // ── UnityEvents ────────────────────────────────────────────────────────

    [Header("Judge Presentation Events")]
    /// <summary>Fires when a new judge is presented at the start of a round.</summary>
    public UnityEvent onJudgeReady;

    /// <summary>Fires with the judge's display name. Wire to ActionDisplayText or a TMP field.</summary>
    public UnityEvent<string> onJudgeNameSet;

    /// <summary>Fires with the judge's style tag (e.g. "Bold"). Wire to a TMP label.</summary>
    public UnityEvent<string> onStyleTagSet;

    /// <summary>Fires with the judge's prompt text. Wire to ActionDisplayText.DisplayText.</summary>
    public UnityEvent<string> onPromptSet;

    /// <summary>Fires with the judge's avatar sprite. Wire to a UI Image component.</summary>
    public UnityEvent<Sprite> onAvatarSet;

    [Header("Reaction Events")]
    /// <summary>Fires once per dialogue line when the judge reacts to the submitted outfit.</summary>
    public UnityEvent<string> onJudgeDialogue;

    /// <summary>Fires with the money reward amount. Wire to GameCollectionManager.Increment.</summary>
    public UnityEvent<int> onMoneyAwarded;

    /// <summary>Fires with the outfit rating after scoring. Use to trigger rating-specific effects.</summary>
    public UnityEvent<OutfitRating> onRatingDetermined;

    [Header("Game Flow Events")]
    /// <summary>Fires when maxRounds is reached and all judges have been served.</summary>
    public UnityEvent onAllJudgesServed;

    // ── Private state ──────────────────────────────────────────────────────

    private JudgeData _currentJudge;
    private int _lastJudgeIndex = -1;
    private int _completedRounds = 0;
    private List<int> _judgeQueue;

    // ── Public properties ──────────────────────────────────────────────────

    /// <summary>Returns how many rounds have been completed so far.</summary>
    public int CompletedRounds => _completedRounds;

    // ── Round flow ─────────────────────────────────────────────────────────

    /// <summary>
    /// Picks and presents the next judge. Fires all onJudge* presentation events.
    /// Call this at the start of each round (e.g. from StylingRoomManager.Initialize
    /// and OnNextJudge).
    /// </summary>
    public void PresentNextJudge()
    {
        if (judges == null || judges.Length == 0)
        {
            Debug.LogWarning("[JudgeManager] No judges assigned. Please add JudgeData assets to the Judges array.", this);
            return;
        }

        int index = PickNextJudgeIndex();
        _currentJudge = judges[index];

        onJudgeReady.Invoke();
        onJudgeNameSet.Invoke(_currentJudge.JudgeName);
        onStyleTagSet.Invoke(_currentJudge.StyleTag);
        onPromptSet.Invoke(_currentJudge.PromptText);
        onAvatarSet.Invoke(_currentJudge.AvatarSprite);
    }

    /// <summary>
    /// Evaluates the score, determines the rating, fires dialogue and money events.
    /// Call this after OutfitScorer fires onScoreCalculated.
    /// </summary>
    /// <param name="score">The raw integer score produced by OutfitScorer.</param>
    public void ReactToScore(int score)
    {
        if (_currentJudge == null)
        {
            Debug.LogWarning("[JudgeManager] ReactToScore called but no judge has been presented yet.", this);
            return;
        }

        OutfitRating rating = GetRating(score);

        onRatingDetermined.Invoke(rating);
        onMoneyAwarded.Invoke(_currentJudge.GetRewardForRating(rating));

        string[] dialogueLines = _currentJudge.GetDialogueForRating(rating);
        foreach (string line in dialogueLines)
        {
            onJudgeDialogue.Invoke(line);
        }

        _completedRounds++;

        if (maxRounds > 0 && _completedRounds >= maxRounds)
        {
            onAllJudgesServed.Invoke();
        }
    }

    // ── Query methods ──────────────────────────────────────────────────────

    /// <summary>Returns the currently active judge, or null if no judge has been presented yet.</summary>
    public JudgeData GetCurrentJudge()
    {
        return _currentJudge;
    }

    /// <summary>
    /// Returns the current judge's style tag. Bridge method for OutfitScorer compatibility.
    /// Returns an empty string if no judge is active.
    /// </summary>
    public string GetCurrentThemeName()
    {
        return _currentJudge != null ? _currentJudge.StyleTag : string.Empty;
    }

    /// <summary>
    /// Returns true if the current judge's theme tags include the given tag.
    /// Bridge method so OutfitScorer can call this without knowing about JudgeData.
    /// Returns false if no judge is active or the tag is null.
    /// </summary>
    /// <param name="tag">The theme tag to check (matched case-insensitively).</param>
    public bool CurrentThemeHasTag(string tag)
    {
        if (_currentJudge == null) return false;
        return _currentJudge.HasThemeTag(tag);
    }

    /// <summary>
    /// Categorises a raw score into an <see cref="OutfitRating"/> using this manager's
    /// configured thresholds.
    /// </summary>
    /// <param name="score">The raw integer score to evaluate.</param>
    /// <returns>
    /// <see cref="OutfitRating.Excellent"/> if score >= excellentThreshold,
    /// <see cref="OutfitRating.Good"/> if score >= goodThreshold,
    /// otherwise <see cref="OutfitRating.Poor"/>.
    /// </returns>
    public OutfitRating GetRating(int score)
    {
        if (score >= excellentThreshold) return OutfitRating.Excellent;
        if (score >= goodThreshold)      return OutfitRating.Good;
        return OutfitRating.Poor;
    }

    // ── Queue system ───────────────────────────────────────────────────────

    /// <summary>
    /// Fills the judge queue with indices 0..judges.Length-1 and optionally
    /// Fisher-Yates shuffles them.
    /// </summary>
    private void RebuildQueue()
    {
        if (_judgeQueue == null)
            _judgeQueue = new List<int>(judges.Length);
        else
            _judgeQueue.Clear();

        for (int i = 0; i < judges.Length; i++)
            _judgeQueue.Add(i);

        if (shuffleJudges)
        {
            for (int i = _judgeQueue.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                int temp = _judgeQueue[i];
                _judgeQueue[i] = _judgeQueue[j];
                _judgeQueue[j] = temp;
            }
        }
    }

    /// <summary>
    /// Picks and removes the next judge index from the queue. Rebuilds the queue
    /// when exhausted. Rotates the front entry to the back when avoidRepeat is
    /// enabled and the queue has more than one entry.
    /// </summary>
    private int PickNextJudgeIndex()
    {
        if (_judgeQueue == null || _judgeQueue.Count == 0)
            RebuildQueue();

        if (avoidRepeat && _judgeQueue.Count > 1 && _judgeQueue[0] == _lastJudgeIndex)
        {
            // Rotate the repeated index to the end so a different judge goes first.
            int repeated = _judgeQueue[0];
            _judgeQueue.RemoveAt(0);
            _judgeQueue.Add(repeated);
        }

        int chosen = _judgeQueue[0];
        _judgeQueue.RemoveAt(0);
        _lastJudgeIndex = chosen;
        return chosen;
    }
}
