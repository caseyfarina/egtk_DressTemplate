# Artist Workflow — Adding Clothing & Character Art

This is the **only** workflow you need for getting new art into the game. The previous "drag every asset into eight Inspector arrays" step is gone. The new system uses one central `ClothingDatabase` that the importer keeps up-to-date for you.

---

## TL;DR — every time you add new art

1. Drop PNGs into the right `Assets/Art/...` subfolder using the naming convention.
2. **Dress To Impress > Import Clothing Assets** → click **Scan & Import All**.
3. Done. Run the game.

That's it. No drag-and-drop into Inspector arrays anymore.

---

## File naming convention

```
{itemName}_color{N}_x{X}_y{Y}.png
```

| Part | Meaning | Example |
|------|---------|---------|
| `itemName` | Shape / variant name. Items sharing this name are color variants of each other. | `brow1`, `Ear2 [dynamic]`, `Jeans (Dark Blue)` |
| `color{N}` | Color variant index. Body uses 1–9 (skin tones). Hair/brows use 1–12. Items without color variants use `color1`. | `_color3` |
| `x{X}_y{Y}` | Top-left pixel position in the 2048×2048 Krita canvas. | `_x488_y236` |

**Rules**
- The same `itemName` across multiple files = a color group. The auto-match system uses this to pair ear color to skin tone, and brow color to hair color.
- Filenames marked `[icon]` are skipped by the importer — those are reference/preview thumbnails.
- The folder you put the PNG in determines the category. See the table below.

## Folder → category map

| Folder | Category |
|---|---|
| `Assets/Art/BodyTypes/` | BodyBase |
| `Assets/Art/Hair/Front/` | FrontHair |
| `Assets/Art/Hair/Back/` | BackHair |
| `Assets/Art/FacialFeatures/Eyes/` | Eyes |
| `Assets/Art/FacialFeatures/Eyebrows/` | Eyebrows |
| `Assets/Art/FacialFeatures/Mouths/` | Mouth |
| `Assets/Art/FacialFeatures/Ears/` | Ears |
| `Assets/Art/FacialFeatures/Nose/` | Nose |
| `Assets/Art/Clothing/Tops/` | Top |
| `Assets/Art/Clothing/Bottoms/` | Bottom |
| `Assets/Art/Clothing/Skirts/` | Skirt |
| `Assets/Art/Clothing/Dresses/` | Dress |
| `Assets/Art/Clothing/Shoes/` | Shoes |
| `Assets/Art/Clothing/SocksLeggings/` | SocksLeggings |
| `Assets/Art/Clothing/Outerwear/` | Outerwear |
| `Assets/Art/Clothing/Hats/` | Hat |
| `Assets/Art/Clothing/Accessories/` | Accessory |

---

## What the importer does

When you click **Scan & Import All**, the importer:

1. Walks every PNG under `Assets/Art/`.
2. Parses the filename for name, color index, and canvas position.
3. Resolves the category from the folder.
4. Creates (or updates) a `ClothingItemData` ScriptableObject at `Assets/DressToImpress/Data/{Category}/{itemName}_color{N}.asset`.
5. **Rebuilds `Assets/DressToImpress/Resources/ClothingDatabase.asset`** to include every item it just found.

The `ClothingDatabase` is a single asset that every scene reads from — `CharacterDisplay`, `CharacterCreator`, and `ClothingPanelManager` all pull their items from it automatically. There are no per-component arrays to maintain.

**Updates preserve instructor tuning.** When the importer re-imports an existing item, it overwrites Sprite, position, color index, group name, and category — but **not** `styleScore` or `themeTags`. You can re-export PNGs freely without losing any tuning data.

---

## Color groups (auto-match)

Items with the same `itemName` are treated as one color group. Two systems use this:

| Layer | Drives color of | What the player sees |
|---|---|---|
| `BodyBase.colorVariantIndex` | `Ears` | Cycling ears steps through *shapes* only; ear color always matches the body's skin tone. |
| `FrontHair.colorVariantIndex` | `Eyebrows` | Cycling brows steps through *shapes* only; brow color always matches hair color. Toggle off ("experimental brows") to cycle every shape *and* color. |

For the auto-match to work, every shape must exist in every color the source layer can show:
- Bodies have 9 skin tones → every ear shape needs `_color1` … `_color9`.
- Hair has 12 colors → every brow shape needs `_color1` … `_color12`.

If a color is missing for a shape, the system falls back to whatever color exists in that group — the player will see the wrong color, so just make sure you export the full set.

---

## Adding a new category

If you want a category that doesn't exist yet (say "Tattoos"):

1. Add the new value to `ClothingCategory.cs` (programmer task, but worth knowing). Don't renumber existing values.
2. Add the folder mapping in `ClothingImporter.TryGetCategory(...)`.
3. Add a sorting order in `CharacterDisplay.SortingOrders`.
4. Decide if it's a clothing slot (scoreable) or a feature (not scored). The default is "scoreable" — if it's a feature, add it to `CharacterDisplay.NonClothingCategories`.

Then drop art in the new folder and import.

---

## Adding a new color variant to an existing item

1. Export the new PNG with the same `itemName` and a new `_color{N}` number, in the same folder.
2. Re-import.

That's it. The new color shows up automatically in cycle UIs and in any color-match group.

---

## Removing items

1. Delete the PNG from `Assets/Art/...`.
2. Delete the corresponding `.asset` from `Assets/DressToImpress/Data/{Category}/`.
3. Re-import (this rebuilds the database without the deleted items).

The importer doesn't auto-delete `.asset` files for missing PNGs — that's intentional, so renaming a PNG in Krita doesn't silently nuke an asset that has tuning data on it. Delete by hand when you actually mean to remove.

---

## Common questions

**Q: I added a new PNG and re-imported but it doesn't show up in the Styling Room.**
A: Check `Assets/DressToImpress/Resources/ClothingDatabase.asset` exists. Open it in the Inspector and confirm your item is in the `allItems` list. If not, check the filename pattern matches and the folder is in the map above.

**Q: The character is missing a layer (ears, eyes, etc.) on first scene load.**
A: That category has no items in the database. Import at least one item per category that the character creator expects.

**Q: My imported item looks correct but the color doesn't track skin/hair.**
A: Make sure `itemName` (everything before `_color`) is byte-for-byte identical across the variants. Capitalization and spaces matter. The auto-match uses `GroupName` which is just the parsed `itemName`.

**Q: I want one scene to show only a subset of items (a boutique).**
A: `ClothingPanelManager` has a legacy `allClothingItems` array. If you fill it, it overrides the database for that scene. Leave it empty to use the full database.

**Q: Do I still have to drag arrays into `CharacterCreator` / `CharacterDisplay`?**
A: No. Those array fields are gone. The components read from the database, which the importer keeps current. The only Inspector field you need to care about is `database` on each component, and the SceneSetup scripts wire it automatically.

---

## Quick checklist for a new clothing pack

- [ ] PNGs follow `{name}_color{N}_x{X}_y{Y}.png`.
- [ ] All color variants of the same shape share an exact `name`.
- [ ] PNGs are in the correct category subfolder.
- [ ] Texture import settings: Texture Type = `Sprite (2D and UI)`. (Unity should detect this automatically for new PNGs in folders that already contain sprites.)
- [ ] Run **Dress To Impress > Import Clothing Assets**.
- [ ] Spot-check one or two new assets in `Assets/DressToImpress/Data/{Category}/` to confirm they were created.
- [ ] Press Play and verify the items appear in the Styling Room panel and on the character.
