using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine.Events;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class FixJudgeWiring
{
    public static void Execute()
    {
        bool dirty = false;

        // ── 1. Wire JudgeManager text events to UI TMP fields ─────────────
        JudgeManager jm = GameObject.FindFirstObjectByType<JudgeManager>();
        if (jm == null) { Debug.LogError("[Wire] JudgeManager not found"); return; }

        WireStringEvent(jm.onJudgeNameSet,  "[JudgeNameText]", ref dirty);
        WireStringEvent(jm.onStyleTagSet,   "[StyleTagText]",  ref dirty);
        WireStringEvent(jm.onPromptSet,     "[PromptText]",    ref dirty);

        // ── 2. Wire onAvatarSet → AvatarImage.sprite via SerializedObject ─
        var avatarGO = GameObject.Find("[AvatarImage]");
        Image avatarImg = avatarGO?.GetComponent<Image>();
        if (avatarImg != null)
        {
            var so = new SerializedObject(jm);
            var avatarProp = so.FindProperty("onAvatarSet");
            if (avatarProp != null && avatarProp.FindPropertyRelative("m_PersistentCalls.m_Calls").arraySize == 0)
            {
                // Image.sprite has no direct UnityAction<Sprite> binding in Unity's API,
                // so we note this and leave it — wire manually to Image.sprite in Inspector.
                Debug.LogWarning("[Wire] onAvatarSet → [AvatarImage].sprite must be wired manually in the Inspector (Unity limitation: property setters can't be bound as persistent listeners from code).");
            }
        }
        else Debug.LogWarning("[Wire] [AvatarImage] Image component not found.");

        // ── 3. Verify SUBMIT and NEXT button wiring ────────────────────────
        StylingRoomManager srm = GameObject.FindFirstObjectByType<StylingRoomManager>();
        if (srm == null) { Debug.LogError("[Wire] StylingRoomManager not found"); return; }

        CheckAndWireButton("[ButtonSubmit]", srm, "OnSubmitOutfit", ref dirty);
        CheckAndWireButton("[ButtonNext]",   srm, "OnNextJudge",    ref dirty);

        // ── 4. Save scene ──────────────────────────────────────────────────
        if (dirty)
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("[Wire] Scene saved.");
        }
        else
        {
            Debug.Log("[Wire] Everything already wired — no changes needed.");
        }
    }

    static void WireStringEvent(UnityEvent<string> evt, string goName, ref bool dirty)
    {
        var go = GameObject.Find(goName);
        if (go == null) { Debug.LogWarning($"[Wire] {goName} not found"); return; }

        TMP_Text tmp = go.GetComponent<TMP_Text>();
        if (tmp == null) { Debug.LogWarning($"[Wire] No TMP_Text on {goName}"); return; }

        int existingCount = evt.GetPersistentEventCount();
        for (int i = 0; i < existingCount; i++)
        {
            if (evt.GetPersistentTarget(i) == (Object)tmp)
            {
                Debug.Log($"[Wire] {goName} already wired ({evt.GetPersistentMethodName(i)})");
                return;
            }
        }

        // TMP_Text.SetText(string) — find the single-string overload
        var method = typeof(TMP_Text).GetMethod("SetText",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
            null,
            new System.Type[] { typeof(string) },
            null);

        if (method == null)
        {
            Debug.LogWarning($"[Wire] TMP_Text.SetText(string) not found via reflection");
            return;
        }

        var action = (UnityAction<string>)System.Delegate.CreateDelegate(
            typeof(UnityAction<string>), tmp, method);

        UnityEventTools.AddPersistentListener(evt, action);
        EditorUtility.SetDirty(tmp.gameObject);
        dirty = true;
        Debug.Log($"[Wire] Wired {goName}.SetText to event ✓");
    }

    static void CheckAndWireButton(string goName, StylingRoomManager srm, string methodName, ref bool dirty)
    {
        var go = GameObject.Find(goName);
        if (go == null) { Debug.LogWarning($"[Wire] {goName} not found"); return; }

        Button btn = go.GetComponent<Button>();
        if (btn == null)
        {
            // Check children
            btn = go.GetComponentInChildren<Button>();
        }
        if (btn == null) { Debug.LogWarning($"[Wire] No Button on {goName}"); return; }

        // Check if already wired
        for (int i = 0; i < btn.onClick.GetPersistentEventCount(); i++)
        {
            if (btn.onClick.GetPersistentTarget(i) == (Object)srm &&
                btn.onClick.GetPersistentMethodName(i) == methodName)
            {
                Debug.Log($"[Wire] {goName} → {methodName} already wired ✓");
                return;
            }
        }

        var method = typeof(StylingRoomManager).GetMethod(methodName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (method == null) { Debug.LogWarning($"[Wire] Method {methodName} not found on StylingRoomManager"); return; }

        var action = (UnityAction)System.Delegate.CreateDelegate(typeof(UnityAction), srm, method);
        UnityEventTools.AddVoidPersistentListener(btn.onClick, action);
        EditorUtility.SetDirty(btn.gameObject);
        dirty = true;
        Debug.Log($"[Wire] {goName} → {methodName} wired ✓");
    }
}
