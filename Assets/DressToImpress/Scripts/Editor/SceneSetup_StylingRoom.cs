using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// Menu item: "Dress To Impress / Setup Scene — Styling Room"
/// Builds the full Styling Room scene hierarchy in the currently open scene, including
/// a camera, EventSystem, CharacterRoot with CharacterDisplay, manager GameObjects
/// (StylingRoomManager, JudgeManager, OutfitScorer), a top bar with money/boutique/next
/// buttons, a judge info panel, a scrollable clothing selection panel, and a submit
/// button. All cross-references are wired automatically. Two prefabs (TabButton and
/// ItemButton) are created in Assets/DressToImpress/Prefabs/ and assigned to
/// ClothingPanelManager. Open an empty scene before running this tool.
/// </summary>
public static class SceneSetup_StylingRoom
{
    [MenuItem("Dress To Impress/Setup Scene \u2014 Styling Room")]
    private static void BuildScene()
    {
        // ── Camera ────────────────────────────────────────────────────────────────
        GameObject cameraGO = new GameObject("[Camera]");
        Undo.RegisterCreatedObjectUndo(cameraGO, "Create Styling Room Scene");

        Camera cam = cameraGO.AddComponent<Camera>();
        cam.orthographic     = true;
        cam.orthographicSize = 5.5f;
        cam.clearFlags       = CameraClearFlags.SolidColor;
        cam.backgroundColor  = new Color(0.08f, 0.08f, 0.1f, 1f);
        cameraGO.transform.position = new Vector3(0f, 0f, -10f);
        cameraGO.tag = "MainCamera";

        // ── EventSystem ───────────────────────────────────────────────────────────
        GameObject eventSystemGO = new GameObject("[EventSystem]");
        Undo.RegisterCreatedObjectUndo(eventSystemGO, "Create Styling Room Scene");
        eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // ── CharacterRoot ─────────────────────────────────────────────────────────
        GameObject characterRootGO = new GameObject("[CharacterRoot]");
        Undo.RegisterCreatedObjectUndo(characterRootGO, "Create Styling Room Scene");
        characterRootGO.transform.position = Vector3.zero;
        CharacterDisplay characterDisplay = characterRootGO.AddComponent<CharacterDisplay>();

        // ── Managers parent ───────────────────────────────────────────────────────
        GameObject managersGO = new GameObject("[Managers]");
        Undo.RegisterCreatedObjectUndo(managersGO, "Create Styling Room Scene");

        // StylingRoomManager
        GameObject stylingRoomManagerGO = new GameObject("[StylingRoomManager_GO]");
        Undo.RegisterCreatedObjectUndo(stylingRoomManagerGO, "Create Styling Room Scene");
        stylingRoomManagerGO.transform.SetParent(managersGO.transform, false);
        StylingRoomManager stylingRoomManager = stylingRoomManagerGO.AddComponent<StylingRoomManager>();

        // JudgeManager
        GameObject judgeManagerGO = new GameObject("[JudgeManager_GO]");
        Undo.RegisterCreatedObjectUndo(judgeManagerGO, "Create Styling Room Scene");
        judgeManagerGO.transform.SetParent(managersGO.transform, false);
        JudgeManager judgeManager = judgeManagerGO.AddComponent<JudgeManager>();

        // OutfitScorer
        GameObject outfitScorerGO = new GameObject("[OutfitScorer_GO]");
        Undo.RegisterCreatedObjectUndo(outfitScorerGO, "Create Styling Room Scene");
        outfitScorerGO.transform.SetParent(managersGO.transform, false);
        OutfitScorer outfitScorer = outfitScorerGO.AddComponent<OutfitScorer>();

        // ── UI Canvas ─────────────────────────────────────────────────────────────
        GameObject canvasGO = new GameObject("[UI_Canvas]");
        Undo.RegisterCreatedObjectUndo(canvasGO, "Create Styling Room Scene");

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight  = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // ── TopBar ────────────────────────────────────────────────────────────────
        GameObject topBarGO = CreateUIObject("[TopBar]", canvasGO.transform);
        RectTransform topBarRT = topBarGO.GetComponent<RectTransform>();
        topBarRT.anchorMin        = new Vector2(0f, 1f);
        topBarRT.anchorMax        = new Vector2(1f, 1f);
        topBarRT.pivot            = new Vector2(0.5f, 1f);
        topBarRT.sizeDelta        = new Vector2(0f, 80f);
        topBarRT.anchoredPosition = new Vector2(0f, 0f);

        Image topBarImg = topBarGO.AddComponent<Image>();
        topBarImg.color = new Color(0f, 0f, 0f, 0.6f);

        // MoneyText
        GameObject moneyTextGO = CreateUIObject("[MoneyText]", topBarGO.transform);
        RectTransform moneyTextRT = moneyTextGO.GetComponent<RectTransform>();
        moneyTextRT.anchorMin        = new Vector2(0f, 0f);
        moneyTextRT.anchorMax        = new Vector2(0f, 1f);
        moneyTextRT.pivot            = new Vector2(0f, 0.5f);
        moneyTextRT.anchoredPosition = new Vector2(30f, 0f);
        moneyTextRT.sizeDelta        = new Vector2(200f, 0f);

        TextMeshProUGUI moneyTMP = moneyTextGO.AddComponent<TextMeshProUGUI>();
        moneyTMP.text      = "$0";
        moneyTMP.fontSize  = 36f;
        moneyTMP.color     = new Color(0.95f, 0.75f, 0.2f, 1f);
        moneyTMP.alignment = TextAlignmentOptions.MidlineLeft;

        // ButtonBoutique
        GameObject buttonBoutiqueGO = CreateUIObject("[ButtonBoutique]", topBarGO.transform);
        RectTransform buttonBoutiqueRT = buttonBoutiqueGO.GetComponent<RectTransform>();
        buttonBoutiqueRT.anchorMin        = new Vector2(1f, 0.5f);
        buttonBoutiqueRT.anchorMax        = new Vector2(1f, 0.5f);
        buttonBoutiqueRT.pivot            = new Vector2(1f, 0.5f);
        buttonBoutiqueRT.anchoredPosition = new Vector2(-20f, 0f);
        buttonBoutiqueRT.sizeDelta        = new Vector2(160f, 50f);

        Image boutiqueImg = buttonBoutiqueGO.AddComponent<Image>();
        boutiqueImg.color = new Color(0.3f, 0.3f, 0.35f, 1f);
        Button boutiqueBtn = buttonBoutiqueGO.AddComponent<Button>();
        boutiqueBtn.targetGraphic = boutiqueImg;

        GameObject boutiqueLabelGO = CreateUIObject("[Label]", buttonBoutiqueGO.transform);
        SetStretchFull(boutiqueLabelGO.GetComponent<RectTransform>());
        TextMeshProUGUI boutiqueLabelTMP = boutiqueLabelGO.AddComponent<TextMeshProUGUI>();
        boutiqueLabelTMP.text      = "BOUTIQUE";
        boutiqueLabelTMP.fontSize  = 22f;
        boutiqueLabelTMP.alignment = TextAlignmentOptions.Center;
        boutiqueLabelTMP.color     = Color.white;

        // ButtonNext
        GameObject buttonNextGO = CreateUIObject("[ButtonNext]", topBarGO.transform);
        RectTransform buttonNextRT = buttonNextGO.GetComponent<RectTransform>();
        buttonNextRT.anchorMin        = new Vector2(1f, 0.5f);
        buttonNextRT.anchorMax        = new Vector2(1f, 0.5f);
        buttonNextRT.pivot            = new Vector2(1f, 0.5f);
        buttonNextRT.anchoredPosition = new Vector2(-200f, 0f);
        buttonNextRT.sizeDelta        = new Vector2(160f, 50f);

        Image nextImg = buttonNextGO.AddComponent<Image>();
        nextImg.color = new Color(0.3f, 0.3f, 0.35f, 1f);
        Button nextBtn = buttonNextGO.AddComponent<Button>();
        nextBtn.targetGraphic = nextImg;

        GameObject nextLabelGO = CreateUIObject("[Label]", buttonNextGO.transform);
        SetStretchFull(nextLabelGO.GetComponent<RectTransform>());
        TextMeshProUGUI nextLabelTMP = nextLabelGO.AddComponent<TextMeshProUGUI>();
        nextLabelTMP.text      = "NEXT";
        nextLabelTMP.fontSize  = 22f;
        nextLabelTMP.alignment = TextAlignmentOptions.Center;
        nextLabelTMP.color     = Color.white;

        // ── JudgePanel ────────────────────────────────────────────────────────────
        GameObject judgePanelGO = CreateUIObject("[JudgePanel]", canvasGO.transform);
        RectTransform judgePanelRT = judgePanelGO.GetComponent<RectTransform>();
        judgePanelRT.anchorMin        = new Vector2(0f, 0.4f);
        judgePanelRT.anchorMax        = new Vector2(0f, 0.9f);
        judgePanelRT.pivot            = new Vector2(0f, 0.5f);
        judgePanelRT.anchoredPosition = new Vector2(20f, 0f);
        judgePanelRT.sizeDelta        = new Vector2(280f, 0f);

        Image judgePanelImg = judgePanelGO.AddComponent<Image>();
        judgePanelImg.color = new Color(0f, 0f, 0f, 0.5f);

        // AvatarImage
        GameObject avatarImageGO = CreateUIObject("[AvatarImage]", judgePanelGO.transform);
        RectTransform avatarRT = avatarImageGO.GetComponent<RectTransform>();
        avatarRT.anchorMin        = new Vector2(0.5f, 1f);
        avatarRT.anchorMax        = new Vector2(0.5f, 1f);
        avatarRT.pivot            = new Vector2(0.5f, 1f);
        avatarRT.anchoredPosition = new Vector2(0f, -20f);
        avatarRT.sizeDelta        = new Vector2(80f, 80f);

        Image avatarImg = avatarImageGO.AddComponent<Image>();
        avatarImg.color = new Color(0.4f, 0.4f, 0.5f, 1f);

        // JudgeNameText
        GameObject judgeNameTextGO = CreateUIObject("[JudgeNameText]", judgePanelGO.transform);
        RectTransform judgeNameRT = judgeNameTextGO.GetComponent<RectTransform>();
        judgeNameRT.anchorMin        = new Vector2(0.5f, 1f);
        judgeNameRT.anchorMax        = new Vector2(0.5f, 1f);
        judgeNameRT.pivot            = new Vector2(0.5f, 1f);
        judgeNameRT.anchoredPosition = new Vector2(0f, -120f);
        judgeNameRT.sizeDelta        = new Vector2(260f, 40f);

        TextMeshProUGUI judgeNameTMP = judgeNameTextGO.AddComponent<TextMeshProUGUI>();
        judgeNameTMP.fontSize  = 28f;
        judgeNameTMP.fontStyle = FontStyles.Bold;
        judgeNameTMP.color     = Color.white;
        judgeNameTMP.alignment = TextAlignmentOptions.Center;

        // StyleTagText
        GameObject styleTagTextGO = CreateUIObject("[StyleTagText]", judgePanelGO.transform);
        RectTransform styleTagRT = styleTagTextGO.GetComponent<RectTransform>();
        styleTagRT.anchorMin        = new Vector2(0.5f, 1f);
        styleTagRT.anchorMax        = new Vector2(0.5f, 1f);
        styleTagRT.pivot            = new Vector2(0.5f, 1f);
        styleTagRT.anchoredPosition = new Vector2(0f, -168f);
        styleTagRT.sizeDelta        = new Vector2(260f, 30f);

        TextMeshProUGUI styleTagTMP = styleTagTextGO.AddComponent<TextMeshProUGUI>();
        styleTagTMP.fontSize  = 20f;
        styleTagTMP.fontStyle = FontStyles.Italic;
        styleTagTMP.color     = new Color(0.9f, 0.75f, 0.4f, 1f);
        styleTagTMP.alignment = TextAlignmentOptions.Center;

        // PromptText
        GameObject promptTextGO = CreateUIObject("[PromptText]", judgePanelGO.transform);
        RectTransform promptRT = promptTextGO.GetComponent<RectTransform>();
        promptRT.anchorMin  = new Vector2(0f, 0f);
        promptRT.anchorMax  = new Vector2(1f, 1f);
        promptRT.offsetMin  = new Vector2(10f, 10f);
        promptRT.offsetMax  = new Vector2(-10f, -210f);

        TextMeshProUGUI promptTMP = promptTextGO.AddComponent<TextMeshProUGUI>();
        promptTMP.fontSize       = 18f;
        promptTMP.color          = Color.white;
        promptTMP.enableWordWrapping = true;
        promptTMP.alignment      = TextAlignmentOptions.TopLeft;

        // ── ClothingPanel ─────────────────────────────────────────────────────────
        GameObject clothingPanelGO = CreateUIObject("[ClothingPanel]", canvasGO.transform);
        RectTransform clothingPanelRT = clothingPanelGO.GetComponent<RectTransform>();
        clothingPanelRT.anchorMin        = new Vector2(0f, 0f);
        clothingPanelRT.anchorMax        = new Vector2(1f, 0f);
        clothingPanelRT.pivot            = new Vector2(0.5f, 0f);
        clothingPanelRT.anchoredPosition = new Vector2(0f, 0f);
        clothingPanelRT.sizeDelta        = new Vector2(0f, 220f);

        Image clothingPanelImg = clothingPanelGO.AddComponent<Image>();
        clothingPanelImg.color = new Color(0.08f, 0.08f, 0.1f, 0.95f);

        ClothingPanelManager clothingPanelManager = clothingPanelGO.AddComponent<ClothingPanelManager>();

        // TabBar
        GameObject tabBarGO = CreateUIObject("[TabBar]", clothingPanelGO.transform);
        RectTransform tabBarRT = tabBarGO.GetComponent<RectTransform>();
        tabBarRT.anchorMin        = new Vector2(0f, 1f);
        tabBarRT.anchorMax        = new Vector2(1f, 1f);
        tabBarRT.pivot            = new Vector2(0.5f, 1f);
        tabBarRT.anchoredPosition = new Vector2(0f, 0f);
        tabBarRT.sizeDelta        = new Vector2(0f, 44f);

        Image tabBarImg = tabBarGO.AddComponent<Image>();
        tabBarImg.color = new Color(0.05f, 0.05f, 0.08f, 1f);

        HorizontalLayoutGroup tabHLG = tabBarGO.AddComponent<HorizontalLayoutGroup>();
        tabHLG.spacing              = 4f;
        tabHLG.padding              = new RectOffset(8, 8, 0, 0);
        tabHLG.childControlWidth    = true;
        tabHLG.childForceExpandWidth = true;

        // ItemScrollView
        GameObject itemScrollViewGO = CreateUIObject("[ItemScrollView]", clothingPanelGO.transform);
        RectTransform itemScrollViewRT = itemScrollViewGO.GetComponent<RectTransform>();
        itemScrollViewRT.anchorMin  = new Vector2(0f, 0f);
        itemScrollViewRT.anchorMax  = new Vector2(1f, 1f);
        itemScrollViewRT.offsetMin  = new Vector2(0f, 0f);
        itemScrollViewRT.offsetMax  = new Vector2(0f, -44f);

        ScrollRect itemScrollRect = itemScrollViewGO.AddComponent<ScrollRect>();
        itemScrollRect.horizontal   = true;
        itemScrollRect.vertical     = false;
        itemScrollRect.movementType = ScrollRect.MovementType.Clamped;

        // ItemViewport
        GameObject itemViewportGO = CreateUIObject("[ItemViewport]", itemScrollViewGO.transform);
        RectTransform itemViewportRT = itemViewportGO.GetComponent<RectTransform>();
        SetStretchFull(itemViewportRT);

        itemViewportGO.AddComponent<Mask>();
        itemViewportGO.AddComponent<Image>(); // required by Mask

        // ItemContent
        GameObject itemContentGO = CreateUIObject("[ItemContent]", itemViewportGO.transform);
        RectTransform itemContentRT = itemContentGO.GetComponent<RectTransform>();
        itemContentRT.anchorMin  = new Vector2(0f, 0f);
        itemContentRT.anchorMax  = new Vector2(0f, 1f);
        itemContentRT.pivot      = new Vector2(0f, 0.5f);
        itemContentRT.sizeDelta  = Vector2.zero;

        HorizontalLayoutGroup itemHLG = itemContentGO.AddComponent<HorizontalLayoutGroup>();
        itemHLG.spacing               = 8f;
        itemHLG.padding               = new RectOffset(8, 8, 8, 8);
        itemHLG.childControlHeight    = true;
        itemHLG.childForceExpandHeight = true;

        ContentSizeFitter itemCSF = itemContentGO.AddComponent<ContentSizeFitter>();
        itemCSF.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Wire scroll rect internals
        itemScrollRect.viewport = itemViewportRT;
        itemScrollRect.content  = itemContentRT;

        // ── ButtonSubmit ──────────────────────────────────────────────────────────
        GameObject buttonSubmitGO = CreateUIObject("[ButtonSubmit]", canvasGO.transform);
        RectTransform buttonSubmitRT = buttonSubmitGO.GetComponent<RectTransform>();
        buttonSubmitRT.anchorMin        = new Vector2(1f, 0f);
        buttonSubmitRT.anchorMax        = new Vector2(1f, 0f);
        buttonSubmitRT.pivot            = new Vector2(1f, 0f);
        buttonSubmitRT.anchoredPosition = new Vector2(-20f, 240f);
        buttonSubmitRT.sizeDelta        = new Vector2(220f, 60f);

        Image submitImg = buttonSubmitGO.AddComponent<Image>();
        submitImg.color = new Color(0.95f, 0.75f, 0.2f, 1f);
        Button submitBtn = buttonSubmitGO.AddComponent<Button>();
        submitBtn.targetGraphic = submitImg;

        GameObject submitLabelGO = CreateUIObject("[Label]", buttonSubmitGO.transform);
        SetStretchFull(submitLabelGO.GetComponent<RectTransform>());
        TextMeshProUGUI submitLabelTMP = submitLabelGO.AddComponent<TextMeshProUGUI>();
        submitLabelTMP.text      = "SUBMIT OUTFIT";
        submitLabelTMP.fontSize  = 22f;
        submitLabelTMP.fontStyle = FontStyles.Bold;
        submitLabelTMP.alignment = TextAlignmentOptions.Center;
        submitLabelTMP.color     = new Color(0.1f, 0.1f, 0.1f, 1f);

        // ── Create prefabs ────────────────────────────────────────────────────────
        const string prefabFolder = "Assets/DressToImpress/Prefabs";
        EnsureFolder(prefabFolder);

        GameObject tabButtonPrefab  = CreateTabButtonPrefab(prefabFolder);
        GameObject itemButtonPrefab = CreateItemButtonPrefab(prefabFolder);

        // ── Wire all component references ─────────────────────────────────────────

        // StylingRoomManager
        SerializedObject srmSO = new SerializedObject(stylingRoomManager);
        srmSO.FindProperty("characterDisplay").objectReferenceValue = characterDisplay;
        srmSO.FindProperty("judgeManager").objectReferenceValue     = judgeManager;
        srmSO.FindProperty("outfitScorer").objectReferenceValue     = outfitScorer;
        srmSO.FindProperty("clothingPanel").objectReferenceValue    = clothingPanelManager;
        srmSO.ApplyModifiedPropertiesWithoutUndo();

        // OutfitScorer
        SerializedObject osSO = new SerializedObject(outfitScorer);
        osSO.FindProperty("characterDisplay").objectReferenceValue = characterDisplay;
        osSO.FindProperty("judgeManager").objectReferenceValue     = judgeManager;
        osSO.ApplyModifiedPropertiesWithoutUndo();

        // ClothingPanelManager
        SerializedObject cpmSO = new SerializedObject(clothingPanelManager);
        cpmSO.FindProperty("characterDisplay").objectReferenceValue  = characterDisplay;
        cpmSO.FindProperty("tabContainer").objectReferenceValue      = tabBarRT;
        cpmSO.FindProperty("itemContainer").objectReferenceValue     = itemContentRT;
        cpmSO.FindProperty("tabButtonPrefab").objectReferenceValue   = tabButtonPrefab;
        cpmSO.FindProperty("itemButtonPrefab").objectReferenceValue  = itemButtonPrefab;
        cpmSO.ApplyModifiedPropertiesWithoutUndo();

        // ── Wire button onClick events ─────────────────────────────────────────────
        UnityEventTools.AddPersistentListener(nextBtn.onClick,     stylingRoomManager.OnNextJudge);
        UnityEventTools.AddPersistentListener(boutiqueBtn.onClick, stylingRoomManager.OnGoToBoutique);
        UnityEventTools.AddPersistentListener(submitBtn.onClick,   stylingRoomManager.OnSubmitOutfit);

        // ── Mark scene dirty ──────────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        // Select first object to orient the user
        Selection.activeGameObject = cameraGO;

        Debug.Log(
            "[DressToImpress] Styling Room scene setup complete.\n" +
            "Manual wiring still required:\n" +
            "  • JudgeManager.onJudgeNameSet  → [JudgeNameText] TMP text field\n" +
            "  • JudgeManager.onStyleTagSet   → [StyleTagText] TMP text field\n" +
            "  • JudgeManager.onPromptSet     → [PromptText] TMP text field\n" +
            "  • JudgeManager.onAvatarSet     → [AvatarImage] UI Image\n" +
            "  • JudgeManager.onMoneyAwarded  → money display (e.g. GameCollectionManager.Increment)\n" +
            "  • Assign JudgeData ScriptableObjects to JudgeManager.judges array\n" +
            "  • Assign ClothingItemData assets to ClothingPanelManager.allClothingItems"
        );
    }

