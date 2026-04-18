using UnityEngine;
using UnityEngine.UI;

public static class DiagnoseAlignment
{
    public static void Execute()
    {
        // ── 1. Check ClothingPanelManager.characterDisplay reference ───────
        ClothingPanelManager cpm = GameObject.FindFirstObjectByType<ClothingPanelManager>();
        if (cpm == null) { Debug.LogError("[Align] ClothingPanelManager not found"); return; }

        var so = new UnityEditor.SerializedObject(cpm);
        var cdProp = so.FindProperty("characterDisplay");
        Debug.Log($"[Align] ClothingPanelManager.characterDisplay = {(cdProp?.objectReferenceValue != null ? cdProp.objectReferenceValue.name : "NULL")}");

        // ── 2. Log all equipped layers with sprite dimensions ──────────────
        CharacterDisplay cd = GameObject.FindFirstObjectByType<CharacterDisplay>();
        if (cd == null) { Debug.LogError("[Align] CharacterDisplay not found"); return; }

        GameObject charRoot = cd.gameObject;
        Debug.Log($"[Align] CharacterDisplay on '{charRoot.name}' at pos {charRoot.transform.position}");

        for (int i = 0; i < charRoot.transform.childCount; i++)
        {
            Transform child = charRoot.transform.GetChild(i);
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            if (sr?.sprite == null) continue;

            Sprite sp = sr.sprite;
            Debug.Log($"[Align] Layer '{child.name}':" +
                $" localPos={child.localPosition}" +
                $" sprite={sp.name}" +
                $" texture={sp.texture.width}x{sp.texture.height}" +
                $" rect={sp.rect.width}x{sp.rect.height}" +
                $" pivot={sp.pivot}");
        }

        // ── 3. Find a hat item and compute its expected position ──────────
        var hatItems = UnityEditor.AssetDatabase.FindAssets("t:ClothingItemData");
        foreach (var guid in hatItems)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            ClothingItemData item = UnityEditor.AssetDatabase.LoadAssetAtPath<ClothingItemData>(path);
            if (item == null || item.Category != ClothingCategory.Hat) continue;
            if (item.Sprite == null) continue;

            int tw = item.Sprite.texture.width;
            int th = item.Sprite.texture.height;
            int rw = Mathf.RoundToInt(item.Sprite.rect.width);
            int rh = Mathf.RoundToInt(item.Sprite.rect.height);

            Vector2 posUsingTexture = item.GetWorldPosition(100f, 2048, 2048, tw, th);
            Vector2 posUsingRect    = item.GetWorldPosition(100f, 2048, 2048, rw, rh);

            Debug.Log($"[Align] Hat '{item.name}' canvasX={item.CanvasX} canvasY={item.CanvasY}" +
                $" textureSize={tw}x{th} rectSize={rw}x{rh}" +
                $" posViaTexture={posUsingTexture} posViaRect={posUsingRect}");
            break; // just check first hat
        }
    }
}
