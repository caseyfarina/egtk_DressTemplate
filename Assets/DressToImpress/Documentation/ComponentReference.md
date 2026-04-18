# Dress To Impress — Component Reference

A reference guide for every custom script in this template. Use the summary table below to find the right component, then jump to its section for the full details.

---

## Summary Table

| Component | What it does |
|---|---|
| **CharacterDisplay** | Renders the dressed character using one sprite layer per clothing slot |
| **CharacterCreator** | Drives the Character Creation scene — prev/next selection rows, saves profile |
| **JudgeManager** | Manages the judge queue, fires presentation events, and reacts to outfit scores |
| **OutfitScorer** | Scores the current outfit against the judge's theme and fires rating events |
| **StylingRoomManager** | Top-level coordinator that runs the round loop in the Styling Room scene |
| **ClothingPanelManager** | Builds and manages the bottom clothing selection panel with category tabs |
| **ClothingItemData** | ScriptableObject asset that stores all data for one clothing item |
| **JudgeData** | ScriptableObject asset that defines one judge's personality and rewards |

---

## CharacterDisplay

> Renders the dressed character by managing one SpriteRenderer layer per clothing slot, and positions each sprite using the pixel coordinates stored in the ClothingItemData asset.

**Where it lives:** Styling Room and Character Creation scenes / `CharacterDisplay` GameObject

### Key Inspector Fields

| Field | What it does |
|---|---|
| **Pixels Per Unit** | Must match the Pixels Per Unit setting on your imported sprites. Default 100. |
| **Canvas Width / Canvas Height** | Pixel dimensions of the Krita source canvas. Default 2048 × 2048. Keep these matching your actual canvas size. |
| **Layer Root** | Optional: a child Transform to parent all the generated sprite layers under. Leave empty to use the CharacterDisplay's own Transform. |
| **Body Types** | Array of ClothingItemData assets for body shape options (used in Character Creation). |
| **Front Hairs** | Array of ClothingItemData assets for front hair options. |
| **Back Hairs** | Array of ClothingItemData assets for back hair options. |
| **Eyes** | Array of ClothingItemData assets for eye options. |
| **Eyebrows** | Array of ClothingItemData assets for eyebrow options. |
| **Mouths** | Array of ClothingItemData assets for mouth options. |
| **Ears** | Array of ClothingItemData assets for ear options. |
| **Noses** | Array of ClothingItemData assets for nose options. |

### Events You Can Wire

| Event | When it fires | Wire it to... |
|---|---|---|
| **onItemEquipped** | When any item is equipped (passes the ClothingItemData) | A sound effect, a "new item" animation |
| **onItemUnequipped** | When a category is cleared (passes the category) | A sound effect, resetting a highlight |

### Methods You Can Call from UnityEvents

| Method | What it does |
|---|---|
| `EquipItem(ClothingItemData)` | Puts an item onto the character. Handles Dress/Top/Bottom/Skirt exclusivity automatically. |
| `UnequipCategory(ClothingCategory)` | Clears a specific clothing layer (removes its sprite). |
| `ToggleItem(ClothingItemData)` | If the item is equipped, removes it. If it isn't, equips it. |
| `UnequipAllClothing()` | Clears all clothing layers at once (Hat through Accessory). Does not touch face or hair layers. |
| `ApplyProfile(CharacterProfile)` | Applies a saved character profile (face and hair choices) to the display. |
| `SetBodyType(ClothingItemData)` | Sets the body base layer specifically. |

---

## CharacterCreator

> Manages the row-by-row selection UI in the Character Creation scene, saves the player's choices to a persistent profile, and loads the Styling Room when the player is ready.

**Where it lives:** Character Creation scene / `CharacterCreatorManager` GameObject

### Key Inspector Fields

| Field | What it does |
|---|---|
| **Character Display** | Drag in the CharacterDisplay component so the preview updates as the player makes choices. |
| **Body Types** | Array of ClothingItemData assets for body type options. |
| **Front Hairs** | Array of ClothingItemData assets for front hair options. |
| **Back Hairs** | Array of ClothingItemData assets for back hair options. |
| **Eyes** | Array of ClothingItemData assets for eye options. |
| **Eyebrows** | Array of ClothingItemData assets for eyebrow options. |
| **Mouths** | Array of ClothingItemData assets for mouth options. |
| **Ears** | Array of ClothingItemData assets for ear options. |
| **Noses** | Array of ClothingItemData assets for nose options. |
| **Styling Room Scene Name** | The exact name of the Styling Room scene to load when the player clicks Start. Default: `"StylingRoom"`. |

