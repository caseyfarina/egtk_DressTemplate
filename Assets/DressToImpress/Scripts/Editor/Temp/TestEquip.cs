using UnityEngine;
using UnityEngine.UI;

public static class TestEquip
{
    public static void Execute()
    {
        // Find the first item button in the ClothingPanel scroll area and click it
        ClothingPanelManager cpm = GameObject.FindFirstObjectByType<ClothingPanelManager>();
        if (cpm == null) { Debug.LogError("[TestEquip] ClothingPanelManager not found"); return; }

        // Find item buttons in the content area
        Transform content = GameObject.Find("[ItemContent]")?.transform;
        if (content == null)
        {
            Debug.LogError("[TestEquip] [ItemContent] not found, searching for ScrollContent...");
            content = GameObject.Find("[ScrollContent]")?.transform;
        }
        if (content == null)
        {
            // Try to find via ClothingPanelManager hierarchy
            var scrollRect = cpm.GetComponentInChildren<UnityEngine.UI.ScrollRect>();
            content = scrollRect?.content;
            Debug.Log($"[TestEquip] Found ScrollRect content: {content?.name}");
        }

        if (content == null) { Debug.LogError("[TestEquip] Could not find item content transform"); return; }

        Debug.Log($"[TestEquip] Content '{content.name}' has {content.childCount} children");

        // Click first button
        Button first = content.GetComponentInChildren<Button>();
        if (first == null) { Debug.LogError("[TestEquip] No buttons found in content"); return; }

        Debug.Log($"[TestEquip] Clicking button: {first.gameObject.name}");
        first.onClick.Invoke();

        // Also log what the character looks like
        GameObject charRoot = GameObject.Find("[CharacterRoot]");
        if (charRoot != null)
        {
            Debug.Log($"[TestEquip] CharacterRoot pos: {charRoot.transform.position}, childCount: {charRoot.transform.childCount}");
            for (int i = 0; i < charRoot.transform.childCount; i++)
            {
                var child = charRoot.transform.GetChild(i);
                var sr = child.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null)
                    Debug.Log($"[TestEquip]   Layer '{child.name}': sprite={sr.sprite.name} localPos={child.localPosition} sortOrder={sr.sortingOrder}");
            }
        }
    }
}
