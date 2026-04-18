using UnityEngine;

public static class DiagnoseEquip
{
    public static void Execute()
    {
        // Find CharacterDisplay instances
        var allCD = GameObject.FindObjectsByType<CharacterDisplay>(FindObjectsSortMode.None);
        Debug.Log($"[DiagEquip] CharacterDisplay instances in scene: {allCD.Length}");
        foreach (var c in allCD)
            Debug.Log($"  CD: {c.gameObject.name} (InstanceID={c.GetInstanceID()})");

        // Get OutfitScorer's reference
        OutfitScorer scorer = GameObject.FindFirstObjectByType<OutfitScorer>();
        if (scorer != null)
        {
            var so = new UnityEditor.SerializedObject(scorer);
            var cdProp = so.FindProperty("characterDisplay");
            var cdRef = cdProp?.objectReferenceValue as CharacterDisplay;
            Debug.Log($"[DiagEquip] OutfitScorer.characterDisplay = {cdRef?.gameObject.name} (InstanceID={cdRef?.GetInstanceID()})");
        }

        // Try equipping one item and immediately check preview score
        CharacterDisplay cd = GameObject.FindFirstObjectByType<CharacterDisplay>();
        if (cd == null) { Debug.LogError("[DiagEquip] No CharacterDisplay found"); return; }

        var guids = UnityEditor.AssetDatabase.FindAssets("t:ClothingItemData");
        ClothingItemData testItem = null;
        foreach (var guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            ClothingItemData item = UnityEditor.AssetDatabase.LoadAssetAtPath<ClothingItemData>(path);
            if (item != null && item.Category == ClothingCategory.Hat)
            {
                testItem = item;
                break;
            }
        }

        if (testItem == null) { Debug.LogWarning("[DiagEquip] No Hat item found"); return; }

        Debug.Log($"[DiagEquip] Before EquipItem — GetEquippedItem(Hat) = {cd.GetEquippedItem(ClothingCategory.Hat)?.name ?? "null"}");
        Debug.Log($"[DiagEquip] Before EquipItem — PreviewScore = {scorer?.PreviewScore()}");

        cd.EquipItem(testItem);
        Debug.Log($"[DiagEquip] After EquipItem({testItem.name}) — GetEquippedItem(Hat) = {cd.GetEquippedItem(ClothingCategory.Hat)?.name ?? "null"}");
        Debug.Log($"[DiagEquip] After EquipItem — PreviewScore = {scorer?.PreviewScore()}");
    }
}
