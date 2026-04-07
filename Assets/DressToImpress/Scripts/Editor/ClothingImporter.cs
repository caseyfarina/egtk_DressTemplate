using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>
/// EditorWindow that auto-generates <see cref="ClothingItemData"/> ScriptableObject assets
/// from PNG sprites found under <c>Assets/Art/</c>.
///
/// <para>Open via <b>Dress To Impress &gt; Import Clothing Assets</b>.</para>
///
/// <para>
/// Expected filename pattern: <c>{itemName}_color{N}_x{X}_y{Y}.png</c><br/>
/// The folder path determines the <see cref="ClothingCategory"/>.<br/>
/// Output assets are written to <c>Assets/DressToImpress/Data/{Category}/</c>.
/// </para>
///
/// <para>
/// On <b>update</b> (asset already exists) only Sprite, CanvasX, CanvasY,
/// ColorVariantIndex, GroupName, and Category are overwritten.
/// StyleScore and ThemeTags are left untouched so instructor tuning is preserved.
/// </para>
/// </summary>
public class ClothingImporter : EditorWindow
{
    // ── Constants ─────────────────────────────────────────────────────────────

    private const string ArtSearchFolder  = "Assets/Art";
    private const string OutputRootFolder = "Assets/DressToImpress/Data";

    /// <summary>
    /// Regex applied to the filename (without directory) to extract item metadata.
    /// Groups: 1 = itemName, 2 = colorVariant, 3 = canvasX, 4 = canvasY.
    /// </summary>
    private static readonly Regex FilenamePattern =
        new Regex(@"^(.+)_color(\d+)_x(\d+)_y(\d+)\.png$", RegexOptions.IgnoreCase);

    // ── State ─────────────────────────────────────────────────────────────────

    private readonly List<string> _log     = new List<string>();
    private string                _summary = string.Empty;
    private Vector2               _scrollPos;

    // ── MenuItem ──────────────────────────────────────────────────────────────

    /// <summary>Opens the Clothing Importer window from the Unity menu bar.</summary>
    [MenuItem("Dress To Impress/Import Clothing Assets")]
    public static void ShowWindow()
    {
        var window = GetWindow<ClothingImporter>("Clothing Importer");
        window.titleContent = new GUIContent("Clothing Importer");
        window.minSize      = new Vector2(480f, 360f);
        window.Show();
    }

    // ── GUI ───────────────────────────────────────────────────────────────────

    private void OnGUI()
    {
        GUILayout.Space(8f);
        EditorGUILayout.LabelField(
            "Dress To Impress — Clothing Importer",
            EditorStyles.boldLabel);

        GUILayout.Space(4f);
        EditorGUILayout.HelpBox(
            "Scans Assets/Art/ for PNGs matching *_color*_x*_y*.png and creates ClothingItemData assets.",
            MessageType.Info);

        GUILayout.Space(8f);

        if (GUILayout.Button("Scan & Import All", GUILayout.Height(30f)))
        {
            RunImport();
        }

        GUILayout.Space(8f);

        // ── Log scroll area ──────────────────────────────────────────────────
        EditorGUILayout.LabelField("Log", EditorStyles.boldLabel);
        _scrollPos = EditorGUILayout.BeginScrollView(
            _scrollPos,
            GUILayout.ExpandHeight(true));

        foreach (string line in _log)
        {
            GUILayout.Label(line, EditorStyles.wordWrappedLabel);
        }

        EditorGUILayout.EndScrollView();

        // ── Summary ──────────────────────────────────────────────────────────
        if (!string.IsNullOrEmpty(_summary))
        {
            GUILayout.Space(4f);
            EditorGUILayout.LabelField(_summary, EditorStyles.boldLabel);
        }

        GUILayout.Space(4f);
    }

    // ── Import logic ──────────────────────────────────────────────────────────