    // ── Prefab creation ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates (or replaces) the TabButton prefab at <paramref name="folder"/>.
    /// The prefab has a Button, Image (background), and a TextMeshProUGUI child named "Label".
    /// Returns the loaded prefab asset.
    /// </summary>
    private static GameObject CreateTabButtonPrefab(string folder)
    {
        string path = folder + "/TabButton.prefab";

        // Build the temporary source object
        GameObject root = new GameObject("TabButton");
        Image bg = root.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.2f, 0.25f, 1f);
        Button btn = root.AddComponent<Button>();
        btn.targetGraphic = bg;

        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(root.transform, false);
        RectTransform labelRT = labelGO.AddComponent<RectTransform>();
        SetStretchFull(labelRT);
        TextMeshProUGUI labelTMP = labelGO.AddComponent<TextMeshProUGUI>();
        labelTMP.text      = "TAB";
        labelTMP.fontSize  = 20f;
        labelTMP.alignment = TextAlignmentOptions.Center;
        labelTMP.color     = Color.white;

        GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefabAsset;
    }

    /// <summary>
    /// Creates (or replaces) the ItemButton prefab at <paramref name="folder"/>.
    /// The prefab has a Button, Image (background), a child "Icon" with an Image,
    /// and a TextMeshProUGUI child named "Label".
    /// Returns the loaded prefab asset.
    /// </summary>
    private static GameObject CreateItemButtonPrefab(string folder)
    {
        string path = folder + "/ItemButton.prefab";

        GameObject root = new GameObject("ItemButton");
        RectTransform rootRT = root.AddComponent<RectTransform>();
        rootRT.sizeDelta = new Vector2(120f, 140f);

        Image bg = root.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.2f, 1f);
        Button btn = root.AddComponent<Button>();
        btn.targetGraphic = bg;

        // Icon child
        GameObject iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(root.transform, false);
        RectTransform iconRT = iconGO.AddComponent<RectTransform>();
        iconRT.anchorMin        = new Vector2(0.1f, 0.25f);
        iconRT.anchorMax        = new Vector2(0.9f, 0.95f);
        iconRT.offsetMin        = Vector2.zero;
        iconRT.offsetMax        = Vector2.zero;
        iconGO.AddComponent<Image>(); // icon image — sprite set at runtime

        // Label child
        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(root.transform, false);
        RectTransform labelRT = labelGO.AddComponent<RectTransform>();
        labelRT.anchorMin        = new Vector2(0f, 0f);
        labelRT.anchorMax        = new Vector2(1f, 0.25f);
        labelRT.offsetMin        = Vector2.zero;
        labelRT.offsetMax        = Vector2.zero;
        TextMeshProUGUI labelTMP = labelGO.AddComponent<TextMeshProUGUI>();
        labelTMP.text      = "Item";
        labelTMP.fontSize  = 14f;
        labelTMP.alignment = TextAlignmentOptions.Center;
        labelTMP.color     = Color.white;

        GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefabAsset;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a UI GameObject (with RectTransform) parented to <paramref name="parent"/>.
    /// Registers the object with Undo.
    /// </summary>
    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Create Styling Room Scene");
        go.transform.SetParent(parent, false);
        return go;
    }

    /// <summary>
    /// Sets a RectTransform to stretch across its entire parent
    /// (anchorMin = 0,0 / anchorMax = 1,1 / offsets = 0).
    /// </summary>
    private static void SetStretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
    }

    /// <summary>
    /// Ensures the given asset folder path exists, creating any missing parts.
    /// </summary>
    private static void EnsureFolder(string folderPath)
    {
        // folderPath is like "Assets/DressToImpress/Prefabs"
        string[] parts = folderPath.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
