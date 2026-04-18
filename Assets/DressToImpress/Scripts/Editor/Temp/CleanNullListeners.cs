using UnityEngine;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine.Events;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Reflection;

public static class CleanNullListeners
{
    public static void Execute()
    {
        bool dirty = false;

        JudgeManager jm = GameObject.FindFirstObjectByType<JudgeManager>();
        if (jm == null) { Debug.LogError("[Clean] JudgeManager not found"); return; }

        dirty |= RemoveNullListeners(jm, "onJudgeNameSet");
        dirty |= RemoveNullListeners(jm, "onStyleTagSet");
        dirty |= RemoveNullListeners(jm, "onPromptSet");
        dirty |= RemoveNullListeners(jm, "onJudgeDialogue");
        dirty |= RemoveNullListeners(jm, "onMoneyAwarded");
        dirty |= RemoveNullListeners(jm, "onRatingDetermined");

        if (dirty)
        {
            EditorUtility.SetDirty(jm.gameObject);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("[Clean] Null listeners removed and scene saved.");
        }
        else
        {
            Debug.Log("[Clean] No null listeners found.");
        }

        // Report final state
        Debug.Log($"[Clean] onJudgeNameSet listeners: {jm.onJudgeNameSet.GetPersistentEventCount()}");
        Debug.Log($"[Clean] onStyleTagSet listeners:  {jm.onStyleTagSet.GetPersistentEventCount()}");
        Debug.Log($"[Clean] onPromptSet listeners:    {jm.onPromptSet.GetPersistentEventCount()}");
        Debug.Log($"[Clean] onJudgeDialogue listeners:{jm.onJudgeDialogue.GetPersistentEventCount()}");
    }

    static bool RemoveNullListeners(Object target, string fieldName)
    {
        var so = new SerializedObject(target);
        var prop = so.FindProperty(fieldName);
        if (prop == null) return false;

        var calls = prop.FindPropertyRelative("m_PersistentCalls.m_Calls");
        if (calls == null) return false;

        bool removed = false;
        for (int i = calls.arraySize - 1; i >= 0; i--)
        {
            var call = calls.GetArrayElementAtIndex(i);
            var targetObj = call.FindPropertyRelative("m_Target").objectReferenceValue;
            if (targetObj == null)
            {
                calls.DeleteArrayElementAtIndex(i);
                removed = true;
                Debug.Log($"[Clean] Removed null listener from {fieldName} at index {i}");
            }
        }

        if (removed)
            so.ApplyModifiedPropertiesWithoutUndo();

        return removed;
    }
}
