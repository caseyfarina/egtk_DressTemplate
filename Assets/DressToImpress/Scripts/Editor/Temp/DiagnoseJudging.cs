using UnityEngine;
using UnityEditor;

public static class DiagnoseJudging
{
    public static void Execute()
    {
        Debug.Log("===== JUDGING SYSTEM AUDIT =====");

        // ── JudgeManager ──────────────────────────────────────────────────
        JudgeManager jm = GameObject.FindFirstObjectByType<JudgeManager>();
        if (jm == null) { Debug.LogError("[Audit] JudgeManager NOT FOUND in scene"); }
        else
        {
            var so = new SerializedObject(jm);
            var judgesProp = so.FindProperty("judges");
            Debug.Log($"[JudgeManager] judges array size: {judgesProp?.arraySize}");
            if (judgesProp != null)
            {
                for (int i = 0; i < judgesProp.arraySize; i++)
                {
                    var elem = judgesProp.GetArrayElementAtIndex(i);
                    var judgeData = elem.objectReferenceValue as JudgeData;
                    if (judgeData == null)
                        Debug.LogWarning($"[JudgeManager]   [{i}] NULL");
                    else
                        Debug.Log($"[JudgeManager]   [{i}] {judgeData.name} | name='{judgeData.JudgeName}' tag='{judgeData.StyleTag}' avatar={(judgeData.AvatarSprite != null ? judgeData.AvatarSprite.name : "NULL")} prompt='{judgeData.PromptText}'");
                }
            }

            // Check current judge
            var currentJudge = jm.GetCurrentJudge();
            Debug.Log($"[JudgeManager] Current judge: {(currentJudge != null ? currentJudge.JudgeName : "NONE (not presented yet)")}");
        }

        // ── StylingRoomManager ────────────────────────────────────────────
        StylingRoomManager srm = GameObject.FindFirstObjectByType<StylingRoomManager>();
        if (srm == null) { Debug.LogError("[Audit] StylingRoomManager NOT FOUND"); }
        else
        {
            var so = new SerializedObject(srm);
            Debug.Log($"[StylingRoomManager] characterDisplay={so.FindProperty("characterDisplay")?.objectReferenceValue?.name ?? "NULL"}");
            Debug.Log($"[StylingRoomManager] judgeManager={so.FindProperty("judgeManager")?.objectReferenceValue?.name ?? "NULL"}");
            Debug.Log($"[StylingRoomManager] outfitScorer={so.FindProperty("outfitScorer")?.objectReferenceValue?.name ?? "NULL"}");
            Debug.Log($"[StylingRoomManager] clothingPanel={so.FindProperty("clothingPanel")?.objectReferenceValue?.name ?? "NULL"}");
        }

        // ── OutfitScorer ──────────────────────────────────────────────────
        OutfitScorer scorer = GameObject.FindFirstObjectByType<OutfitScorer>();
        if (scorer == null) { Debug.LogError("[Audit] OutfitScorer NOT FOUND"); }
        else
        {
            var so = new SerializedObject(scorer);
            Debug.Log($"[OutfitScorer] characterDisplay={so.FindProperty("characterDisplay")?.objectReferenceValue?.name ?? "NULL"}");
            Debug.Log($"[OutfitScorer] judgeManager={so.FindProperty("judgeManager")?.objectReferenceValue?.name ?? "NULL"}");
            Debug.Log($"[OutfitScorer] basePoints={so.FindProperty("basePointsPerItem")?.intValue} themeBonus={so.FindProperty("themeBonusPerMatch")?.intValue} completeBonus={so.FindProperty("completeOutfitBonus")?.intValue}");
            Debug.Log($"[OutfitScorer] excellentThreshold={so.FindProperty("excellentThreshold")?.intValue} goodThreshold={so.FindProperty("goodThreshold")?.intValue}");

            // Simulate a score
            int preview = scorer.PreviewScore();
            Debug.Log($"[OutfitScorer] Current PreviewScore (no outfit): {preview}");
        }

        // ── ClothingPanelManager ──────────────────────────────────────────
        ClothingPanelManager cpm = GameObject.FindFirstObjectByType<ClothingPanelManager>();
        if (cpm == null) { Debug.LogError("[Audit] ClothingPanelManager NOT FOUND"); }
        else
        {
            var so = new SerializedObject(cpm);
            Debug.Log($"[ClothingPanelManager] characterDisplay={so.FindProperty("characterDisplay")?.objectReferenceValue?.name ?? "NULL"}");
            Debug.Log($"[ClothingPanelManager] allClothingItems count={so.FindProperty("allClothingItems")?.arraySize}");
        }

        // ── JudgeData assets (from AssetDatabase) ─────────────────────────
        Debug.Log("--- JudgeData Asset Inspection ---");
        var guids = AssetDatabase.FindAssets("t:JudgeData");
        Debug.Log($"Found {guids.Length} JudgeData assets in project");
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            JudgeData jd = AssetDatabase.LoadAssetAtPath<JudgeData>(path);
            if (jd == null) continue;
            string[] dialogue = jd.GetDialogueForRating(OutfitRating.Excellent);
            Debug.Log($"  {jd.name}: name='{jd.JudgeName}' tag='{jd.StyleTag}' avatar={(jd.AvatarSprite != null ? "YES" : "NULL")} excellent_dialogue={dialogue?.Length} lines");
        }

        // ── UI Text fields ────────────────────────────────────────────────
        Debug.Log("--- UI Wire Check ---");
        string[] uiNames = { "[JudgeNameText]", "[StyleTagText]", "[PromptText]", "[AvatarImage]", "[MoneyText]" };
        foreach (var name in uiNames)
        {
            var go = GameObject.Find(name);
            Debug.Log($"  {name}: {(go != null ? "FOUND" : "NOT FOUND")}");
        }

        Debug.Log("===== AUDIT COMPLETE =====");
    }
}
