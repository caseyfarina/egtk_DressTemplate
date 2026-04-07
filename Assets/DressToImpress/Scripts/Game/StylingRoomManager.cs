using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Top-level coordinator for the Styling Room scene. Manages the round loop:
/// present judge → player dresses character → player submits → score →
/// judge reacts → next round. Wire the Submit, Next, Boutique, and MainMenu
/// buttons to the public methods on this component.
/// </summary>
public class StylingRoomManager : MonoBehaviour
{
    // ── Serialized fields ──────────────────────────────────────────────────

    [Header("References")]
    [Tooltip("The CharacterDisplay that renders the dressed character.")]
    [SerializeField] private CharacterDisplay characterDisplay;

    [Tooltip("The JudgeManager that presents judges and reacts to scores.")]
    [SerializeField] private JudgeManager judgeManager;

    [Tooltip("The OutfitScorer that calculates the round score.")]
    [SerializeField] private OutfitScorer outfitScorer;

    [Tooltip("The ClothingPanelManager that controls the clothing selection UI.")]
    [SerializeField] private ClothingPanelManager clothingPanel;

    [Header("Scene Names")]
    [Tooltip("Name of the Boutique scene to load when the player clicks BOUTIQUE.")]
    [SerializeField] private string boutiqueSceneName = "Boutique";

    [Tooltip("Name of the Main Menu scene to load when the player clicks MAIN MENU.")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Round Settings")]
    [Tooltip("Seconds to wait after CalculateScore fires before calling judgeManager.ReactToScore. Gives the score display time to animate.")]
    [SerializeField] private float scoreReactionDelay = 1.5f;

    // ── UnityEvents ────────────────────────────────────────────────────────

    [Header("Events")]
    /// <summary>Fires at the start of each round, after the judge is presented.</summary>
    public UnityEvent onRoundStart;

    /// <summary>Fires when the player submits an outfit, before scoring.</summary>
    public UnityEvent onRoundEnd;

    /// <summary>
    /// Fires with the calculated score. Wire to GameCollectionManager.Increment for money,
    /// or to a score popup animator to display the result.
    /// </summary>
    public UnityEvent<int> onScoreReady;

    // ── Private state ──────────────────────────────────────────────────────

    private bool _isSubmitting = false;

    // ── Unity lifecycle ────────────────────────────────────────────────────

    private void Start()
    {
        // Apply the player's character customisation choices to the display.
        if (characterDisplay != null)
        {
            characterDisplay.ApplyProfile(CharacterProfile.Instance);
        }
        else
        {
            Debug.LogWarning("[StylingRoomManager] characterDisplay is not assigned. Character profile will not be applied.", this);
        }

        // Subscribe to OutfitScorer's score event before presenting the first judge,
        // so we never miss a score that fires synchronously.
        if (outfitScorer != null)
        {
            outfitScorer.onScoreCalculated.AddListener(OnScoreReceived);
        }
        else
        {
            Debug.LogWarning("[StylingRoomManager] outfitScorer is not assigned. Scoring will not trigger judge reactions.", this);
        }

        // Present the first judge and begin round 1.
        if (judgeManager != null)
        {
            judgeManager.PresentNextJudge();
        }
        else
        {
            Debug.LogWarning("[StylingRoomManager] judgeManager is not assigned. No judge will be presented.", this);
        }

        onRoundStart?.Invoke();
    }

    private void OnDestroy()
    {
        // Always unsubscribe to prevent memory leaks and stale delegate calls
        // after the GameObject is destroyed.
        if (outfitScorer != null)
        {
            outfitScorer.onScoreCalculated.RemoveListener(OnScoreReceived);
        }
    }

    // ── Public API ─────────────────────────────────────────────────────────

    /// <summary>
    /// Called when the player clicks SUBMIT OUTFIT.
    /// Disables the clothing panel, fires onRoundEnd, scores the outfit, and
    /// starts the judge reaction sequence after a brief delay so score
    /// animations have time to play. Guards against double-submission if the
    /// player clicks rapidly.
    /// Wire the Submit button's onClick to this method.
    /// </summary>
    public void OnSubmitOutfit()
    {
        if (_isSubmitting) return;

        _isSubmitting = true;

        if (clothingPanel != null)
            clothingPanel.SetInteractable(false);

        onRoundEnd?.Invoke();

        // Triggers OnScoreReceived via the subscription set up in Start(),
        // which then starts the DelayedReaction coroutine.
        if (outfitScorer != null)
        {
            outfitScorer.CalculateScore();
        }
        else
        {
            Debug.LogWarning("[StylingRoomManager] outfitScorer is not assigned. Cannot calculate score.", this);
        }
    }

    /// <summary>
    /// Called when the player clicks NEXT JUDGE.
    /// Clears the current outfit, re-enables the clothing panel, presents the
    /// next judge, and fires onRoundStart.
    /// Wire the Next button's onClick to this method.
    /// </summary>
    public void OnNextJudge()
    {
        _isSubmitting = false;

        if (characterDisplay != null)
            characterDisplay.UnequipAllClothing();

        if (clothingPanel != null)
        {
            clothingPanel.SetInteractable(true);
            clothingPanel.RefreshButtonStates();
        }

        if (judgeManager != null)
        {
            judgeManager.PresentNextJudge();
        }
        else
        {
            Debug.LogWarning("[StylingRoomManager] judgeManager is not assigned. Cannot present next judge.", this);
        }

        onRoundStart?.Invoke();
    }

    /// <summary>
    /// Called when the player clicks BOUTIQUE.
    /// Loads the boutique scene so the player can purchase new clothing items.
    /// Wire the Boutique button's onClick to this method.
    /// </summary>
    public void OnGoToBoutique()
    {
        if (string.IsNullOrEmpty(boutiqueSceneName))
        {
            Debug.LogWarning("[StylingRoomManager] boutiqueSceneName is empty. Cannot load boutique scene.", this);
            return;
        }

        SceneManager.LoadScene(boutiqueSceneName);
    }

    /// <summary>
    /// Returns to the main menu.
    /// Wire the Main Menu button's onClick to this method.
    /// </summary>
    public void OnGoToMainMenu()
    {
        if (string.IsNullOrEmpty(mainMenuSceneName))
        {
            Debug.LogWarning("[StylingRoomManager] mainMenuSceneName is empty. Cannot load main menu scene.", this);
            return;
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }

    // ── Private helpers ────────────────────────────────────────────────────

    /// <summary>
    /// Handles the score value produced by OutfitScorer. Fires onScoreReady so
    /// any wired UI can display the score, then starts the delayed reaction
    /// coroutine so judgeManager.ReactToScore is called after the score
    /// animation has had time to play.
    /// </summary>
    private void OnScoreReceived(int score)
    {
        onScoreReady?.Invoke(score);
        StartCoroutine(DelayedReaction(score));
    }

    /// <summary>
    /// Waits for <see cref="scoreReactionDelay"/> seconds before asking the
    /// judge to react. This gives score-display animations time to finish
    /// before the judge dialogue appears.
    /// </summary>
    private IEnumerator DelayedReaction(int score)
    {
        yield return new WaitForSeconds(scoreReactionDelay);

        if (judgeManager != null)
        {
            judgeManager.ReactToScore(score);
        }
        else
        {
            Debug.LogWarning("[StylingRoomManager] judgeManager is not assigned. Cannot trigger judge reaction.", this);
        }
    }
}
