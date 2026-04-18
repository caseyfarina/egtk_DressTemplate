using UnityEngine;
using UnityEditor;

public static class DirectEquip
{
    public static void Execute()
    {
        GameObject charRoot = GameObject.Find("[CharacterRoot]");
        if (charRoot == null) { Debug.LogError("[Direct] CharacterRoot not found"); return; }

        // Log all Layer_ children
        Debug.Log($"[Direct] CharacterRoot has {charRoot.transform.childCount} children:");
        for (int i = 0; i < charRoot.transform.childCount; i++)
        {
            Transform child = charRoot.transform.GetChild(i);
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            Debug.Log($"[Direct]   [{i}] {child.name} sr={(sr != null ? "YES" : "NO")} sprite={(sr?.sprite != null ? sr.sprite.name : "null")}");
        }

        // Find the Layer_Hat SpriteRenderer directly
        Transform hatLayer = charRoot.transform.Find("Layer_Hat");
        if (hatLayer == null) { Debug.LogError("[Direct] Layer_Hat not found"); return; }

        SpriteRenderer hatSR = hatLayer.GetComponent<SpriteRenderer>();
        if (hatSR == null) { Debug.LogError("[Direct] No SpriteRenderer on Layer_Hat"); return; }

        // Load beanie sprite
        var guids = AssetDatabase.FindAssets("t:ClothingItemData");
        ClothingItemData hat = null;
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ClothingItemData item = AssetDatabase.LoadAssetAtPath<ClothingItemData>(path);
            if (item != null && item.Category == ClothingCategory.Hat && item.Sprite != null)
            { hat = item; break; }
        }

        if (hat == null) { Debug.LogError("[Direct] No hat with sprite found"); return; }

        Debug.Log($"[Direct] Found hat: {hat.name} sprite={hat.Sprite.name} canvasX={hat.CanvasX} canvasY={hat.CanvasY}");

        hatSR.sprite = hat.Sprite;
        int w = Mathf.RoundToInt(hat.Sprite.rect.width);
        int h = Mathf.RoundToInt(hat.Sprite.rect.height);
        Vector2 pos = hat.GetWorldPosition(100f, 2048, 2048, w, h);
        hatLayer.localPosition = new Vector3(pos.x, pos.y, 0f);

        Debug.Log($"[Direct] Set hat sprite on Layer_Hat at localPos={hatLayer.localPosition}");
        Debug.Log($"[Direct] Hat world pos = {hatLayer.position}");
        Debug.Log($"[Direct] hatSR.sprite after set = {hatSR.sprite?.name ?? "null"}");
    }
}