### Events You Can Wire

| Event | When it fires | Wire it to... |
|---|---|---|
| **onCreationComplete** | Fires just before the scene transitions to the Styling Room | A transition animation, a fade-to-black effect |

### Methods You Can Call from UnityEvents

The setup script wires the **Previous** and **Next** arrow buttons in each selection row automatically. The `Next*` / `Prev*` methods wrap around (going past the last item cycles back to the first, and vice versa). The `Select*(int)` methods set an explicit index — useful for resetting or applying a saved profile.

| Method | What it does |
|---|---|
| `NextBodyType()` / `PrevBodyType()` | Advance or retreat through body type options, wrapping at the ends. |
| `NextFrontHair()` / `PrevFrontHair()` | Cycle through front hair options. |
| `NextBackHair()` / `PrevBackHair()` | Cycle through back hair options. |
| `NextEyes()` / `PrevEyes()` | Cycle through eye options. |
| `NextEyebrows()` / `PrevEyebrows()` | Cycle through eyebrow options. |
| `NextMouth()` / `PrevMouth()` | Cycle through mouth options. |
| `NextEars()` / `PrevEars()` | Cycle through ear options. |
| `NextNose()` / `PrevNose()` | Cycle through nose options. |
| `SelectBodyType(int)` | Sets the body type to the given index directly. |
| `SelectFrontHair(int)` | Sets the front hair to the given index directly. |
| `SelectBackHair(int)` | Sets the back hair to the given index directly. |
| `SelectEyes(int)` | Sets the eyes to the given index directly. |
| `SelectEyebrows(int)` | Sets the eyebrows to the given index directly. |
| `SelectMouth(int)` | Sets the mouth to the given index directly. |
| `SelectEars(int)` | Sets the ears to the given index directly. |
| `SelectNose(int)` | Sets the nose to the given index directly. |
| `ApplyAllSelections()` | Re-applies all current selections to the display. Useful after loading a saved profile. |
| `OnStartStyling()` | Saves the profile and loads the Styling Room scene. Wire this to the START STYLING button. |

---

## JudgeManager

> Manages the queue of judges, presents each judge's name/prompt/avatar at the start of a round, and reacts to the outfit score with dialogue and a money reward.

**Where it lives:** Styling Room scene / `JudgeManager` GameObject

### Key Inspector Fields

| Field | What it does |
|---|---|
| **Judges** | The list of JudgeData assets to use. Add as many as you like. |
| **Shuffle Judges** | If ticked, the judge order is randomised at the start of each game. |
| **Avoid Repeat** | If ticked, the same judge won't appear twice in a row. |
| **Max Rounds** | How many rounds before `onAllJudgesServed` fires. Set to 0 for unlimited. |
| **Excellent Threshold** | Score needed for an Excellent rating (and Excellent money reward). Default 75. |
| **Good Threshold** | Score needed for a Good rating. Default 50. |

### Events You Can Wire

| Event | When it fires | Wire it to... |
|---|---|---|
| **onJudgeReady** | When a new judge is about to be introduced | An entrance animation, a "ding" sound |
| **onJudgeNameSet** | Passes the judge's name as a string | A TMP Text component to display the name |
| **onStyleTagSet** | Passes the judge's style tag (e.g. "Bold") | A TMP Text label for the style category |
| **onPromptSet** | Passes the judge's prompt sentence | `ActionDisplayText.DisplayText`, or a TMP Text field |
| **onAvatarSet** | Passes the judge's portrait sprite | A UI Image component |
| **onJudgeDialogue** | Fires once per reaction line (passes the string) | `ActionDisplayText.DisplayText` to show the judge's words |
| **onMoneyAwarded** | Passes the money amount as an int after scoring | `GameCollectionManager.Increment` to add money to the total |
| **onRatingDetermined** | Passes the OutfitRating enum value | An animator to trigger a specific reaction animation |
| **onAllJudgesServed** | Fires when Max Rounds is reached | A "game complete" screen, a score summary panel |

### Methods You Can Call from UnityEvents

