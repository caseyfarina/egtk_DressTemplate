# Dress To Impress — Quick Start Guide

Welcome! This guide walks you through everything you need to get a dress-up game running in Unity. No programming required.

---

## 1. What Is This Template?

**Dress To Impress** is a 2D dress-up game built on the eventGameToolKit (EGTK) framework. Here's the big picture:

- A **customer (judge)** walks in and tells you the look they want ("Show me something bold!")
- The **player** picks clothing from a panel at the bottom of the screen to dress the character
- The player clicks **SUBMIT OUTFIT** — the judge reacts and awards money based on how well the outfit matched their style
- Click **NEXT** to bring in the next judge and start fresh
- The **BOUTIQUE** (coming soon!) lets players spend their money on new clothes

The game spans two scenes: **Character Creation** (where the player designs their character's face and hair) and the **Styling Room** (where the actual dress-up rounds happen).

---

## 2. How the Game Works

Here's the round-by-round flow in plain English:

1. The **Styling Room** scene loads and a judge appears with their name, a style label (like "Bold" or "Glam"), and a prompt line ("I need something that pops!").
2. The player clicks clothing buttons along the bottom panel. Clicking an item dresses the character in it. Clicking it again removes it.
3. When the player is happy with the outfit, they click **SUBMIT OUTFIT**.
4. The clothing buttons lock so nothing can change. The outfit is scored based on how many items were worn and how well their theme tags match the judge's style.
5. The judge reacts with a line of dialogue and awards money. The better the match, the more money.
6. The player clicks **NEXT** — the clothing resets, a new judge appears, and a new round begins.

The money total is tracked by a **GameCollectionManager** (an EGTK component). You can display it on screen by wiring `JudgeManager.onMoneyAwarded` to `GameCollectionManager.Increment`.

---

## 3. Setting Up Your Art (Krita to Unity)

All the character art lives in a single Krita file (`Assets/dressAssets/mainBase.kra`). A Python export script slices each layer into individual PNGs and names them with the position data Unity needs for automatic alignment. The Unity importer then reads those PNGs and creates the game data assets in one click.

### 3A. How the Export Script Works

The script (`Assets/dressAssets/export_layers.py`) does the following when you run it:

1. Scans the Krita layer tree for groups that match the expected names (see 3B below).
2. For each clothing item it finds, it hides everything else in the document, makes only that item's layers visible, measures the pixel bounding box of the visible area, and exports a cropped PNG.
3. Each PNG is named using this convention:

   ```
   {itemName}_color{N}_x{X}_y{Y}.png
   ```

   - `itemName` — the name of the item's group in Krita
   - `N` — the color variant number (1 for the first color, 2 for the second, etc.)
   - `X`, `Y` — the **top-left pixel coordinate** of the cropped sprite within the 2048 × 2048 canvas

   **This filename is the source of truth for positioning.** Unity reads `X` and `Y` directly from the filename to place each sprite at exactly the right world-space position on the character — no manual positioning required.

4. Files are written into your Unity project under `Assets/Art/`, organized into subfolders by category.

### 3B. Krita File Organization

> **Canvas must be 2048 × 2048 pixels.** All items are drawn on the same canvas. The canvas center is world origin (0, 0) in Unity.

The layer panel must follow this exact group hierarchy. **Group names are case-insensitive** but must match the names listed here:

```
[Root]
│
├── clothes/                    ← top-level group
│   ├── hats/                  → Assets/Art/Clothing/Hats
│   ├── tops/                  → Assets/Art/Clothing/Tops
│   ├── inners/                → Assets/Art/Clothing/Tops  (same output as tops)
│   ├── trousers/              → Assets/Art/Clothing/Bottoms
│   ├── skirts/                → Assets/Art/Clothing/Skirts
│   ├── dresses/               → Assets/Art/Clothing/Dresses
│   ├── shoes/                 → Assets/Art/Clothing/Shoes
│   ├── socks/                 → Assets/Art/Clothing/SocksLeggings
│   ├── accessories/           → Assets/Art/Clothing/Accessories
│   └── outwear/               → Assets/Art/Clothing/Outerwear
│
├── hair/                       ← top-level group
│   ├── front/                 → Assets/Art/Hair/Front
│   └── back/                  → Assets/Art/Hair/Back
│
├── brows/                      ← top-level group (contains a sub-group also named "brows")
│   └── brows/                 → Assets/Art/FacialFeatures/Eyebrows
│
└── body/                       ← top-level group (special handler — see below)
    ├── eyes/                  → Assets/Art/FacialFeatures/Eyes
    ├── ears/                  → Assets/Art/FacialFeatures/Ears
    ├── mouths/                → Assets/Art/FacialFeatures/Mouths
    ├── nose/                  → Assets/Art/FacialFeatures/Nose
    ├── bases/                 → body outline layers (combined with skin tones, output to Assets/Art/BodyTypes)
    └── skinToen/              → skin-tone fill layers (name must start with "skin")
```

### 3C. Item Group Structure (Clothing, Hair, Facial Features)

Each item inside a category group should be organized as its own **sub-group**. The sub-group name becomes the item's display name in the game.

**Standard structure (linework + color variants):**

```
hats/
└── beret1/                    ← item group — the group name is the item name
    ├── beret1_line            ← linework layer: name must end with _line
    ├── beret1_color1          ← color variant 1: name must end with _color1
    └── beret1_color2          ← color variant 2: name must end with _color2
```

- The `_line` layer is shared across all variants — it's composited with each color layer when exporting.
- Each `_color{N}` layer is exported separately, producing one PNG per variant.
- The number after `_color` becomes the `N` in the filename.

**Single-color item (no color variants):**

```
accessories/
└── ribbon1/
    └── ribbon1               ← one plain layer with no special suffix → exported as color1
```

**Multiple plain layers (no _line or _color suffix):**

```
shoes/
└── sneaker1/
    ├── sneaker1_white        ← no suffix → exported as color1
    └── sneaker1_black        ← no suffix → exported as color2
```

Each plain layer becomes its own color variant, numbered in the order they appear (top to bottom).

**Nested sub-categories (grouping many items):**

```
dresses/
├── casual/                   ← optional sub-category group for organization
│   ├── sundress1/
│   └── sundress2/
└── formal/
    └── gown1/
```

The walker recurses into nested groups until it finds a leaf item group (one whose children are all paint layers, not groups).

### 3D. Body Type Structure (Special)

Body types are handled differently because each body outline must be combined with every skin-tone fill before export.

```
body/
├── bases/                    ← must be named exactly "bases"
│   ├── base1/               ← one sub-group per body shape
│   │   └── base1_outline    ← the paint layer with the body outline/linework
│   └── base2/
│       └── base2_outline
└── skinToen/                 ← sub-group name must start with "skin" (typo in script is intentional)
    ├── skin1                 ← paint layer: skin-tone fill #1
    ├── skin2
    └── skin3
```

The script composites every **base outline × every skin tone**, producing:

```
base1_color1_x{X}_y{Y}.png   ← base1 + skin1
base1_color2_x{X}_y{Y}.png   ← base1 + skin2
base1_color3_x{X}_y{Y}.png   ← base1 + skin3
base2_color1_x{X}_y{Y}.png   ← base2 + skin1
...
```

These land in `Assets/Art/BodyTypes/`.

### 3E. Running the Export Script

1. Open `Assets/dressAssets/mainBase.kra` in **Krita 5+**.
2. Go to **Tools → Scripts → Run Script** and select `export_layers.py`.
   - Alternatively: open **Tools → Scripter**, paste the script contents, and click **Run**.
3. A folder picker dialog appears. Navigate to your Unity project root (the folder that contains the `Assets/` directory) and click **OK**.
4. The script runs. Watch the Scripter console for progress. Each exported PNG prints a confirmation line. Warnings print for any group names it cannot find.
5. When it prints `Export complete.`, switch to Unity.

### 3F. Running the Unity Importer

6. Unity may auto-reimport the new PNGs. If not, right-click `Assets/Art/` and choose **Reimport**.
7. Open **Dress To Impress → Import Clothing Assets** from the menu bar.
8. Click **Scan & Import All**. The importer scans `Assets/Art/` for any PNG whose filename matches `{name}_color{N}_x{X}_y{Y}.png`, determines the category from the folder path, and creates or updates a **ClothingItemData** asset for each one.
9. Assets land in `Assets/DressToImpress/Data/{Category}/`.

**What the importer preserves on update:** If you re-export and re-import, the importer only overwrites the Sprite, canvas position, color variant index, and category. It leaves your **Style Score** and **Theme Tags** untouched — tuning you've done in the Inspector is safe.

---

## 4. Setting Up the Scenes

### Scene 1: Character Creation

1. Create a new empty scene (**File → New Scene**) and name it `CharacterCreation`.
2. Run **Dress To Impress → Setup Scene — Character Creation** from the menu bar. This adds all the required GameObjects, wires the prev/next arrow buttons for all 8 feature rows (Skin Tone, Eyes, Eyebrows, Mouth, Ears, Nose, Front Hair, Back Hair), and assigns all ClothingItemData arrays automatically.
3. Press **Play** to test. The character should appear in the left half of the screen with the selection panel on the right. Each arrow button cycles through all available options with wrap-around.
4. Save the scene.

**Camera note:** The Character Creation camera uses `Orthographic Size = 10.24`, which exactly fits the 2048 × 2048 canvas at 100 Pixels Per Unit (2048 ÷ 100 ÷ 2 = 10.24). Do not change this value.

### Scene 2: Styling Room

1. Create another new empty scene and name it `StylingRoom`.
2. Run **Dress To Impress → Setup Scene — Styling Room** from the menu bar. This builds the full hierarchy including the camera, judge panel, clothing panel, and top bar.
3. Select the `JudgeManager` GameObject. Drag your JudgeData assets into the **Judges** list. (See Section 5 for how to create judges.)
4. Save the scene.

**Camera note:** The Styling Room camera uses `Orthographic Size = 5.5` to leave room for the clothing panel at the bottom. The `[CharacterRoot]` is positioned at world `(4.08, −3.36, 0)` so the character body is centered in the visible area. **Do not move the CharacterRoot** — its offset is what makes the canvas-coordinate positioning system work correctly with this smaller camera.

### Add Both Scenes to the Build

5. Go to **File → Build Settings**.
6. Drag both `CharacterCreation` and `StylingRoom` scenes into the **Scenes In Build** list. `CharacterCreation` should be index 0.

---

## 5. Creating Judges

Each judge is a small data file called a **JudgeData** asset.

1. In the Project window, right-click in the folder where you want to store your judges (e.g. `Assets/DressToImpress/Data/Judges/`).
2. Choose **Create → Dress To Impress → Judge Data**.
3. Rename the asset to something recognizable, like `Judge_MarcellaVoss`.
4. Select it and fill in the Inspector fields:

| Field | What to put here |
|---|---|
| **Judge Name** | Display name shown on screen (e.g. "Marcella Voss") |
| **Style Tag** | One word that defines their vibe (e.g. "Gothic", "Glam", "Bold") |
| **Avatar Sprite** | Drag in a portrait illustration of the judge |
| **Accent Color** | A color used to theme the UI for this judge |
| **Prompt Text** | The flavour sentence the judge says at round start (e.g. "Darker. Lacier. More drama.") |
| **Theme Tags** | Words that match clothing items (e.g. "Gothic", "Lace", "Dark") — these must match the Theme Tags on your ClothingItemData assets |
| **Dialogue Excellent** | 1–3 lines of excited praise for a great outfit |
| **Dialogue Good** | 1–3 lines of mild approval |
| **Dialogue Poor** | 1–3 lines of disappointment |
| **Money Reward Excellent** | Amount awarded for a high score (default 200) |
| **Money Reward Good** | Amount awarded for a mid score (default 100) |
| **Money Reward Poor** | Amount awarded for a low score (default 25) |

5. Once you've created all your judges, drag them into `JudgeManager`'s **Judges** list in the Styling Room scene.

**Tip:** Theme Tags are matched case-insensitively. A judge with theme tag "beach" will match a clothing item with theme tag "Beach" or "BEACH".

---

## 6. Adding a New Clothing Category

Follow these steps to add a brand-new category (e.g. "Gloves") from scratch — both in the art pipeline and in Unity.

### Step 1 — Add the enum value

Open `Assets/DressToImpress/Scripts/Data/ClothingCategory.cs` and add your new value:

```csharp
public enum ClothingCategory
{
    // ... existing values ...
    Gloves,      // ← add here
}
```

The order of values determines the sorting layer order, so place it logically (e.g. near Accessories).

### Step 2 — Assign a sorting order in CharacterDisplay

Open `Assets/DressToImpress/Scripts/Game/CharacterDisplay.cs`. Find the `SortingOrders` dictionary and add an entry:

```csharp
{ ClothingCategory.Gloves, 38 },  // renders above Outerwear (35), below Ears (40)
```

Higher numbers render in front. Choose a value that makes sense for layering (e.g. gloves go over an outerwear jacket but under earrings).

### Step 3 — Add the Krita folder mapping

Open `Assets/dressAssets/export_layers.py`. Find the `FOLDER_MAP` dictionary and add a line:

```python
("clothes", "gloves"):  "Assets/Art/Clothing/Gloves",
```

The first string in the tuple is the **top-level group name** in Krita (lowercase). The second is the **sub-group name** inside it. The value is where the PNGs will be written.

### Step 4 — Add the folder-to-category mapping in ClothingImporter

Open `Assets/DressToImpress/Scripts/Editor/ClothingImporter.cs`. Find `TryGetCategory` and add:

```csharp
if (path.Contains("Art/Clothing/Gloves")) { category = ClothingCategory.Gloves; return true; }
```

This must appear **before** the `category = default; return false;` line at the end.

### Step 5 — (Optional) Add exclusivity rules

If equipping this item should automatically remove another category (like how Dress removes Top + Bottom), open `CharacterDisplay.cs` and update the `EquipItem` method's exclusivity block.

### Step 6 — (Optional) Add it to the clothing panel

Open the Styling Room scene, select the `ClothingPanel` GameObject, and find **Visible Categories** on the `ClothingPanelManager` component. Add `Gloves` to the list. This adds a new tab in the clothing selection UI.

### Step 7 — Draw the art in Krita

In `mainBase.kra`, create a sub-group named `gloves` inside the `clothes` group:

```
clothes/
└── gloves/           ← matches the "gloves" key in FOLDER_MAP
    └── glove1/       ← item group
        ├── glove1_line
        ├── glove1_color1
        └── glove1_color2
```

Paint your glove art on the shared 2048 × 2048 canvas, positioned where it should sit on the character body.

### Step 8 — Export and import

1. Run the export script from Krita (Tools → Scripts → Run Script → `export_layers.py`).
2. Back in Unity, open **Dress To Impress → Import Clothing Assets** and click **Scan & Import All**.
3. ClothingItemData assets appear in `Assets/DressToImpress/Data/Gloves/`.
4. Add those assets to `ClothingPanelManager`'s **All Clothing Items** list in the Styling Room scene.

---

## 7. Tuning Clothing Scores

After running the importer, find your ClothingItemData assets in `Assets/DressToImpress/Data/`. Select any one to see its fields.

The two fields that affect scoring are:

- **Style Score** — the base number of points this item adds to an outfit (default is 10). Raise this for rare or special items.
- **Theme Tags** — a list of style words (like "beach", "formal", "summer"). When the active judge has a matching word in their own Theme Tags, this item earns **bonus points**.

For example: if a judge has theme tag `"beach"` and a shirt has theme tag `"beach"`, that shirt earns bonus points. If the shirt has `"formal"` instead, no bonus — but it still earns base points just for being worn.

---

## 8. Customising the Scoring

Select the `OutfitScorer` GameObject in the Styling Room scene. The Inspector fields let you tune how scoring works:

| Field | What it does | Default |
|---|---|---|
| **Base Points Per Item** | Points for wearing any item, regardless of theme | 10 |
| **Theme Bonus Per Match** | Extra points when an item's tags match the judge's tags | 15 |
| **Complete Outfit Bonus** | Bonus points if a Hat, Top, Bottom, AND Shoes are all worn | 10 |
| **Min Score** | The lowest score possible (even with nothing equipped) | 0 |
| **Max Score** | Score is clamped to this ceiling | 100 |
| **Excellent Threshold** | Score needed to earn an Excellent reaction | 75 |
| **Good Threshold** | Score needed to earn a Good reaction | 50 |

**Note:** `JudgeManager` has its own copy of **Excellent Threshold** and **Good Threshold** — these control which dialogue lines play and how much money is awarded. Keep them in sync with `OutfitScorer`'s thresholds so the score text and the judge's reaction match.

---

## 9. EGTK Event Wiring Tips

This template is built on EGTK, so you can connect components visually in the Inspector without writing any code. Here are the most useful connections:

| Wire this event... | To this method... | What happens |
|---|---|---|
| `JudgeManager` → **onMoneyAwarded** | `GameCollectionManager` → `Increment` | Adds the money reward to the player's total |
| `JudgeManager` → **onJudgeDialogue** | `ActionDisplayText` → `DisplayText` | Shows the judge's reaction line on screen |
| `JudgeManager` → **onPromptSet** | A TMP Text field | Displays the judge's prompt at round start |
| `JudgeManager` → **onAvatarSet** | A UI Image | Swaps the judge portrait |
| `OutfitScorer` → **onRatingExcellent** | `GameAudioManager` → a fanfare clip | Plays a cheer sound on a great score |
| `OutfitScorer` → **onRatingPoor** | `GameAudioManager` → a buzzer clip | Plays a sad sound on a poor score |
| `OutfitScorer` → **onScoreCalculated** | `GameCollectionManager` → `SetValue` | Displays the score number |
| `StylingRoomManager` → **onRoundStart** | `ActionAnimateTransform` | Bounces the character in at the start of each round |
| `StylingRoomManager` → **onRoundEnd** | An animator trigger | Plays a "thinking" animation on the judge |

To wire an event: select the GameObject with the event, find it in the Inspector, click **+**, drag the target GameObject into the slot, then choose the method from the dropdown.

---

## What's Next?

- Add more clothing items by drawing them in Krita and re-running the export + import
- Create more judges with different personalities and theme tags
- Wire `JudgeManager.onAllJudgesServed` to show a "game over" or summary screen when all judges have been served
- Check the **Component Reference** doc for a full list of every field and event available
