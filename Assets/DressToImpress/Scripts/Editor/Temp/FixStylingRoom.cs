using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using System.Linq;

public static class FixStylingRoom
{
    public static void Execute()
    {
        // ── 1. Remove default perspective Main Camera ─────────────────────────────
        GameObject defaultCam = GameObject.Find("Main Camera");
        if (defaultCam != null) { GameObject.DestroyImmediate(defaultCam); Debug.Log("[Fix] Removed Main Camera."); }

        // ── 2. Position CharacterRoot so body center lands near world (0,0) ─────────
        // Body sprite local center relative to CharacterRoot: (-4.08, 3.86)
        // Camera orthoSize=5.5, center (0,0). Offset CharacterRoot to cancel body offset.
        // Shift right 4.08, down 3.86. Leave slight upward bias (+0.5) for visual breathing room.
        GameObject charRoot = GameObject.Find("[CharacterRoot]");
        if (charRoot != null)
        {
            charRoot.transform.position = new Vector3(4.08f, -3.36f, 0f);
            Debug.Log("[Fix] CharacterRoot at (4.08, -3.36, 0) — body centered in camera view.");
        }

        // ── 3. Load all ClothingItemData assets ───────────────────────────────────
        var allItems = AssetDatabase.FindAssets("t:ClothingItemData")
            .Select(g => AssetDatabase.LoadAssetAtPath<ClothingItemData>(AssetDatabase.GUIDToAssetPath(g)))
            .Where(a => a != null)
            .ToArray();

        // Clothing panel needs clothing-only items (not body/facial/hair)
        ClothingCategory[] clothingCats = {
            ClothingCategory.Hat, ClothingCategory.Top, ClothingCategory.Bottom,
            ClothingCategory.Skirt, ClothingCategory.Dress, ClothingCategory.Shoes,
            ClothingCategory.SocksLeggings, ClothingCategory.Outerwear, ClothingCategory.Accessory
        };
        var clothingItems = allItems.Where(a => clothingCats.Contains(a.Category)).ToArray();

        // CharacterDisplay also needs body+face data to apply CharacterProfile
        var bodyTypes  = allItems.Where(a => a.Category == ClothingCategory.BodyBase).OrderBy(a => a.name).ToArray();
        var eyes       = allItems.Where(a => a.Category == ClothingCategory.Eyes).OrderBy(a => a.name).ToArray();
        var eyebrows   = allItems.Where(a => a.Category == ClothingCategory.Eyebrows).OrderBy(a => a.name).ToArray();
        var mouths     = allItems.Where(a => a.Category == ClothingCategory.Mouth).OrderBy(a => a.name).ToArray();
        var ears       = allItems.Where(a => a.Category == ClothingCategory.Ears).OrderBy(a => a.name).ToArray();
        var noses      = allItems.Where(a => a.Category == ClothingCategory.Nose).OrderBy(a => a.name).ToArray();

        Debug.Log($"[Fix] Clothing items: {clothingItems.Length}  BodyBase:{bodyTypes.Length}");

        // ── 4. Assign body+face arrays to CharacterDisplay ────────────────────────
        CharacterDisplay display = charRoot?.GetComponent<CharacterDisplay>();
        if (display != null)
        {
            var so = new SerializedObject(display);
            SetArray(so, "bodyTypes",  bodyTypes);
            SetArray(so, "eyes",       eyes);
            SetArray(so, "eyebrows",   eyebrows);
            SetArray(so, "mouths",     mouths);
            SetArray(so, "ears",       ears);
            SetArray(so, "noses",      noses);
            so.ApplyModifiedProperties();
            Debug.Log("[Fix] CharacterDisplay arrays assigned.");
        }

        // ── 5. Assign clothing items to ClothingPanelManager ─────────────────────
        ClothingPanelManager cpm = GameObject.FindFirstObjectByType<ClothingPanelManager>();
        if (cpm != null)
        {
            var so = new SerializedObject(cpm);
            SetArray(so, "allClothingItems", clothingItems);
            so.ApplyModifiedProperties();
            Debug.Log("[Fix] ClothingPanelManager items assigned.");
        }

        // ── 6. Assign JudgeData assets to JudgeManager ───────────────────────────
        JudgeManager judgeManager = GameObject.FindFirstObjectByType<JudgeManager>();
        if (judgeManager != null)
        {
            var judgeAssets = AssetDatabase.FindAssets("t:JudgeData")
                .Select(g => AssetDatabase.LoadAssetAtPath<JudgeData>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(a => a != null)
                .ToArray();

            var so = new SerializedObject(judgeManager);
            SetArray(so, "judges", judgeAssets);
            so.ApplyModifiedProperties();
            Debug.Log($"[Fix] JudgeManager: {judgeAssets.Length} judges assigned.");

            // Wire JudgeManager events to UI text fields (persistent listeners)
            var judgeNameTMP  = FindTMP("[JudgeNameText]");
            var styleTagTMP   = FindTMP("[StyleTagText]");
            var promptTMP     = FindTMP("[PromptText]");
            var avatarImg     = GameObject.Find("[AvatarImage]")?.GetComponent<Image>();

            // Note: JudgeManager string/sprite events need Unity Object targets for
            // persistent wiring — wire these manually in the Inspector:
            //   onJudgeNameSet → [JudgeNameText].text
            //   onStyleTagSet  → [StyleTagText].text
            //   onPromptSet    → [PromptText].text
            //   onAvatarSet    → [AvatarImage].sprite
            Debug.Log("[Fix] JudgeManager: wire UI events manually in Inspector.");
        }

        // ── 7. Wire MoneyText to StylingRoomManager.onScoreReady ─────────────────
        // We'll display score as money text for now
        var moneyTMP = FindTMP("[MoneyText]");
        StylingRoomManager srm = GameObject.FindFirstObjectByType<StylingRoomManager>();
        if (srm != null && moneyTMP != null)
        {
            Debug.Log("[Fix] StylingRoomManager found. Wire onScoreReady → MoneyText manually if needed.");
        }

        // ── 8. Save scene ──────────────────────────────────────────────────────────
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[Fix] StylingRoom fix complete.");
    }

    private static void SetArray<T>(SerializedObject so, string propName, T[] items) where T : Object
    {
        var prop = so.FindProperty(propName);
        if (prop == null) { Debug.LogWarning($"[Fix] Property '{propName}' not found on {so.targetObject.name}"); return; }
        prop.arraySize = items.Length;
        for (int i = 0; i < items.Length; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
    }

    private static TextMeshProUGUI FindTMP(string goName)
    {
        var go = GameObject.Find(goName);
        return go?.GetComponent<TextMeshProUGUI>();
    }

    private static void WireStringEvent(UnityEngine.Events.UnityEvent<string> evt,
                                         TextMeshProUGUI tmp, string setterName)
    {
        if (tmp == null) return;
        var method = typeof(TextMeshProUGUI).GetProperty("text")?.GetSetMethod();
        if (method == null) method = typeof(TMP_Text).GetMethod(setterName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
            null, new[] { typeof(string) }, null);

        // Use the text property setter via UnityAction<string>
        var action = new UnityEngine.Events.UnityAction<string>(s => tmp.text = s);
        try {
            UnityEventTools.AddPersistentListener(evt, action);
        } catch (System.Exception e) {
            Debug.LogWarning($"[Fix] Could not wire event to {tmp.gameObject.name}: {e.Message}");
        }
    }
}
