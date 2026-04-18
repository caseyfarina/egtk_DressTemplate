using UnityEngine;
using UnityEditor;

public static class EquipHat
{
    public static void Execute()
    {
        CharacterDisplay cd = GameObject.FindFirstObjectByType<CharacterDisplay>();
        if (cd == null) { Debug.LogError("[EquipHat] CharacterDisplay not found"); return; }

        // Find first Hat item with a sprite
        var guids = AssetDatabase.FindAssets("t:ClothingItemData");
        ClothingItemData hat = null;
        ClothingItemData top = null;
        ClothingItemData bottom = null;
        ClothingItemData shoes = null;

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ClothingItemData item = AssetDatabase.LoadAssetAtPath<ClothingItemData>(path);
            if (item == null || item.Sprite == null) continue;
            if (hat == null && item.Category == ClothingCategory.Hat) hat = item;
            if (top == null && item.Category == ClothingCategory.Top) top = item;
            if (bottom == null && item.Category == ClothingCategory.Bottom) bottom = item;
            if (shoes == null && item.Category == ClothingCategory.Shoes) shoes = item;
        }

        if (hat != null)    { cd.EquipItem(hat);    Debug.Log($"[EquipHat] Equipped hat: {hat.name}"); }
        if (top != null)    { cd.EquipItem(top);    Debug.Log($"[EquipHat] Equipped top: {top.name}"); }
        if (bottom != null) { cd.EquipItem(bottom); Debug.Log($"[EquipHat] Equipped bottom: {bottom.name}"); }
        if (shoes != null)  { cd.EquipItem(shoes);  Debug.Log($"[EquipHat] Equipped shoes: {shoes.name}"); }

        // Log all active layers
        GameObject charRoot = cd.gameObject;
        for (int i = 0; i < charRoot.transform.childCount; i++)
        {
            Transform child = charRoot.transform.GetChild(i);
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            if (sr?.sprite == null) continue;
            Debug.Log($"[EquipHat] Layer '{child.name}': sprite={sr.sprite.name} localPos={child.localPosition}");
        }
    }
}