    /// <summary>
    /// Scans <c>Assets/Art/</c> for all Texture2D assets, filters by filename
    /// pattern, resolves the <see cref="ClothingCategory"/> from the folder path,
    /// then creates or updates a <see cref="ClothingItemData"/> asset for each match.
    /// </summary>
    public void RunImport()
    {
        _log.Clear();
        _summary = string.Empty;

        // Guard: art folder must exist
        if (!AssetDatabase.IsValidFolder(ArtSearchFolder))
        {
            _log.Add($"[ERROR] Folder '{ArtSearchFolder}' does not exist. " +
                     "Create it and place your PNG sprites there first.");
            _summary = "Import aborted — Assets/Art/ not found.";
            Repaint();
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Texture2D",
            new[] { ArtSearchFolder });

        if (guids.Length == 0)
        {
            _log.Add($"[INFO] No Texture2D assets found under '{ArtSearchFolder}'.");
            _summary = "Nothing to import.";
            Repaint();
            return;
        }

        int created = 0, updated = 0, skipped = 0;

        try
        {
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);

                // Progress bar
                EditorUtility.DisplayProgressBar(
                    "Clothing Importer",
                    $"Processing {Path.GetFileName(assetPath)} ({i + 1}/{guids.Length})",
                    (float)(i + 1) / guids.Length);

                // Must be a PNG
                if (!assetPath.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
                {
                    skipped++;
                    _log.Add($"[SKIP] Not a PNG: {assetPath}");
                    continue;
                }

                string filename = Path.GetFileName(assetPath);
                Match  match    = FilenamePattern.Match(filename);

                if (!match.Success)
                {
                    skipped++;
                    _log.Add($"[SKIP] Filename does not match pattern: {filename}");
                    continue;
                }

                string itemName     = match.Groups[1].Value;
                int    colorVariant = int.Parse(match.Groups[2].Value);
                int    canvasX      = int.Parse(match.Groups[3].Value);
                int    canvasY      = int.Parse(match.Groups[4].Value);

                // Resolve category from folder path
                if (!TryGetCategory(assetPath, out ClothingCategory category))
                {
                    skipped++;
                    _log.Add($"[WARN] No category mapping for path: {assetPath}");
                    continue;
                }

                // Load sprite
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (sprite == null)
                {
                    skipped++;
                    _log.Add($"[WARN] Could not load Sprite at: {assetPath}  " +
                             "(check Import Settings — Texture Type must be Sprite)");
                    continue;
                }

                // Build output path
                string categoryName = category.ToString();
                string outputDir    = $"{OutputRootFolder}/{categoryName}";
                string outputPath   = $"{outputDir}/{itemName}_color{colorVariant}.asset";

                // Ensure output directory exists
                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                    AssetDatabase.Refresh();
                }

                ClothingItemData existing =
                    AssetDatabase.LoadAssetAtPath<ClothingItemData>(outputPath);

                if (existing != null)
                {
                    // Update — do NOT touch styleScore or themeTags
                    ApplyFields(existing, itemName, sprite, category,
                                canvasX, canvasY, colorVariant, isNew: false);
                    EditorUtility.SetDirty(existing);
                    updated++;
                    _log.Add($"[UPDATED] {outputPath}");
                }
                else
                {
                    // Create
                    ClothingItemData asset =
                        ScriptableObject.CreateInstance<ClothingItemData>();
                    ApplyFields(asset, itemName, sprite, category,
                                canvasX, canvasY, colorVariant, isNew: true);
                    AssetDatabase.CreateAsset(asset, outputPath);
                    created++;
                    _log.Add($"[CREATED] {outputPath}");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        _summary = $"Import complete — Created: {created}, Updated: {updated}, Skipped: {skipped}";
        _log.Add(string.Empty);
        _log.Add(_summary);
        Repaint();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Sets fields on a <see cref="ClothingItemData"/> asset via
    /// <see cref="SerializedObject"/> so private <c>[SerializeField]</c> backing
    /// fields are written correctly.
    /// </summary>
    /// <param name="asset">The target asset instance.</param>
    /// <param name="itemName">Parsed item base name (no color suffix).</param>
    /// <param name="sprite">The sprite loaded from the PNG.</param>
    /// <param name="category">Resolved clothing category.</param>
    /// <param name="canvasX">Parsed X canvas coordinate.</param>
    /// <param name="canvasY">Parsed Y canvas coordinate.</param>
    /// <param name="colorVariant">Parsed color variant index.</param>
    /// <param name="isNew">
    /// When <c>true</c>, styleScore and themeTags are also initialised to defaults.
    /// When <c>false</c>, those fields are left untouched.
    /// </param>
    private static void ApplyFields(
        ClothingItemData asset,
        string           itemName,
        Sprite           sprite,
        ClothingCategory category,
        int              canvasX,
        int              canvasY,
        int              colorVariant,
        bool             isNew)
    {
        var so = new SerializedObject(asset);

        so.FindProperty("itemName").stringValue            = itemName;
        so.FindProperty("sprite").objectReferenceValue     = sprite;
        so.FindProperty("category").enumValueIndex         = (int)category;
        so.FindProperty("canvasX").intValue                = canvasX;
        so.FindProperty("canvasY").intValue                = canvasY;
        so.FindProperty("colorVariantIndex").intValue      = colorVariant;
        so.FindProperty("groupName").stringValue           = itemName;

        if (isNew)
        {
            // Leave styleScore at its inspector default (10) and themeTags empty —
            // they are already at default on a freshly created ScriptableObject.
            // Explicitly writing them here would overwrite any future default change,
            // so we intentionally omit them for new assets as well and rely on the
            // [SerializeField] initialiser values in ClothingItemData.
        }

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary>
    /// Maps a Unity asset path to the appropriate <see cref="ClothingCategory"/>
    /// by checking whether the path contains one of the known folder segments.
    /// </summary>
    /// <param name="assetPath">Full Unity asset path (forward slashes).</param>
    /// <param name="category">The resolved category, if a match was found.</param>
    /// <returns><c>true</c> if a matching folder segment was found; otherwise <c>false</c>.</returns>
    private static bool TryGetCategory(string assetPath, out ClothingCategory category)
    {
        // Normalise to forward slashes just in case
        string path = assetPath.Replace('\\', '/');

        if (path.Contains("Art/Clothing/Outerwear"))      { category = ClothingCategory.Outerwear;    return true; }
        if (path.Contains("Art/Clothing/Tops"))           { category = ClothingCategory.Top;          return true; }
        if (path.Contains("Art/Clothing/Bottoms"))        { category = ClothingCategory.Bottom;       return true; }
        if (path.Contains("Art/Clothing/Skirts"))         { category = ClothingCategory.Skirt;        return true; }
        if (path.Contains("Art/Clothing/Dresses"))        { category = ClothingCategory.Dress;        return true; }
        if (path.Contains("Art/Clothing/Shoes"))          { category = ClothingCategory.Shoes;        return true; }
        if (path.Contains("Art/Clothing/SocksLeggings"))  { category = ClothingCategory.SocksLeggings;return true; }
        if (path.Contains("Art/Clothing/Accessories"))    { category = ClothingCategory.Accessory;    return true; }
        if (path.Contains("Art/Clothing/Hats"))           { category = ClothingCategory.Hat;          return true; }
        if (path.Contains("Art/Hair/Front"))              { category = ClothingCategory.FrontHair;    return true; }
        if (path.Contains("Art/Hair/Back"))               { category = ClothingCategory.BackHair;     return true; }
        if (path.Contains("Art/FacialFeatures/Eyebrows")) { category = ClothingCategory.Eyebrows;     return true; }
        if (path.Contains("Art/FacialFeatures/Eyes"))     { category = ClothingCategory.Eyes;         return true; }
        if (path.Contains("Art/FacialFeatures/Ears"))     { category = ClothingCategory.Ears;         return true; }
        if (path.Contains("Art/FacialFeatures/Mouths"))   { category = ClothingCategory.Mouth;        return true; }
        if (path.Contains("Art/FacialFeatures/Nose"))     { category = ClothingCategory.Nose;         return true; }
        if (path.Contains("Art/BodyTypes"))               { category = ClothingCategory.BodyBase;     return true; }

        category = default;
        return false;
    }
}