| Method | What it does |
|---|---|
| `PresentNextJudge()` | Picks and displays the next judge. Fires all the onJudge* events. Normally called automatically by StylingRoomManager. |
| `ReactToScore(int)` | Given a score number, determines the rating, fires dialogue and money events. Normally called automatically by StylingRoomManager. |

---

## OutfitScorer

> Looks at what's currently equipped on the CharacterDisplay, compares the items' theme tags against the active judge's style, and produces a score from 0 to 100.

**Where it lives:** Styling Room scene / `OutfitScorer` GameObject

### Key Inspector Fields

| Field | What it does |
|---|---|
| **Character Display** | The CharacterDisplay to read equipped items from. **Must be assigned.** |
| **Judge Manager** | The JudgeManager to read the active judge's theme from. **Must be assigned.** |
| **Base Points Per Item** | Points earned per item worn, regardless of theme. Default 10. |
| **Theme Bonus Per Match** | Extra points when an item's theme tag matches the judge's tag. Default 15. |
| **Complete Outfit Bonus** | Bonus points if Hat + Top + Bottom + Shoes are all worn simultaneously. Default 10. |
| **Min Score** | The floor — score can never go below this. Default 0. |
| **Max Score** | The ceiling — score is clamped to this. Default 100. |
| **Excellent Threshold** | Score needed to fire `onRatingExcellent`. Default 75. |
| **Good Threshold** | Score needed to fire `onRatingGood`. Default 50. |

### Events You Can Wire

| Event | When it fires | Wire it to... |
|---|---|---|
| **onScoreCalculated** | After CalculateScore runs — passes the final integer score | `GameCollectionManager.SetValue` to display the score, or `JudgeManager.ReactToScore` |
| **onRatingExcellent** | Score is 75 or above (by default) | A fanfare sound, confetti particle effect |
| **onRatingGood** | Score is 50–74 (by default) | A cheerful chime |
| **onRatingPoor** | Score is below 50 (by default) | A sad sound, a wince animation |

### Methods You Can Call from UnityEvents

| Method | What it does |
|---|---|
| `CalculateScore()` | Scores the current outfit and fires all score/rating events. Normally called automatically by StylingRoomManager when the player submits. |
| `PreviewScore()` | Calculates and **returns** the score without firing any events. Useful for showing a live score preview. |

---

## StylingRoomManager

> Top-level coordinator for the Styling Room. Runs the round loop: present judge → player dresses character → player submits → score → judge reacts → next round.

**Where it lives:** Styling Room scene / `StylingRoomManager` GameObject

### Key Inspector Fields

| Field | What it does |
|---|---|
| **Character Display** | The CharacterDisplay to reset between rounds. **Must be assigned.** |
| **Judge Manager** | The JudgeManager to call PresentNextJudge and ReactToScore on. **Must be assigned.** |
| **Outfit Scorer** | The OutfitScorer to call CalculateScore on when the player submits. **Must be assigned.** |
| **Clothing Panel** | The ClothingPanelManager to lock/unlock during submission. |
| **Boutique Scene Name** | Scene to load when the player clicks BOUTIQUE. Default `"Boutique"`. |
| **Main Menu Scene Name** | Scene to load when the player clicks MAIN MENU. Default `"MainMenu"`. |
| **Score Reaction Delay** | Seconds to wait after scoring before the judge's dialogue appears. This gives score animations time to play. Default 1.5 seconds. |

### Events You Can Wire

| Event | When it fires | Wire it to... |
|---|---|---|
| **onRoundStart** | At the beginning of each round, after the judge is presented | A character entrance animation, a "round start" sound |
| **onRoundEnd** | When the player clicks SUBMIT OUTFIT, before scoring | A "thinking" animation on the judge, a drumroll sound |
| **onScoreReady** | Passes the calculated score as an int | A score popup animator, a TMP score display |

### Methods You Can Call from UnityEvents

Wire these to your UI buttons:

| Method | What it does |
|---|---|
| `OnSubmitOutfit()` | The player submits the current outfit. Locks the clothing panel, scores the outfit, starts the reaction sequence. Wire to the SUBMIT OUTFIT button. |
| `OnNextJudge()` | Clears the outfit, re-enables the panel, and presents the next judge. Wire to the NEXT button. |
| `OnGoToBoutique()` | Loads the Boutique scene. Wire to the BOUTIQUE button. |
| `OnGoToMainMenu()` | Returns to the Main Menu scene. Wire to the MAIN MENU button. |

