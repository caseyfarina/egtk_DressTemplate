using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages row-by-row character customisation in the Character Creation scene.
/// Players select body type, hair, and facial features. Selections are saved to
/// <see cref="CharacterProfile.Instance"/> and carried into the Styling Room scene.
/// Wire each row's prev/next buttons to the corresponding Select* method, and wire
/// the START STYLING button to <see cref="OnStartStyling"/>.
///
/// <para>
/// Available items are pulled from the central <see cref="ClothingDatabase"/> via
/// <c>characterDisplay.GetItemsByCategory(...)</c> — no per-category arrays to drag.
/// </para>
/// </summary>
public class CharacterCreator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterDisplay characterDisplay;

    [Header("Scene")]
    [SerializeField] private string stylingRoomSceneName = "StylingRoom";

    [Header("Events")]
    /// <summary>
    /// Fires when the player clicks Start Styling, just before the scene loads.
    /// </summary>
    public UnityEvent onCreationComplete;

    // ── Private index state ────────────────────────────────────────────────────

    private int _bodyTypeIndex;
    private int _frontHairIndex;
    private int _backHairIndex;
    private int _eyesIndex;
    private int _eyebrowsIndex;
    private int _mouthIndex;
    private int _earsIndex;
    private int _noseIndex;

    // ── Unity lifecycle ────────────────────────────────────────────────────────

    private void Start()
    {
        CharacterProfile profile = CharacterProfile.Instance;

        _bodyTypeIndex  = profile.BodyTypeIndex;
        _frontHairIndex = profile.FrontHairIndex;
        _backHairIndex  = profile.BackHairIndex;
        _eyesIndex      = profile.EyesIndex;
        _eyebrowsIndex  = profile.EyebrowsIndex;
        _mouthIndex     = profile.MouthIndex;
        _earsIndex      = profile.EarsIndex;
        _noseIndex      = profile.NoseIndex;

        ApplyAllSelections();
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>Sets the body type by index and updates the character display.</summary>
    public void SelectBodyType(int index)
    {
        SelectFeature(ClothingCategory.BodyBase, index,
            item => characterDisplay.SetBodyType(item),
            ref _bodyTypeIndex);
        CharacterProfile.Instance.BodyTypeIndex = _bodyTypeIndex;
    }

    /// <summary>Sets the front hair by index and updates the character display.</summary>
    public void SelectFrontHair(int index)
    {
        SelectFeature(ClothingCategory.FrontHair, index,
            item => characterDisplay.SetHair(ClothingCategory.FrontHair, item),
            ref _frontHairIndex);
        CharacterProfile.Instance.FrontHairIndex = _frontHairIndex;
    }

    /// <summary>Sets the back hair by index and updates the character display.</summary>
    public void SelectBackHair(int index)
    {
        SelectFeature(ClothingCategory.BackHair, index,
            item => characterDisplay.SetHair(ClothingCategory.BackHair, item),
            ref _backHairIndex);
        CharacterProfile.Instance.BackHairIndex = _backHairIndex;
    }

    /// <summary>Sets the eyes by index and updates the character display.</summary>
    public void SelectEyes(int index)
    {
        SelectFeature(ClothingCategory.Eyes, index,
            item => characterDisplay.SetFacialFeature(ClothingCategory.Eyes, item),
            ref _eyesIndex);
        CharacterProfile.Instance.EyesIndex = _eyesIndex;
    }

    /// <summary>Sets the eyebrows by index and updates the character display.</summary>
    public void SelectEyebrows(int index)
    {
        SelectFeature(ClothingCategory.Eyebrows, index,
            item => characterDisplay.SetFacialFeature(ClothingCategory.Eyebrows, item),
            ref _eyebrowsIndex);
        CharacterProfile.Instance.EyebrowsIndex = _eyebrowsIndex;
    }

    /// <summary>Sets the mouth by index and updates the character display.</summary>
    public void SelectMouth(int index)
    {
        SelectFeature(ClothingCategory.Mouth, index,
            item => characterDisplay.SetFacialFeature(ClothingCategory.Mouth, item),
            ref _mouthIndex);
        CharacterProfile.Instance.MouthIndex = _mouthIndex;
    }

    /// <summary>Sets the ears by index and updates the character display.</summary>
    public void SelectEars(int index)
    {
        SelectFeature(ClothingCategory.Ears, index,
            item => characterDisplay.SetFacialFeature(ClothingCategory.Ears, item),
            ref _earsIndex);
        CharacterProfile.Instance.EarsIndex = _earsIndex;
    }

    /// <summary>Sets the nose by index and updates the character display.</summary>
    public void SelectNose(int index)
    {
        SelectFeature(ClothingCategory.Nose, index,
            item => characterDisplay.SetFacialFeature(ClothingCategory.Nose, item),
            ref _noseIndex);
        CharacterProfile.Instance.NoseIndex = _noseIndex;
    }

    /// <summary>
    /// Saves all current selections to CharacterProfile and loads the Styling Room scene.
    /// Wire this to the START STYLING button onClick.
    /// </summary>
    public void OnStartStyling()
    {
        CharacterProfile profile = CharacterProfile.Instance;

        profile.BodyTypeIndex  = _bodyTypeIndex;
        profile.FrontHairIndex = _frontHairIndex;
        profile.BackHairIndex  = _backHairIndex;
        profile.EyesIndex      = _eyesIndex;
        profile.EyebrowsIndex  = _eyebrowsIndex;
        profile.MouthIndex     = _mouthIndex;
        profile.EarsIndex      = _earsIndex;
        profile.NoseIndex      = _noseIndex;

        onCreationComplete.Invoke();
        SceneManager.LoadScene(stylingRoomSceneName);
    }

    // ── Next / Prev convenience methods (wire to UI buttons) ──────────────────

    /// <summary>Advances to the next body type, wrapping around.</summary>
    public void NextBodyType()    => SelectBodyType(Wrap(_bodyTypeIndex + 1, ClothingCategory.BodyBase));
    /// <summary>Goes back to the previous body type, wrapping around.</summary>
    public void PrevBodyType()    => SelectBodyType(Wrap(_bodyTypeIndex - 1, ClothingCategory.BodyBase));

    /// <summary>Advances to the next front hair, wrapping around.</summary>
    public void NextFrontHair()   => SelectFrontHair(Wrap(_frontHairIndex + 1, ClothingCategory.FrontHair));
    /// <summary>Goes back to the previous front hair, wrapping around.</summary>
    public void PrevFrontHair()   => SelectFrontHair(Wrap(_frontHairIndex - 1, ClothingCategory.FrontHair));

    /// <summary>Advances to the next back hair, wrapping around.</summary>
    public void NextBackHair()    => SelectBackHair(Wrap(_backHairIndex + 1, ClothingCategory.BackHair));
    /// <summary>Goes back to the previous back hair, wrapping around.</summary>
    public void PrevBackHair()    => SelectBackHair(Wrap(_backHairIndex - 1, ClothingCategory.BackHair));

    /// <summary>Advances to the next eyes option, wrapping around.</summary>
    public void NextEyes()        => SelectEyes(Wrap(_eyesIndex + 1, ClothingCategory.Eyes));
    /// <summary>Goes back to the previous eyes option, wrapping around.</summary>
    public void PrevEyes()        => SelectEyes(Wrap(_eyesIndex - 1, ClothingCategory.Eyes));

    /// <summary>
    /// Advances to the next eyebrows option. When CharacterDisplay's
    /// EyebrowsAutoMatchHair is on, steps shape-by-shape (color tracks hair);
    /// when off, cycles through every entry including color variants.
    /// </summary>
    public void NextEyebrows()
    {
        bool autoMatch = characterDisplay != null && characterDisplay.EyebrowsAutoMatchHair;
        ClothingItemData[] arr = GetArray(ClothingCategory.Eyebrows);
        SelectEyebrows(autoMatch
            ? NextDifferentGroup(_eyebrowsIndex, arr, +1)
            : Wrap(_eyebrowsIndex + 1, arr));
    }

    /// <summary>Goes back to the previous eyebrows option (see <see cref="NextEyebrows"/>).</summary>
    public void PrevEyebrows()
    {
        bool autoMatch = characterDisplay != null && characterDisplay.EyebrowsAutoMatchHair;
        ClothingItemData[] arr = GetArray(ClothingCategory.Eyebrows);
        SelectEyebrows(autoMatch
            ? NextDifferentGroup(_eyebrowsIndex, arr, -1)
            : Wrap(_eyebrowsIndex - 1, arr));
    }

    /// <summary>
    /// Toggles eyebrow auto-match-to-hair on the CharacterDisplay. Wire this to
    /// an "Experimental Brows" UI button so the player can opt out of color
    /// matching and pick brow colors independently.
    /// </summary>
    public void ToggleEyebrowsAutoMatchHair()
    {
        if (characterDisplay == null) return;
        characterDisplay.SetEyebrowsAutoMatchHair(!characterDisplay.EyebrowsAutoMatchHair);
    }

    /// <summary>Sets eyebrow auto-match-to-hair to a specific value.</summary>
    public void SetEyebrowsAutoMatchHair(bool autoMatch)
    {
        if (characterDisplay == null) return;
        characterDisplay.SetEyebrowsAutoMatchHair(autoMatch);
    }

    /// <summary>Advances to the next mouth option, wrapping around.</summary>
    public void NextMouth()       => SelectMouth(Wrap(_mouthIndex + 1, ClothingCategory.Mouth));
    /// <summary>Goes back to the previous mouth option, wrapping around.</summary>
    public void PrevMouth()       => SelectMouth(Wrap(_mouthIndex - 1, ClothingCategory.Mouth));

    /// <summary>Advances to the next ears shape, skipping color variants (color tracks body).</summary>
    public void NextEars()        => SelectEars(NextDifferentGroup(_earsIndex, GetArray(ClothingCategory.Ears), +1));
    /// <summary>Goes back to the previous ears shape, skipping color variants (color tracks body).</summary>
    public void PrevEars()        => SelectEars(NextDifferentGroup(_earsIndex, GetArray(ClothingCategory.Ears), -1));

    /// <summary>Advances to the next nose option, wrapping around.</summary>
    public void NextNose()        => SelectNose(Wrap(_noseIndex + 1, ClothingCategory.Nose));
    /// <summary>Goes back to the previous nose option, wrapping around.</summary>
    public void PrevNose()        => SelectNose(Wrap(_noseIndex - 1, ClothingCategory.Nose));

    /// <summary>Applies all current index selections to the CharacterDisplay.</summary>
    public void ApplyAllSelections()
    {
        SelectBodyType(_bodyTypeIndex);
        SelectFrontHair(_frontHairIndex);
        SelectBackHair(_backHairIndex);
        SelectEyes(_eyesIndex);
        SelectEyebrows(_eyebrowsIndex);
        SelectMouth(_mouthIndex);
        SelectEars(_earsIndex);
        SelectNose(_noseIndex);
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// Fetches the array of available items for a category from the linked
    /// <see cref="CharacterDisplay"/>'s database. Returns an empty array if no
    /// display or database is available.
    /// </summary>
    private ClothingItemData[] GetArray(ClothingCategory category)
    {
        if (characterDisplay == null) return System.Array.Empty<ClothingItemData>();
        return characterDisplay.GetItemsByCategory(category);
    }

    private int Wrap(int index, ClothingCategory category)
    {
        return Wrap(index, GetArray(category));
    }

    private static int Wrap(int index, ClothingItemData[] array)
    {
        if (array == null || array.Length == 0) return 0;
        return ((index % array.Length) + array.Length) % array.Length;
    }

    /// <summary>
    /// Walks <paramref name="array"/> from <paramref name="currentIndex"/> in
    /// <paramref name="direction"/> (+1 or -1) until it finds an entry whose
    /// <see cref="ClothingItemData.GroupName"/> differs from the current entry's,
    /// then returns that index. Used to cycle ear/brow shapes while skipping
    /// per-color duplicates (color is auto-selected by CharacterDisplay).
    /// </summary>
    private static int NextDifferentGroup(int currentIndex, ClothingItemData[] array, int direction)
    {
        if (array == null || array.Length == 0) return 0;
        if (direction == 0) return Wrap(currentIndex, array);

        int n = array.Length;
        int start = Wrap(currentIndex, array);
        string currentGroup = array[start] != null ? array[start].GroupName : null;

        for (int step = 1; step <= n; step++)
        {
            int idx = Wrap(start + direction * step, array);
            ClothingItemData candidate = array[idx];
            if (candidate == null) continue;
            if (candidate.GroupName != currentGroup)
                return idx;
        }
        return start; // only one group present
    }

    /// <summary>
    /// Clamps the index, updates the stored index, and calls applyAction with
    /// the item at that index. Does nothing if the category has no items.
    /// </summary>
    private void SelectFeature(ClothingCategory category, int index,
        System.Action<ClothingItemData> applyAction, ref int storedIndex)
    {
        ClothingItemData[] array = GetArray(category);
        if (array.Length == 0) return;

        int clamped = Mathf.Clamp(index, 0, array.Length - 1);
        storedIndex = clamped;

        ClothingItemData item = array[clamped];
        if (item != null && applyAction != null)
            applyAction(item);
    }
}
