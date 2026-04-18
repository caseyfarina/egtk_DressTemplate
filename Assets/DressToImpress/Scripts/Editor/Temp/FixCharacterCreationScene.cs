using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine.Events;
using System.Linq;

public static class FixCharacterCreationScene
{
    public static void Execute()
    {
        // ── 1. Remove the default perspective "Main Camera" ───────────────────────
        GameObject defaultCam = GameObject.Find("Main Camera");
        if (defaultCam != null)
        {
            GameObject.DestroyImmediate(defaultCam);
            Debug.Log("[Fix] Removed 'Main Camera'.");
        }

        // ── 2. Position CharacterRoot on the left portion of the scene ────────────
        // Camera orthographicSize=10.24 at 16:9 → half-width ≈ 18.2 world units.
        // Shift root left so character centres in the left 50% of the screen.
        GameObject charRoot = GameObject.Find("[CharacterRoot]");
        if (charRoot != null)
        {
            charRoot.transform.position = new Vector3(-7f, 0f, 0f);
            Debug.Log("[Fix] Moved [CharacterRoot] to x=-7.");
        }

        // ── 3. Load ClothingItemData assets ───────────────────────────────────────
        var allItems = AssetDatabase.FindAssets("t:ClothingItemData")
            .Select(g => AssetDatabase.LoadAssetAtPath<ClothingItemData>(AssetDatabase.GUIDToAssetPath(g)))
            .Where(a => a != null)
            .ToArray();

        ClothingItemData[] bodyTypes   = Filter(allItems, ClothingCategory.BodyBase);
        ClothingItemData[] eyes        = Filter(allItems, ClothingCategory.Eyes);
        ClothingItemData[] eyebrows    = Filter(allItems, ClothingCategory.Eyebrows);
        ClothingItemData[] mouths      = Filter(allItems, ClothingCategory.Mouth);
        ClothingItemData[] ears        = Filter(allItems, ClothingCategory.Ears);
        ClothingItemData[] noses       = Filter(allItems, ClothingCategory.Nose);

        Debug.Log($"[Fix] Loaded — BodyBase:{bodyTypes.Length} Eyes:{eyes.Length} Eyebrows:{eyebrows.Length} Mouths:{mouths.Length} Ears:{ears.Length} Noses:{noses.Length}");

        // ── 4. Assign arrays to CharacterCreator ──────────────────────────────────
        GameObject creatorGO = GameObject.Find("[CharacterCreatorManager]");
        CharacterCreator creator = creatorGO?.GetComponent<CharacterCreator>();
        if (creator != null)
        {
            SerializedObject so = new SerializedObject(creator);
            SetArray(so, "bodyTypes",  bodyTypes);
            SetArray(so, "eyes",       eyes);
            SetArray(so, "eyebrows",   eyebrows);
            SetArray(so, "mouths",     mouths);
            SetArray(so, "ears",       ears);
            SetArray(so, "noses",      noses);
            // No FrontHair/BackHair assets yet — leave empty
            so.ApplyModifiedProperties();
            Debug.Log("[Fix] Assigned arrays to CharacterCreator.");
        }
        else Debug.LogError("[Fix] CharacterCreator not found.");

        // ── 5. Assign arrays to CharacterDisplay ──────────────────────────────────
        CharacterDisplay display = charRoot?.GetComponent<CharacterDisplay>();
        if (display != null)
        {
            SerializedObject so = new SerializedObject(display);
            SetArray(so, "bodyTypes",  bodyTypes);
            SetArray(so, "eyes",       eyes);
            SetArray(so, "eyebrows",   eyebrows);
            SetArray(so, "mouths",     mouths);
            SetArray(so, "ears",       ears);
            SetArray(so, "noses",      noses);
            so.ApplyModifiedProperties();
            Debug.Log("[Fix] Assigned arrays to CharacterDisplay.");
        }
        else Debug.LogError("[Fix] CharacterDisplay not found.");

        // ── 6. Rebuild OptionsContent with feature rows ───────────────────────────
        GameObject contentGO = GameObject.Find("[OptionsContent]");
        if (contentGO == null) { Debug.LogError("[Fix] [OptionsContent] not found."); return; }

        // Clear existing children
        for (int i = contentGO.transform.childCount - 1; i >= 0; i--)
            GameObject.DestroyImmediate(contentGO.transform.GetChild(i).gameObject);

        // Section header colours
        Color headerBg    = new Color(0.12f, 0.12f, 0.16f, 1f);
        Color rowBg       = new Color(0.08f, 0.08f, 0.12f, 0.85f);
        Color btnColor    = new Color(0.95f, 0.75f, 0.2f,  1f);
        Color btnTextCol  = new Color(0.1f,  0.1f,  0.1f,  1f);
        Color labelCol    = Color.white;

        // Define rows: (label, prevMethodName, nextMethodName, hasAssets)
        var rows = new (string label, string prev, string next, bool has)[]
        {
            ("Skin Tone", "PrevBodyType",  "NextBodyType",  bodyTypes.Length  > 0),
            ("Eyes",      "PrevEyes",      "NextEyes",      eyes.Length       > 0),
            ("Eyebrows",  "PrevEyebrows",  "NextEyebrows",  eyebrows.Length   > 0),
            ("Mouth",     "PrevMouth",     "NextMouth",     mouths.Length     > 0),
            ("Ears",      "PrevEars",      "NextEars",      ears.Length       > 0),
            ("Nose",      "PrevNose",      "NextNose",      noses.Length      > 0),
        };

        AddSectionHeader(contentGO.transform, "CHARACTER", headerBg);

        foreach (var row in rows)
        {
            if (!row.has) continue;
            AddFeatureRow(contentGO.transform, row.label, creator, row.prev, row.next,
                          rowBg, btnColor, btnTextCol, labelCol);
        }

        Debug.Log("[Fix] Feature rows built.");

        // ── 7. Fix the OptionsPanel background (it was showing white from Mask) ───
        // The Viewport needs a transparent image for the Mask to work correctly
        GameObject viewport = GameObject.Find("[OptionsViewport]");
        if (viewport != null)
        {
            Image vpImg = viewport.GetComponent<Image>();
            if (vpImg != null) vpImg.color = new Color(0f, 0f, 0f, 0.01f); // nearly transparent
        }

        // ── 8. Re-title the title label ───────────────────────────────────────────
        GameObject titleGO = GameObject.Find("[TitleLabel]");
        if (titleGO != null)
        {
            var tmp = titleGO.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = "Create Your Look";
                tmp.fontSize = 48f;
                tmp.fontStyle = FontStyles.Bold;
            }
            // Shift title to the left so it's over the character area
            RectTransform rt = titleGO.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin        = new Vector2(0f, 1f);
                rt.anchorMax        = new Vector2(0.53f, 1f);
                rt.pivot            = new Vector2(0.5f, 1f);
                rt.anchoredPosition = new Vector2(0f, -30f);
                rt.sizeDelta        = new Vector2(0f, 70f);
            }
        }

        // ── 9. Mark scene dirty and save ─────────────────────────────────────────
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[Fix] Character Creation scene fix complete.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────────

    private static ClothingItemData[] Filter(ClothingItemData[] all, ClothingCategory cat)
        => all.Where(a => a.Category == cat).OrderBy(a => a.name).ToArray();

    private static void SetArray(SerializedObject so, string propName, ClothingItemData[] items)
    {
        SerializedProperty prop = so.FindProperty(propName);
        if (prop == null) { Debug.LogWarning($"[Fix] Property '{propName}' not found."); return; }
        prop.arraySize = items.Length;
        for (int i = 0; i < items.Length; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
    }

    private static void AddSectionHeader(Transform parent, string text, Color bg)
    {
        GameObject go = new GameObject("[Header_" + text + "]", typeof(RectTransform));
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0f, 36f);

        Image img = go.AddComponent<Image>();
        img.color = bg;

        GameObject labelGO = new GameObject("Label", typeof(RectTransform));
        labelGO.transform.SetParent(go.transform, false);
        RectTransform lrt = labelGO.GetComponent<RectTransform>();
        lrt.anchorMin  = Vector2.zero;
        lrt.anchorMax  = Vector2.one;
        lrt.offsetMin  = new Vector2(12f, 0f);
        lrt.offsetMax  = Vector2.zero;
        lrt.sizeDelta  = Vector2.zero;

        TextMeshProUGUI tmp = labelGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = 13f;
        tmp.color     = new Color(0.7f, 0.7f, 0.8f, 1f);
        tmp.fontStyle = FontStyles.Bold | FontStyles.UpperCase;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
    }

    private static void AddFeatureRow(Transform parent, string featureName,
        CharacterCreator creator, string prevMethodName, string nextMethodName,
        Color rowBg, Color btnColor, Color btnTextCol, Color labelCol)
    {
        // Row container
        GameObject rowGO = new GameObject("[Row_" + featureName + "]", typeof(RectTransform));
        rowGO.transform.SetParent(parent, false);

        RectTransform rowRT = rowGO.GetComponent<RectTransform>();
        rowRT.sizeDelta = new Vector2(0f, 72f);

        Image rowImg = rowGO.AddComponent<Image>();
        rowImg.color = rowBg;

        HorizontalLayoutGroup hlg = rowGO.AddComponent<HorizontalLayoutGroup>();
        hlg.padding            = new RectOffset(12, 12, 10, 10);
        hlg.spacing            = 8f;
        hlg.childAlignment     = TextAnchor.MiddleCenter;
        hlg.childControlHeight = true;
        hlg.childControlWidth  = false;
        hlg.childForceExpandHeight = true;
        hlg.childForceExpandWidth  = false;

        // Feature label
        GameObject labelGO = new GameObject("Label", typeof(RectTransform));
        labelGO.transform.SetParent(rowGO.transform, false);
        LayoutElement labelLE = labelGO.AddComponent<LayoutElement>();
        labelLE.preferredWidth  = 120f;
        labelLE.flexibleWidth   = 1f;
        TextMeshProUGUI labelTMP = labelGO.AddComponent<TextMeshProUGUI>();
        labelTMP.text      = featureName;
        labelTMP.fontSize  = 18f;
        labelTMP.color     = labelCol;
        labelTMP.fontStyle = FontStyles.Bold;
        labelTMP.alignment = TextAlignmentOptions.MidlineLeft;

        // Prev / Next buttons wired via reflection to get proper Unity persistent listeners
        Button prevBtn = CreateNavButton(rowGO.transform, "BtnPrev", "<", btnColor, btnTextCol);
        Button nextBtn = CreateNavButton(rowGO.transform, "BtnNext", ">", btnColor, btnTextCol);

        WireButton(prevBtn, creator, prevMethodName);
        WireButton(nextBtn, creator, nextMethodName);
    }

    private static void WireButton(Button btn, CharacterCreator creator, string methodName)
    {
        var method = typeof(CharacterCreator).GetMethod(methodName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (method == null) { Debug.LogError($"[Fix] Method '{methodName}' not found."); return; }
        var action = (UnityAction)System.Delegate.CreateDelegate(typeof(UnityAction), creator, method);
        UnityEventTools.AddVoidPersistentListener(btn.onClick, action);
    }

    private static Button CreateNavButton(Transform parent, string goName, string label,
                                          Color bgColor, Color textColor)
    {
        GameObject go = new GameObject(goName, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredWidth  = 52f;
        le.preferredHeight = 52f;
        le.minWidth        = 52f;

        Image img = go.AddComponent<Image>();
        img.color = bgColor;

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        ColorBlock cb = btn.colors;
        cb.normalColor      = bgColor;
        cb.highlightedColor = bgColor * 1.15f;
        cb.pressedColor     = bgColor * 0.85f;
        cb.selectedColor    = bgColor;
        cb.fadeDuration     = 0.1f;
        btn.colors          = cb;

        // Label child
        GameObject labelGO = new GameObject("Label", typeof(RectTransform));
        labelGO.transform.SetParent(go.transform, false);
        RectTransform lrt = labelGO.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero;
        lrt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = labelGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 20f;
        tmp.color     = textColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;

        return btn;
    }
}
