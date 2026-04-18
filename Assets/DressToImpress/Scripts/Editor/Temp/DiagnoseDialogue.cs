using UnityEngine;
using UnityEditor;

public static class DiagnoseDialogue
{
    public static void Execute()
    {
        var guids = AssetDatabase.FindAssets("t:JudgeData");
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            JudgeData jd = AssetDatabase.LoadAssetAtPath<JudgeData>(path);
            if (jd == null) continue;

            string[] excellent = jd.GetDialogueForRating(OutfitRating.Excellent);
            string[] good      = jd.GetDialogueForRating(OutfitRating.Good);
            string[] poor      = jd.GetDialogueForRating(OutfitRating.Poor);

            Debug.Log($"[Dialogue] {jd.name} ({jd.JudgeName} / {jd.StyleTag})");
            Debug.Log($"  Excellent ({excellent?.Length} lines): {(excellent?.Length > 0 ? excellent[0] : "EMPTY")}");
            Debug.Log($"  Good      ({good?.Length} lines): {(good?.Length > 0 ? good[0] : "EMPTY")}");
            Debug.Log($"  Poor      ({poor?.Length} lines): {(poor?.Length > 0 ? poor[0] : "EMPTY")}");

            // Also check money rewards
            var so = new SerializedObject(jd);
            Debug.Log($"  rewards: excellent={so.FindProperty("excellentReward")?.intValue} good={so.FindProperty("goodReward")?.intValue} poor={so.FindProperty("poorReward")?.intValue}");
        }
    }
}