---

## ClothingPanelManager

> Builds and manages the clothing selection UI panel at the bottom of the Styling Room — a row of category tabs (HATS, TOPS, etc.) and a scrollable grid of item buttons below.

**Where it lives:** Styling Room scene / `ClothingPanel` GameObject

### Key Inspector Fields

| Field | What it does |
|---|---|
| **Character Display** | The CharacterDisplay to equip/unequip items on. **Must be assigned.** |
| **Tab Button Prefab** | A prefab with a Button component and a TMP_Text child named `"Label"` for category tabs. |
| **Item Button Prefab** | A prefab with a Button component, an Image child for the icon, and a TMP_Text child named `"Label"` for item buttons. |
| **Tab Container** | The Transform where tab buttons are created (typically a horizontal layout group). |
| **Item Container** | The Content Transform inside a ScrollRect where item buttons are created. |
| **Visible Categories** | Which clothing categories get tabs. You can reorder or remove categories here. |
| **All Clothing Items** | **Drag all your ClothingItemData assets here.** The panel reads this list to build item buttons. |
| **Active Tab Color** | Text/image color for the currently selected tab. Default yellow. |
| **Inactive Tab Color** | Text/image color for unselected tabs. Default white. |
| **Selected Item Color** | Button tint when an item is currently equipped. Default light green. |
| **Default Item Color** | Button tint when an item is not equipped. Default white. |

### Events You Can Wire

| Event | When it fires | Wire it to... |
|---|---|---|
| **onCategoryChanged** | When the player switches to a different tab (passes the category) | A panel slide animation, a click sound |
| **onItemSelected** | When the player clicks any item button (passes the ClothingItemData) | A "try on" sound effect |

### Methods You Can Call from UnityEvents

| Method | What it does |
|---|---|
| `ShowCategory(ClothingCategory)` | Switches the panel to show items for the given category. |
| `RefreshButtonStates()` | Updates button highlight colors to match the current equipped state. |
| `SetInteractable(bool)` | Pass `true` to enable buttons, `false` to disable them all. Used during outfit submission. |
| `SetAvailableItems(ClothingItemData[])` | Replaces the item list and rebuilds the panel. Use this when new items are unlocked in the Boutique. |
| `Initialize()` | Rebuilds the entire panel from scratch. |

---

## ClothingItemData

> A ScriptableObject data asset that stores everything Unity needs to know about one clothing item: the sprite, where it sits on the character canvas, its category slot, and its scoring metadata.

**Create via:** Right-click in Project → **Create → Dress To Impress → Clothing Item Data**

(In practice, the importer creates these automatically. You mainly need to open them to adjust **Style Score** and **Theme Tags**.)

**Where they live:** `Assets/DressToImpress/Data/` (auto-created by the importer)

### Inspector Fields

| Field | What it does |
|---|---|
| **Item Name** | Display name shown on the clothing button in the game UI. |
| **Sprite** | The sprite displayed on the character when this item is equipped. |
| **Category** | Which layer slot this item occupies (Hat, Top, Bottom, Skirt, Dress, Shoes, SocksLeggings, Outerwear, Accessory, or face/hair categories). |
| **Canvas X / Canvas Y** | The pixel position of this item's top-left corner on the Krita canvas. Set by the importer — you usually won't change these. |
| **Color Variant Index** | Which color variant this item is within its group (set by the importer). |
| **Group Name** | The shared base name that groups color variants of the same shape (e.g. `"beret1"`). Set by the importer. |
| **Style Score** | Base points this item adds to an outfit score. Default 10. Increase for rarer or fancier items. |
| **Theme Tags** | Style words that judges match against. Examples: `"beach"`, `"formal"`, `"gothic"`, `"glam"`. Matched case-insensitively. |

**Note on categories:** Equipping a **Dress** automatically removes any equipped Top, Bottom, and Skirt. Equipping a Top, Bottom, or Skirt automatically removes any equipped Dress. This exclusivity is handled by CharacterDisplay.

---

## JudgeData

> A ScriptableObject data asset that defines one judge — their personality, style preferences, the dialogue they say for each rating, and how much money they award.

