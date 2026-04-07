using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// Menu item: "Dress To Impress / Setup Scene — Character Creation"
/// Builds the full Character Creation scene hierarchy in the currently open scene,
/// including a camera, EventSystem, CharacterRoot with CharacterDisplay, a scrollable
/// options panel inside a Canvas, and a CharacterCreator manager. All cross-references
/// are wired automatically. Open an empty scene before running this tool.
/// </summary>
public static class SceneSetup_CharacterCreation
{
    [MenuItem("Dress To Impress/Setup Scene \u2014 Character Creation")]
    private static void BuildScene()
    {
        // ── Camera ────────────────────────────────────────────────────────────────
        GameObject cameraGO = new GameObject("[Camera]");
        Undo.RegisterCreatedObjectUndo(cameraGO, "Create Character Creation Scene");

        Camera cam = cameraGO.AddComponent<Camera>();
        cam.orthographic     = true;
        cam.orthographicSize = 10.24f;
        cam.clearFlags       = CameraClearFlags.SolidColor;
        cam.backgroundColor  = new Color(0.08f, 0.08f, 0.1f, 1f);
        cameraGO.transform.position = new Vector3(0f, 0f, -10f);
        cameraGO.tag = "MainCamera";

        // ── EventSystem ───────────────────────────────────────────────────────────
        GameObject eventSystemGO = new GameObject("[EventSystem]");
        Undo.RegisterCreatedObjectUndo(eventSystemGO, "Create Character Creation Scene");
        eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // ── CharacterRoot ─────────────────────────────────────────────────────────
        GameObject characterRootGO = new GameObject("[CharacterRoot]");
        Undo.RegisterCreatedObjectUndo(characterRootGO, "Create Character Creation Scene");
        characterRootGO.transform.position = Vector3.zero;
        CharacterDisplay characterDisplay = characterRootGO.AddComponent<CharacterDisplay>();

        // ── UI Canvas ─────────────────────────────────────────────────────────────
        GameObject canvasGO = new GameObject("[UI_Canvas]");
        Undo.RegisterCreatedObjectUndo(canvasGO, "Create Character Creation Scene");

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight  = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // ── TitleLabel ────────────────────────────────────────────────────────────
        GameObject titleLabelGO = CreateUIObject("[TitleLabel]", canvasGO.transform);
        RectTransform titleRT = titleLabelGO.GetComponent<RectTransform>();
        // Anchored top-centre
        titleRT.anchorMin        = new Vector2(0.5f, 1f);
        titleRT.anchorMax        = new Vector2(0.5f, 1f);
        titleRT.pivot            = new Vector2(0.5f, 1f);
        titleRT.anchoredPosition = new Vector2(0f, -60f);
        titleRT.sizeDelta        = new Vector2(600f, 80f);

        TextMeshProUGUI titleTMP = titleLabelGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text      = "Create Your Look";
        titleTMP.fontSize  = 52f;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color     = Color.white;

        // ── OptionsPanel ──────────────────────────────────────────────────────────
        GameObject optionsPanelGO = CreateUIObject("[OptionsPanel]", canvasGO.transform);
        RectTransform optionsPanelRT = optionsPanelGO.GetComponent<RectTransform>();
        optionsPanelRT.anchorMin  = new Vector2(0.55f, 0.05f);
        optionsPanelRT.anchorMax  = new Vector2(0.98f, 0.95f);
        optionsPanelRT.offsetMin  = Vector2.zero;
        optionsPanelRT.offsetMax  = Vector2.zero;

        Image optionsPanelImg = optionsPanelGO.AddComponent<Image>();
        optionsPanelImg.color = new Color(0f, 0f, 0f, 0.4f);

        ScrollRect optionsScrollRect = optionsPanelGO.AddComponent<ScrollRect>();
        optionsScrollRect.horizontal    = false;
        optionsScrollRect.vertical      = true;
        optionsScrollRect.movementType  = ScrollRect.MovementType.Clamped;

        // ── OptionsViewport ───────────────────────────────────────────────────────
        GameObject optionsViewportGO = CreateUIObject("[OptionsViewport]", optionsPanelGO.transform);
        RectTransform optionsViewportRT = optionsViewportGO.GetComponent<RectTransform>();
        SetStretchFull(optionsViewportRT);

        optionsViewportGO.AddComponent<Mask>();
        optionsViewportGO.AddComponent<Image>(); // required source image for Mask

        // ── OptionsContent ────────────────────────────────────────────────────────
        GameObject optionsContentGO = CreateUIObject("[OptionsContent]", optionsViewportGO.transform);
        RectTransform optionsContentRT = optionsContentGO.GetComponent<RectTransform>();
        optionsContentRT.anchorMin  = new Vector2(0f, 1f);
        optionsContentRT.anchorMax  = new Vector2(1f, 1f);
        optionsContentRT.pivot      = new Vector2(0.5f, 1f);
        optionsContentRT.sizeDelta  = Vector2.zero;

        VerticalLayoutGroup vlg = optionsContentGO.AddComponent<VerticalLayoutGroup>();
        vlg.spacing               = 8f;
        vlg.padding               = new RectOffset(10, 10, 10, 10);
        vlg.childControlWidth     = true;
        vlg.childControlHeight    = false;
        vlg.childForceExpandWidth = true;

        ContentSizeFitter csf = optionsContentGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // ── Wire ScrollRect ───────────────────────────────────────────────────────
        optionsScrollRect.viewport = optionsViewportRT;
        optionsScrollRect.content  = optionsContentRT;

        // ── ButtonStartStyling ────────────────────────────────────────────────────
        GameObject buttonStartGO = CreateUIObject("[ButtonStartStyling]", canvasGO.transform);
        RectTransform buttonStartRT = buttonStartGO.GetComponent<RectTransform>();
        // Anchored bottom-right
        buttonStartRT.anchorMin        = new Vector2(1f, 0f);
        buttonStartRT.anchorMax        = new Vector2(1f, 0f);
        buttonStartRT.pivot            = new Vector2(1f, 0f);
        buttonStartRT.anchoredPosition = new Vector2(-60f, 60f);
        buttonStartRT.sizeDelta        = new Vector2(260f, 70f);

        Image buttonStartImg = buttonStartGO.AddComponent<Image>();
        buttonStartImg.color = new Color(0.95f, 0.75f, 0.2f, 1f);

        Button buttonStartBtn = buttonStartGO.AddComponent<Button>();
        // Set the button's target graphic to the image we added
        buttonStartBtn.targetGraphic = buttonStartImg;

        // ── ButtonStartStyling / Label ────────────────────────────────────────────
        GameObject buttonLabelGO = CreateUIObject("[Label]", buttonStartGO.transform);
        RectTransform buttonLabelRT = buttonLabelGO.GetComponent<RectTransform>();
        SetStretchFull(buttonLabelRT);

        TextMeshProUGUI buttonLabelTMP = buttonLabelGO.AddComponent<TextMeshProUGUI>();
        buttonLabelTMP.text      = "START STYLING";
        buttonLabelTMP.fontSize  = 28f;
        buttonLabelTMP.alignment = TextAlignmentOptions.Center;
        buttonLabelTMP.color     = new Color(0.1f, 0.1f, 0.1f, 1f);
        buttonLabelTMP.fontStyle = FontStyles.Bold;

        // ── CharacterCreatorManager ───────────────────────────────────────────────
        GameObject creatorManagerGO = new GameObject("[CharacterCreatorManager]");
        Undo.RegisterCreatedObjectUndo(creatorManagerGO, "Create Character Creation Scene");
        CharacterCreator characterCreator = creatorManagerGO.AddComponent<CharacterCreator>();

        // ── Wire references ───────────────────────────────────────────────────────
        SerializedObject creatorSO = new SerializedObject(characterCreator);
        creatorSO.FindProperty("characterDisplay").objectReferenceValue = characterDisplay;
        creatorSO.ApplyModifiedPropertiesWithoutUndo();

        // Wire ButtonStartStyling.onClick → CharacterCreator.OnStartStyling()
        UnityEventTools.AddPersistentListener(buttonStartBtn.onClick, characterCreator.OnStartStyling);

        // ── Mark scene dirty ──────────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        // Select the first object so the user can see what was built in the Hierarchy
        Selection.activeGameObject = cameraGO;

        Debug.Log("[DressToImpress] Character Creation scene setup complete. Assign ClothingItemData arrays to CharacterCreator in the Inspector.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a UI GameObject (with RectTransform) parented to <paramref name="parent"/>.
    /// Registers the object with Undo.
    /// </summary>
    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Create Character Creation Scene");
        go.transform.SetParent(parent, false);
        return go;
    }

    /// <summary>
    /// Sets a RectTransform to stretch across its entire parent
    /// (anchorMin = 0,0 / anchorMax = 1,1 / offsets = 0).
    /// </summary>
    private static void SetStretchFull(RectTransform rt)
    {
        rt.anchorMin  = Vector2.zero;
        rt.anchorMax  = Vector2.one;
        rt.offsetMin  = Vector2.zero;
        rt.offsetMax  = Vector2.zero;
        rt.sizeDelta  = Vector2.zero;
    }
}
