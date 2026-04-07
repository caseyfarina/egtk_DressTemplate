/// <summary>
/// Defines the clothing layer categories used to organize and sort
/// items on the character. Values are fixed to preserve serialization
/// of existing ScriptableObject assets — do not renumber.
/// </summary>
public enum ClothingCategory
{
    /// <summary>Hats and headwear worn on top of the head.</summary>
    Hat = 0,

    /// <summary>Tops, shirts, and upper-body garments.</summary>
    Top = 1,

    /// <summary>Pants, shorts, and other lower-body garments.</summary>
    Bottom = 2,

    /// <summary>Footwear including sneakers, heels, and boots.</summary>
    Shoes = 3,

    /// <summary>Jewellery, bags, and other accessories.</summary>
    Accessory = 4,

    /// <summary>Skirts worn on the lower body.</summary>
    Skirt = 5,

    /// <summary>One-piece dress garments.</summary>
    Dress = 6,

    /// <summary>Socks, stockings, and leggings.</summary>
    SocksLeggings = 7,

    /// <summary>Jackets, coats, and other outerwear.</summary>
    Outerwear = 8,

    /// <summary>Front hair layer rendered in front of the face.</summary>
    FrontHair = 9,

    /// <summary>Back hair layer rendered behind the body.</summary>
    BackHair = 10,

    /// <summary>Eye sprites for the character face.</summary>
    Eyes = 11,

    /// <summary>Eyebrow sprites for the character face.</summary>
    Eyebrows = 12,

    /// <summary>Mouth and lip sprites for the character face.</summary>
    Mouth = 13,

    /// <summary>Ear sprites for the character face.</summary>
    Ears = 14,

    /// <summary>Nose sprites for the character face.</summary>
    Nose = 15,

    /// <summary>The base body/skin layer underneath all clothing.</summary>
    BodyBase = 16,
}

/// <summary>
/// Represents how well an outfit matched a judge's style preferences.
/// Used to select dialogue lines and calculate money rewards.
/// </summary>
public enum OutfitRating
{
    /// <summary>The outfit perfectly matched the judge's theme.</summary>
    Excellent,

    /// <summary>The outfit partially matched the judge's theme.</summary>
    Good,

    /// <summary>The outfit did not match the judge's theme.</summary>
    Poor,
}
