using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine.Events;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class AddDialogueText
{
    public static void Execute()
    {
        // Find [JudgePanel]
        var judgePanel = GameObject.Find("[JudgePanel]");
        if (judgePanel == null) { Debug.LogError("[Add] [JudgePanel] not found"); return; }

        // Check if [DialogueText] already exists
        if (judgePanel.transform.Find("[DialogueText]") != null)
        {
            Debug.Log("[Add] [DialogueText] already exists — skipping creation.");
        }
        else
        {
            // Create [DialogueText]
            var go = new GameObject("[DialogueText]");
            go.transform.SetParent(judgePanel.transform, false);

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0f, 0f);
            rt.anchorMax        = new Vector2(1f, 0f);
            rt.pivot            = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 10f);
            rt.sizeDelta        = new Vector2(-20f, 80f);

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize          = 16f;
            tmp.color             = new Color(0.9f, 0.9f, 0.7f, 1f);
            tmp.enableWordWrapping = true;
            tmp.textWrappingMode   = TextWrappingModes.Normal;
            tmp.alignment          = TextAlignmentOptions.BottomLeft;
            tmp.text               = string.Empty;

            EditorUtility.SetDirty(go);
            Debug.Log("[Add] Created [DialogueText] in [JudgePanel]");
        }

        // Wire onJudgeDialogue → [DialogueText].SetText
        JudgeManager jm = GameObject.FindFirstObjectByType<JudgeManager>();
        if (jm == null) { Debug.LogError("[Add] JudgeManager not found"); return; }

        var dialogueGO = judgePanel.transform.Find("[DialogueText]");
        if (dialogueGO == null) { Debug.LogError("[Add] Could not find [DialogueText] after creation"); return; }

        TMP_Text dialogueTMP = dialogueGO.GetComponent<TMP_Text>();

        // Check if already wired
        for (int i = 0; i < jm.onJudgeDialogue.GetPersistentEventCount(); i++)
        {
            if (jm.onJudgeDialogue.GetPersistentTarget(i) == (Object)dialogueTMP)
            {
                Debug.Log("[Add] onJudgeDialogue already wired — skipping.");
                return;
            }
        }

        var method = typeof(TMP_Text).GetMethod("SetText",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
            null, new System.Type[] { typeof(string) }, null);

        if (method == null) { Debug.LogError("[Add] TMP_Text.SetText(string) not found"); return; }

        var action = (UnityAction<string>)System.Delegate.CreateDelegate(
            typeof(UnityAction<string>), dialogueTMP, method);

        UnityEventTools.AddPersistentListener(jm.onJudgeDialogue, action);
        EditorUtility.SetDirty(jm.gameObject);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        Debug.Log("[Add] onJudgeDialogue → [DialogueText].SetText wired and scene saved ✓");
    }
}
