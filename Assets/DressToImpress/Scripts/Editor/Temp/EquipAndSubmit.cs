using UnityEngine;

public static class EquipAndSubmit
{
    public static void Execute()
    {
        CharacterDisplay cd = GameObject.FindFirstObjectByType<CharacterDisplay>();
        if (cd == null) { Debug.LogError("[E&S] CharacterDisplay not found"); return; }

        var allItems = UnityEngine.Object.FindObjectsByType<ClothingPanelManager>(FindObjectsSortMode.None);
        if (allItems.Length == 0) { Debug.LogError("[E&S] ClothingPanelManager not found"); return; }

        // Get some items via AssetDatabase
        var guids = UnityEditor.AssetDatabase.FindAssets("t:ClothingItemData");
        if (guids.Length == 0) { Debug.LogError("[E&S] No ClothingItemData assets found"); return; }

        int equipped = 0;
        foreach (var guid in guids)
        {
            if (equipped >= 5) break;
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            ClothingItemData item = UnityEditor.AssetDatabase.LoadAssetAtPath<ClothingItemData>(path);
            if (item == null) continue;
            cd.EquipItem(item);
            Debug.Log($"[E&S] Equipped {item.name} (category={item.Category})");
            equipped++;
        }

        OutfitScorer scorer = GameObject.FindFirstObjectByType<OutfitScorer>();
        if (scorer != null)
            Debug.Log($"[E&S] PreviewScore after equipping: {scorer.PreviewScore()}");

        StylingRoomManager srm = GameObject.FindFirstObjectByType<StylingRoomManager>();
        if (srm == null) { Debug.LogError("[E&S] StylingRoomManager not found"); return; }

        Debug.Log("[E&S] Calling OnSubmitOutfit...");
        srm.OnSubmitOutfit();
    }
}