**Create via:** Right-click in Project → **Create → Dress To Impress → Judge Data**

**Where they live:** Anywhere in your project — recommended: `Assets/DressToImpress/Data/Judges/`

### Inspector Fields

| Field | What it does |
|---|---|
| **Judge Name** | Display name shown on screen (e.g. "Marcella Voss"). |
| **Style Tag** | A single word that labels this judge's style (e.g. "Gothic", "Glam", "Bold"). Shown in the UI. |
| **Avatar Sprite** | Portrait illustration displayed alongside the judge's dialogue. |
| **Accent Color** | A color for UI theming elements associated with this judge. |
| **Prompt Text** | The sentence the judge says at the start of a round (e.g. "I need something that pops!"). |
| **Theme Tags** | Style words that clothing items are matched against for bonus scoring. Must match the Theme Tags on ClothingItemData assets. |
| **Dialogue Excellent** | 1–3 lines of excited praise for a high-scoring outfit. |
| **Dialogue Good** | 1–3 lines of mild approval for a mid-scoring outfit. |
| **Dialogue Poor** | 1–3 lines of disappointment for a low-scoring outfit. |
| **Money Reward Excellent** | Money awarded for an Excellent rating. Default 200. |
| **Money Reward Good** | Money awarded for a Good rating. Default 100. |
| **Money Reward Poor** | Money awarded for a Poor rating. Default 25. |

**Tip:** Write dialogue that matches the judge's personality. A tough fashion critic might say "Barely passable. I've seen better on a mannequin." for a Good score, while a warm mentor might say "Getting there! I like where your head's at."

---

## Troubleshooting

### "My clothing isn't showing up in the panel"

- Make sure you ran **Dress To Impress → Import Clothing Assets** from the menu after exporting from Krita.
- Select the ClothingItemData asset and check that the **Sprite** field is not empty.
- Make sure the asset's **Category** is one of the categories listed in `ClothingPanelManager`'s **Visible Categories** array.
- Check that the asset is in the **All Clothing Items** list on `ClothingPanelManager`.

### "The judge panel is blank / no judge appears"

- Select the `JudgeManager` GameObject in the Styling Room scene. Make sure at least one JudgeData asset is in the **Judges** array.
- Check that `StylingRoomManager`'s **Judge Manager** field is assigned (drag the JudgeManager GameObject in).

### "Clothing items appear in the wrong position on the character"

- Make sure the **Pixels Per Unit** on `CharacterDisplay` matches the **Pixels Per Unit** set in the sprite's import settings (the default for both is 100).
- Canvas X and Canvas Y on each ClothingItemData are set by the importer. If items look shifted, re-run the export from Krita and re-run the importer.
- The two scenes use **different camera setups** — do not mix them up:
  - **Character Creation**: `Orthographic Size = 10.24`, `[CharacterRoot]` at world `(0, 0, 0)`.
  - **Styling Room**: `Orthographic Size = 5.5`, `[CharacterRoot]` at world `(4.08, −3.36, 0)`. The offset is intentional — it compensates for the body art being positioned left-of-center on the 2048 × 2048 canvas so the character is centered in the smaller camera view.
- If `[CharacterRoot]` is at the wrong position in the Styling Room, run **Dress To Impress → Setup Scene — Styling Room** to rebuild with the correct values.

### "Clicking clothing buttons does nothing"

- Select `ClothingPanelManager` in the Inspector. Check that the **Character Display** field is assigned.
- If buttons are grayed out, check that `StylingRoomManager.OnNextJudge` has been called (it re-enables buttons). You may need to make sure the NEXT button is wired correctly.

### "The score is always 0"

- Select `OutfitScorer` and check that both **Character Display** and **Judge Manager** are assigned.
- Make sure the clothing items have been equipped (score is 0 if no items are worn).
- Open the Console (**Window → General → Console**) and look for any warning messages from `[OutfitScorer]`.

### "onMoneyAwarded fires but my GameCollectionManager balance doesn't change"

- Make sure `JudgeManager.onMoneyAwarded` is wired to `GameCollectionManager.Increment` (not `SetValue`). `Increment` adds to the current value; `SetValue` replaces it.
- Check that the GameCollectionManager is in the same scene as the JudgeManager, or uses Scene Persistence if it lives in a different scene.
