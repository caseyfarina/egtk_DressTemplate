using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class DiagnoseReactionWiring
{
    public static void Execute()
    {
        Debug.Log("===== REACTION WIRING AUDIT =====");

        JudgeManager jm = GameObject.FindFirstObjectByType<JudgeManager>();
        if (jm == null) { Debug.LogError("[Audit] JudgeManager not found"); return; }

        var so = new UnityEditor.SerializedObject(jm);

        int dialogueListeners = jm.onJudgeDialogue.GetPersistentEventCount();
        int moneyListeners    = jm.onMoneyAwarded.GetPersistentEventCount();
        int ratingListeners   = jm.onRatingDetermined.GetPersistentEventCount();

        Debug.Log($"[JudgeManager] onJudgeDialogue persistent listeners: {dialogueListeners}");
        Debug.Log($"[JudgeManager] onMoneyAwarded persistent listeners: {moneyListeners}");
        Debug.Log($"[JudgeManager] onRatingDetermined persistent listeners: {ratingListeners}");

        for (int i = 0; i < moneyListeners; i++)
        {
            var t = jm.onMoneyAwarded.GetPersistentTarget(i);
            Debug.Log($"  onMoneyAwarded[{i}] → {(t != null ? t.name : "NULL")}.{jm.onMoneyAwarded.GetPersistentMethodName(i)}");
        }

        for (int i = 0; i < dialogueListeners; i++)
        {
            var t = jm.onJudgeDialogue.GetPersistentTarget(i);
            Debug.Log($"  onJudgeDialogue[{i}] → {(t != null ? t.name : "NULL")}.{jm.onJudgeDialogue.GetPersistentMethodName(i)}");
        }

        StylingRoomManager srm = GameObject.FindFirstObjectByType<StylingRoomManager>();
        if (srm != null)
        {
            int scoreReady = srm.onScoreReady.GetPersistentEventCount();
            Debug.Log($"[StylingRoomManager] onScoreReady persistent listeners: {scoreReady}");
            for (int i = 0; i < scoreReady; i++)
            {
                var t = srm.onScoreReady.GetPersistentTarget(i);
                Debug.Log($"  onScoreReady[{i}] → {(t != null ? t.name : "NULL")}.{srm.onScoreReady.GetPersistentMethodName(i)}");
            }
        }

        // Check for dialogue UI elements
        string[] candidates = { "[DialogueText]", "[JudgeDialogue]", "[ReactionText]", "[DialoguePannel]", "[DialoguePanel]", "[ScoreText]", "[RatingText]" };
        Debug.Log("--- UI element search ---");
        foreach (var name in candidates)
        {
            var go = GameObject.Find(name);
            Debug.Log($"  {name}: {(go != null ? "FOUND" : "not found")}");
        }

        // List all TMP_Text components in scene
        Debug.Log("--- All TMP_Text in scene ---");
        var allTmp = GameObject.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None);
        foreach (var t in allTmp)
            Debug.Log($"  TMP_Text: '{t.gameObject.name}' text='{t.text}'");

        Debug.Log("===== AUDIT COMPLETE =====");
    }
}
