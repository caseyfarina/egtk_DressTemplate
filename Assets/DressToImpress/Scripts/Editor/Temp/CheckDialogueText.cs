using UnityEngine;
using TMPro;

public static class CheckDialogueText
{
    public static void Execute()
    {
        var go = GameObject.Find("[DialogueText]");
        if (go == null) { Debug.LogError("[Check] [DialogueText] not found in scene"); return; }

        TMP_Text tmp = go.GetComponent<TMP_Text>();
        RectTransform rt = go.GetComponent<RectTransform>();

        Debug.Log($"[Check] [DialogueText] found: active={go.activeInHierarchy}");
        Debug.Log($"[Check] text content: '{tmp?.text}'");
        Debug.Log($"[Check] color: {tmp?.color}");
        Debug.Log($"[Check] anchorMin={rt?.anchorMin} anchorMax={rt?.anchorMax}");
        Debug.Log($"[Check] anchoredPos={rt?.anchoredPosition} sizeDelta={rt?.sizeDelta}");

        // World corners
        Vector3[] corners = new Vector3[4];
        rt?.GetWorldCorners(corners);
        Debug.Log($"[Check] world corners: BL={corners[0]} BR={corners[3]}");

        // Check parent
        Debug.Log($"[Check] parent: {go.transform.parent?.name}");

        // Also log JudgeManager onJudgeDialogue listener count
        JudgeManager jm = GameObject.FindFirstObjectByType<JudgeManager>();
        if (jm != null)
            Debug.Log($"[Check] onJudgeDialogue persistent listeners: {jm.onJudgeDialogue.GetPersistentEventCount()}");
    }
}
