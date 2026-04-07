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

All the character art is drawn in a single Krita file. The export script slices it into individual sprites automatically.

### Step-by-step

**In Krita:**

1. Open **`mainBase.kra`** (found in the project's art folder).
2. Add your clothing artwork as layers inside the correct group. The groups follow this structure:
   - `Clothes/Hats` — hats and headwear
   - `Clothes/Tops` — shirts, jackets, blouses
   - `Clothes/Bottoms` — pants, shorts
   - `Clothes/Skirts`
   - `Clothes/Dresses`
   - `Clothes/Shoes`
   - `Clothes/SocksLeggings`
   - `Clothes/Outerwear`
   - `Clothes/Accessories`
   - `Body/BodyBase` — body shapes / skin tones
   - `Body/FrontHair`, `Body/BackHair` — hair layers
   - `Body/Eyes`, `Body/Eyebrows`, `Body/Mouths`, `Body/Ears`, `Body/Noses`
3. Name each layer clearly — the name becomes the item's display name in the game.
4. Open **Tools → Scripter** in Krita, load `export_layers.py`, and click **Run Script**. When prompted, point it at your Unity project folder. It will export every layer as a separate PNG and write a JSON file that Unity reads during import.

**In Unity:**

5. Switch back to Unity (it may reimport automatically).
6. From the top menu bar, go to **Dress To Impress → Import Clothing Assets**.
7. Unity reads the exported JSON and automatically creates a **ClothingItemData** asset for every clothing layer. You'll find them in `Assets/DressToImpress/Data/`.

That's it — no manual sprite-slicing needed.

---

## 4. Setting Up the Scenes

### Scene 1: Character Creation

1. Create a new empty scene (**File → New Scene**) and name it `CharacterCreation`.
2. Run **Dress To Impress → Setup Scene — Character Creation** from the menu bar. This adds all the required GameObjects.
3. Select the `CharacterCreatorManager` GameObject in the Hierarchy.
4. In the Inspector, find the arrays for **Body Types**, **Front Hairs**, **Back Hairs**, **Eyes**, **Eyebrows**, **Mouths**, **Ears**, and **Noses**. Drag the matching ClothingItemData assets from `Assets/DressToImpress/Data/` into each array.
5. Save the scene.

### Scene 2: Styling Room

1. Create another new empty scene and name it `StylingRoom`.
2. Run **Dress To Impress → Setup Scene — Styling Room** from the menu bar.
3. Select the `ClothingPanelManager` GameObject. Drag all your clothing ClothingItemData assets into the **All Clothing Items** list.
4. Select the `JudgeManager` GameObject. Drag your JudgeData assets into the **Judges** list. (See Section 5 for how to create judges.)
5. Save the scene.

### Add Both Scenes to the Build

6. Go to **File → Build Settings**.
7. Drag both `CharacterCreation` and `StylingRoom` scenes into the **Scenes In Build** list. `CharacterCreation` should be index 0.

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

## 6. Tuning Clothing Scores

After running the importer, find your ClothingItemData assets in `Assets/DressToImpress/Data/`. Select any one to see its fields.

The two fields that affect scoring are:

- **Style Score** — the base number of points this item adds to an outfit (default is 10). Raise this for rare or special items.
- **Theme Tags** — a list of style words (like "beach", "formal", "summer"). When the active judge has a matching word in their own Theme Tags, this item earns **bonus points**.

For example: if a judge has theme tag `"beach"` and a shirt has theme tag `"beach"`, that shirt earns bonus points. If the shirt has `"formal"` instead, no bonus — but it still earns base points just for being worn.

---

## 7. Customising the Scoring

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

## 8. EGTK Event Wiring Tips

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

- Add more clothing items by drawing them in Krita and re-running the importer
- Create more judges with different personalities and theme tags
- Wire `JudgeManager.onAllJudgesServed` to show a "game over" or summary screen when all judges have been served
- Check the **Component Reference** doc for a full list of every field and event available
