using UnityEngine;
using UnityEditor;

public static class EquipFullOutfit
{
    public static void Execute()
    {
        CharacterDisplay cd = GameObject.FindFirstObjectByType<CharacterDisplay>();
        if (cd == null) { Debug.LogError("[Outfit] CharacterDisplay not found"); return; }

        var guids = AssetDatabase.FindAssets("t:ClothingItemData");
        ClothingItemData hat = null, top = null, bottom = null, shoes = null, socks = null;

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ClothingItemData item = AssetDatabase.LoadAssetAtPath<ClothingItemData>(path);
            if (item == null || item.Sprite == null) continue;
            if (hat == null   && item.Category == ClothingCategory.Hat)          hat = item;
            if (top == null   && item.Category == ClothingCategory.Top)          top = item;
            if (bottom == null && item.Category == ClothingCategory.Bottom)      bottom = item;
            if (shoes == null && item.Category == ClothingCategory.Shoes)        shoes = item;
            if (socks == null && item.Category == ClothingCategory.SocksLeggings) socks = item;
        }

        GameObject charRoot = cd.gameObject;
        // Set sprites directly on layers (bypassing dictionary issue)
        void SetLayer(string name, ClothingItemData item)
        {
            if (item == null) return;
            Transform t = charRoot.transform.Find(name);
            if (t == null) { Debug.LogWarning($"[Outfit] {name} not found"); return; }
            SpriteRenderer sr = t.GetComponent<SpriteRenderer>();
            if (sr == null) return;
            sr.sprite = item.Sprite;
            int w = Mathf.RoundToInt(item.Sprite.rect.width);
            int h = Mathf.RoundToInt(item.Sprite.rect.height);
            Vector2 pos = item.GetWorldPosition(100f, 2048, 2048, w, h);
            t.localPosition = new Vector3(pos.x, pos.y, 0f);
            Debug.Log($"[Outfit] {name} → {item.name} localPos={t.localPosition} worldPos={t.position}");
        }

        SetLayer("Layer_Hat",          hat);
        SetLayer("Layer_Top",          top);
        SetLayer("Layer_Bottom",       bottom);
        SetLayer("Layer_Shoes",        shoes);
        SetLayer("Layer_SocksLeggings", socks);
    }
}
