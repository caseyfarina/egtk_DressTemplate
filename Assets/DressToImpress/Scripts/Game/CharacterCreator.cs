using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages row-by-row character customisation in the Character Creation scene.
/// Players select body type, hair, and facial features. Selections are saved to
/// <see cref="CharacterProfile.Instance"/> and carried into the Styling Room scene.
/// Wire each row's prev/next buttons to the corresponding Select* method, and wire
/// the START STYLING button to <see cref="OnStartStyling"/>.
/// </summary>
public class CharacterCreator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterDisplay characterDisplay;

    [Header("Available Options — assign ClothingItemData arrays in Inspector")]
    [SerializeField] private ClothingItemData[] bodyTypes;
    [SerializeField] private ClothingItemData[] frontHairs;
    [SerializeField] private ClothingItemData[] backHairs;
    [SerializeField] private ClothingItemData[] eyes;
    [SerializeField] private ClothingItemData[] eyebrows;
    [SerializeField] private ClothingItemData[] mouths;
    [SerializeField] private ClothingItemData[] ears;
    [SerializeField] private ClothingItemData[] noses;

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
    /// <param name="index">Index into the bodyTypes array. Clamped to valid bounds.</param>
    public void SelectBodyType(int index)
    {
        SelectFeature(bodyTypes, index,
            item => characterDisplay.SetBodyType(item),
            ref _bodyTypeIndex);
        CharacterProfile.Instance.BodyTypeIndex = _bodyTypeIndex;
    }

    /// <summary>Sets the front hair by index and updates the character display.</summary>
    /// <param name="index">Index into the frontHairs array. Clamped to valid bounds.</param>
    public void SelectFrontHair(int index)
    {
        SelectFeature(frontHairs, index,
            item => characterDisplay.SetHair(ClothingCategory.FrontHair, item),
            ref _frontHairIndex);
        CharacterProfile.Instance.FrontHairIndex = _frontHairIndex;
    }

    /// <summary>Sets the back hair by index and updates the character display.</summary>
    /// <param name="index">Index into the backHairs array. Clamped to valid bounds.</param>
    public void SelectBackHair(int index)
    {
        SelectFeature(backHairs, index,
            item => characterDisplay.SetHair(ClothingCategory.BackHair, item),
            ref _backHairIndex);
        CharacterProfile.Instance.BackHairIndex = _backHairIndex;
    }

    /// <summary>Sets the eyes by index and updates the character display.</summary>
    /// <param name="index">Index into the eyes array. Clamped to valid bounds.</param>
    public void SelectEyes(int index)
    {
        SelectFeature(eyes, index,
            item => characterDisplay.SetFacialFeature(ClothingCategory.Eyes, item),
            ref _eyesIndex);
        CharacterProfile.Instance.EyesIndex = _eyesIndex;
    }

    /// <summary>Sets the eyebrows by index and updates the character display.</summary>
    /// <param name="index">Index into the eyebrows array. Clamped to valid bounds.</param>
    public void SelectEyebrows(int index)
    {
        SelectFeature(eyebrows, index,
            item => characterDisplay.SetFacialFeature(ClothingCategory.Eyebrows, item),
            ref _eyebrowsIndex);
        CharacterProfile.Instance.EyebrowsIndex = _eyebrowsIndex;
    }

    /// <summary>Sets the mouth by index and updates the character display.</summary>
    /// <param name="index">Index into the mouths array. Clamped to valid bounds.</param>
    public void SelectMouth(int index)
    {
        SelectFeature(mouths, index,
            item => characterDisplay.SetFacialFeature(ClothingCategory.Mouth, item),
            ref _mouthIndex);
        CharacterProfile.Instance.MouthIndex = _mouthIndex;
    }

    /// <summary>Sets the ears by index and updates the character display.</summary>
    /// <param name="index">Index into the ears array. Clamped to valid bounds.</param>
    public void SelectEars(int index)
    {
        SelectFeature(ears, index,
            item => characterDisplay.SetFacialFeature(ClothingCategory.Ears, item),
            ref _earsIndex);
        CharacterProfile.Instance.EarsIndex = _earsIndex;
    }

    /// <summary>Sets the nose by index and updates the character display.</summary>
    /// <param name="index">Index into the noses array. Clamped to valid bounds.</param>
    public void SelectNose(int index)
    {
        SelectFeature(noses, index,
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
    public void NextBodyType()    => SelectBodyType(Wrap(_bodyTypeIndex + 1, bodyTypes));
    /// <summary>Goes back to the previous body type, wrapping around.</summary>
    public void PrevBodyType()    => SelectBodyType(Wrap(_bodyTypeIndex - 1, bodyTypes));

    /// <summary>Advances to the next front hair, wrapping around.</summary>
    public void NextFrontHair()   => SelectFrontHair(Wrap(_frontHairIndex + 1, frontHairs));
    /// <summary>Goes back to the previous front hair, wrapping around.</summary>
    public void PrevFrontHair()   => SelectFrontHair(Wrap(_frontHairIndex - 1, frontHairs));

    /// <summary>Advances to the next back hair, wrapping around.</summary>
    public void NextBackHair()    => SelectBackHair(Wrap(_backHairIndex + 1, backHairs));
    /// <summary>Goes back to the previous back hair, wrapping around.</summary>
    public void PrevBackHair()    => SelectBackHair(Wrap(_backHairIndex - 1, backHairs));

    /// <summary>Advances to the next eyes option, wrapping around.</summary>
    public void NextEyes()        => SelectEyes(Wrap(_eyesIndex + 1, eyes));
    /// <summary>Goes back to the previous eyes option, wrapping around.</summary>
    public void PrevEyes()        => SelectEyes(Wrap(_eyesIndex - 1, eyes));

    /// <summary>Advances to the next eyebrows option, wrapping around.</summary>
    public void NextEyebrows()    => SelectEyebrows(Wrap(_eyebrowsIndex + 1, eyebrows));
    /// <summary>Goes back to the previous eyebrows option, wrapping around.</summary>
    public void PrevEyebrows()    => SelectEyebrows(Wrap(_eyebrowsIndex - 1, eyebrows));

    /// <summary>Advances to the next mouth option, wrapping around.</summary>
    public void NextMouth()       => SelectMouth(Wrap(_mouthIndex + 1, mouths));
    /// <summary>Goes back to the previous mouth option, wrapping around.</summary>
    public void PrevMouth()       => SelectMouth(Wrap(_mouthIndex - 1, mouths));

    /// <summary>Advances to the next ears option, wrapping around.</summary>
    public void NextEars()        => SelectEars(Wrap(_earsIndex + 1, ears));
    /// <summary>Goes back to the previous ears option, wrapping around.</summary>
    public void PrevEars()        => SelectEars(Wrap(_earsIndex - 1, ears));

    /// <summary>Advances to the next nose option, wrapping around.</summary>
    public void NextNose()        => SelectNose(Wrap(_noseIndex + 1, noses));
    /// <summary>Goes back to the previous nose option, wrapping around.</summary>
    public void PrevNose()        => SelectNose(Wrap(_noseIndex - 1, noses));

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
    /// Clamps the index to the array bounds, updates the stored index, and calls
    /// applyAction with the item at that index. Does nothing if the array is null or empty.
    /// </summary>
    /// <param name="array">The options array to index into.</param>
    /// <param name="index">The requested index (will be clamped).</param>
    /// <param name="applyAction">Callback that applies the selected item to the CharacterDisplay.</param>
    /// <param name="storedIndex">The field that stores the current selection index (updated in place).</param>
    private static int Wrap(int index, ClothingItemData[] array)
    {
        if (array == null || array.Length == 0) return 0;
        return ((index % array.Length) + array.Length) % array.Length;
    }

    private void SelectFeature(ClothingItemData[] array, int index,
        System.Action<ClothingItemData> applyAction, ref int storedIndex)
    {
        if (array == null || array.Length == 0) return;

        int clamped = Mathf.Clamp(index, 0, array.Length - 1);
        storedIndex = clamped;

        ClothingItemData item = array[clamped];
        if (item != null && applyAction != null)
            applyAction(item);
    }
}
