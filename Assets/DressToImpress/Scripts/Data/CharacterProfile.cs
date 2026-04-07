using UnityEngine;

/// <summary>
/// Runtime-only ScriptableObject singleton that stores the player's current
/// character customisation selections (body type, skin tone, facial features, hair).
/// Never saved to disk — created via <see cref="ScriptableObject.CreateInstance{T}"/>
/// and reset automatically at the start of each play session.
/// Access via <see cref="Instance"/>.
/// </summary>
public class CharacterProfile : ScriptableObject
{
    // ── Singleton ──────────────────────────────────────────────────────────

    private static CharacterProfile _instance;

    /// <summary>
    /// The global runtime instance of <see cref="CharacterProfile"/>.
    /// Created lazily on first access; hidden from the Asset Database so it
    /// is never accidentally saved to disk.
    /// </summary>
    public static CharacterProfile Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = ScriptableObject.CreateInstance<CharacterProfile>();
                _instance.hideFlags = HideFlags.HideAndDontSave;
            }
            return _instance;
        }
    }

    /// <summary>
    /// Clears the static instance reference at the start of every play session,
    /// including when domain reload is disabled. This ensures a clean slate
    /// regardless of Unity 6 Enter Play Mode settings.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetInstance()
    {
        _instance = null;
    }

    // ── Runtime-only fields (never serialized) ─────────────────────────────

    /// <summary>Index of the selected body type variant.</summary>
    [System.NonSerialized] private int _bodyTypeIndex;

    /// <summary>Index of the selected skin tone.</summary>
    [System.NonSerialized] private int _skinToneIndex;

    /// <summary>Index of the selected front hair style.</summary>
    [System.NonSerialized] private int _frontHairIndex;

    /// <summary>Index of the selected back hair style.</summary>
    [System.NonSerialized] private int _backHairIndex;

    /// <summary>Index of the selected hair colour.</summary>
    [System.NonSerialized] private int _hairColorIndex;

    /// <summary>Index of the selected eye style.</summary>
    [System.NonSerialized] private int _eyesIndex;

    /// <summary>Index of the selected eyebrow style.</summary>
    [System.NonSerialized] private int _eyebrowsIndex;

    /// <summary>Index of the selected mouth style.</summary>
    [System.NonSerialized] private int _mouthIndex;

    /// <summary>Index of the selected ear style.</summary>
    [System.NonSerialized] private int _earsIndex;

    /// <summary>Index of the selected nose style.</summary>
    [System.NonSerialized] private int _noseIndex;

    // ── Public properties ──────────────────────────────────────────────────

    /// <summary>Index of the selected body type variant.</summary>
    public int BodyTypeIndex    { get => _bodyTypeIndex;   set => _bodyTypeIndex   = value; }

    /// <summary>Index of the selected skin tone.</summary>
    public int SkinToneIndex    { get => _skinToneIndex;   set => _skinToneIndex   = value; }

    /// <summary>Index of the selected front hair style.</summary>
    public int FrontHairIndex   { get => _frontHairIndex;  set => _frontHairIndex  = value; }

    /// <summary>Index of the selected back hair style.</summary>
    public int BackHairIndex    { get => _backHairIndex;   set => _backHairIndex   = value; }

    /// <summary>Index of the selected hair colour.</summary>
    public int HairColorIndex   { get => _hairColorIndex;  set => _hairColorIndex  = value; }

    /// <summary>Index of the selected eye style.</summary>
    public int EyesIndex        { get => _eyesIndex;       set => _eyesIndex       = value; }

    /// <summary>Index of the selected eyebrow style.</summary>
    public int EyebrowsIndex    { get => _eyebrowsIndex;   set => _eyebrowsIndex   = value; }

    /// <summary>Index of the selected mouth style.</summary>
    public int MouthIndex       { get => _mouthIndex;      set => _mouthIndex      = value; }

    /// <summary>Index of the selected ear style.</summary>
    public int EarsIndex        { get => _earsIndex;       set => _earsIndex       = value; }

    /// <summary>Index of the selected nose style.</summary>
    public int NoseIndex        { get => _noseIndex;       set => _noseIndex       = value; }

    // ── Public methods ─────────────────────────────────────────────────────

    /// <summary>Resets all selections to index 0.</summary>
    public void Reset()
    {
        _bodyTypeIndex  = 0;
        _skinToneIndex  = 0;
        _frontHairIndex = 0;
        _backHairIndex  = 0;
        _hairColorIndex = 0;
        _eyesIndex      = 0;
        _eyebrowsIndex  = 0;
        _mouthIndex     = 0;
        _earsIndex      = 0;
        _noseIndex      = 0;
    }

    /// <summary>Copies all index values from another CharacterProfile.</summary>
    /// <param name="other">The source profile to copy values from. Must not be null.</param>
    public void CopyFrom(CharacterProfile other)
    {
        if (other == null)
        {
            Debug.LogWarning("[CharacterProfile] CopyFrom received a null profile — no values were copied.");
            return;
        }

        _bodyTypeIndex  = other._bodyTypeIndex;
        _skinToneIndex  = other._skinToneIndex;
        _frontHairIndex = other._frontHairIndex;
        _backHairIndex  = other._backHairIndex;
        _hairColorIndex = other._hairColorIndex;
        _eyesIndex      = other._eyesIndex;
        _eyebrowsIndex  = other._eyebrowsIndex;
        _mouthIndex     = other._mouthIndex;
        _earsIndex      = other._earsIndex;
        _noseIndex      = other._noseIndex;
    }
}
