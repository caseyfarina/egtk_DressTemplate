# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

**⚠️ IMPORTANT: Keep this file in the `main` branch.** CLAUDE.md is project-wide documentation that should be visible regardless of which branch is checked out. When documenting feature branches, add the documentation here in main (with clear branch labels), not in the feature branch itself.

## Quick Links

- **[Program Flow Diagram](Assets/DressToImpress/Documentation/ProgramFlow.md)** - Mermaid flowchart of all class interactions
- **[Component Reference](Assets/DressToImpress/Documentation/ComponentReference.md)** - DressToImpress component guide
- **[Quick Start](Assets/DressToImpress/Documentation/QuickStart.md)** - Setup and usage guide

## Project Overview

**egtk_DressTemplate — Dress-To-Impress Game Template**

This is a standalone Unity project and single git repository that implements a dress-up game template built on top of the eventGameToolKit (egtk) framework. It is a **template** — intended as a starting point for dress-up style games.

- **Git Repo**: `https://github.com/caseyfarina/egtk_DressTemplate` (single repo, branch: `main`)
- **Unity Version**: Unity 6 (6000.0.x or later)
- **Render Pipeline**: Universal Render Pipeline (URP)
- **Animation Engine**: DOTween FREE

### Two-Scene Flow

| Scene | Purpose |
|---|---|
| **Character Creation** | Player customizes body type, hair, and facial features |
| **Styling Room** | Player selects clothing to match judge themes and earns a score |

### Core Scripts (`Assets/DressToImpress/Scripts/`)

| Folder | Scripts | Responsibility |
|---|---|---|
| `Data/` | `ClothingCategory`, `ClothingItemData`, `JudgeData`, `CharacterProfile` | ScriptableObjects and shared enums |
| `Game/` | `CharacterDisplay`, `CharacterCreator`, `StylingRoomManager`, `OutfitScorer`, `JudgeManager`, `DressUpThemeManager`, `DressUpSlot`, `ClothingItem` | Core game logic |
| `Input/` | `InputSpriteClick`, `InputSpriteDrag` | 2D sprite interaction |
| `UI/` | `ClothingPanelManager` | Category tabs + item grid UI |
| `Editor/` | `ClothingImporter`, `SceneSetup_CharacterCreation`, `SceneSetup_StylingRoom` | Editor tools |

### Key Design Notes

- `ClothingCategory` enum is the **single authoritative source** defined in `Data/ClothingCategory.cs` — do not redefine it in other files.
- `CharacterProfile` is a runtime singleton (never serialized) that carries character appearance choices from Scene 1 to Scene 2.
- `OutfitScorer` scoring: `(items × basePoints) + (themeMatches × themeBonus) + completeOutfitBonus`, clamped to [0, 100].
- `ClothingImporter` auto-generates `ClothingItemData` assets from PNGs named `{item}_color{N}_x{X}_y{Y}.png` in `Assets/Art/`.
- Scene hierarchies can be auto-built via **Dress To Impress** menu items in the Unity Editor (the two `SceneSetup_` editor scripts).
- **Styling Room camera** uses `orthographicSize = 5.5`. `[CharacterRoot]` is positioned at world `(4.08, -3.36, 0)` to center the body art (which is painted in the upper-left quadrant of the 2048×2048 Krita canvas) in the camera view.
- `CharacterDisplay.NonClothingCategories` is a static `HashSet` listing face/hair categories excluded from scoring and `UnequipAllClothing`. All other enum values are treated as scoreable clothing slots automatically — **do not add new clothing categories to this set**.
- `JudgeManager` events `onJudgeNameSet`, `onStyleTagSet`, `onPromptSet`, and `onJudgeDialogue` are wired to their TMP fields by `SceneSetup_StylingRoom` automatically on rebuild. `onAvatarSet` → `[AvatarImage].sprite` and `onMoneyAwarded` → money display must be wired manually in the Inspector (property-setter limitation).

## Git Repository

This is a **single-repository project** — no sync steps required.

- **Remote**: `https://github.com/caseyfarina/egtk_DressTemplate`
- **Branch**: `main`
- **Push**: `git push origin HEAD:main`

Standard workflow: make changes → test in Unity → commit → push.

## Project Structure

```
Assets/
├── DressToImpress/
│   ├── Scripts/
│   │   ├── Data/          # Enums and ScriptableObjects (ClothingCategory, ClothingItemData, JudgeData, CharacterProfile)
│   │   ├── Game/          # Core game logic (CharacterDisplay, OutfitScorer, JudgeManager, etc.)
│   │   ├── Input/         # 2D sprite interaction (InputSpriteClick, InputSpriteDrag)
│   │   ├── UI/            # Clothing panel UI (ClothingPanelManager)
│   │   └── Editor/        # Editor tools (ClothingImporter, SceneSetup scripts)
│   └── Documentation/
│       ├── ProgramFlow.md         # Mermaid flowchart of full system
│       ├── ComponentReference.md  # Component guide
│       └── QuickStart.md          # Setup guide
├── Art/                   # Source PNGs for ClothingImporter (naming: {item}_color{N}_x{X}_y{Y}.png)
├── Scenes/                # Game scenes
└── eventGameToolKit/      # egtk framework package (Input System, URP, DOTween, etc.)

## Unity Packages Used

| Package | Version | Purpose |
|---------|---------|---------|
| Input System | 1.11.2 | Modern input handling |
| URP | 17.0.3 | Rendering pipeline |
| Cinemachine | 3.1.2 | Camera management |
| AI Navigation | 2.0.5 | Pathfinding |
| DOTween FREE | - | Animation tweening |
| Adobe Substance 3D | - | Material authoring |

### DOTween FREE Compatibility

**CRITICAL: This project uses DOTween FREE, not DOTween Pro.**

**IMPORTANT: Due to asmdef conflicts, avoid DOTween module-specific extensions in package code.** Use `DOTween.To()` instead - it's just as good and has no assembly reference issues.

| Avoid (asmdef conflicts) | Use Instead |
|--------------------------|-------------|
| `audioSource.DOFade()` | `DOTween.To(() => source.volume, x => source.volume = x, target, duration)` |
| `rigidbody.DOMove()` | `DOTween.To()` or `transform.DOMove()` |
| `spriteRenderer.DOFade()` | `DOTween.To(() => sr.color, x => sr.color = x, target, duration)` |

**Safe to use (core DOTween, no module dependencies):**
- `transform.DOMove()`, `DORotate()`, `DOScale()`, `DOPunchScale()`
- `rectTransform.DOAnchorPos()`
- `canvasGroup.DOFade()`, `image.DOFade()`, `image.DOColor()`
- `DOTween.To()` for any value type (universal, always works)
- `DOTween.Sequence()` for chaining
- `.SetUpdate()`, `.SetEase()`, `.OnComplete()`, `.Kill()`

**DOTween Pro Only (DO NOT USE):**
- `text.DOText()` (TextMesh Pro module)
- Path tweening
- DeAudio, DeUnityExtended

## Educational Design Philosophy

### UnityEvent-Driven Architecture

Students create interactions by wiring UnityEvents in the Inspector:
- **No-Code Approach**: Visual connections replace programming
- **Event Sources**: Input components (triggers, keys, mouse)
- **Event Targets**: Action components (spawn, display, animate)
- **Modular**: Mix and match components to create complex systems

### Core Component Categories

| Category | Description | Example Scripts |
|----------|-------------|-----------------|
| **Input** | Event sources triggered by player or game state | InputKeyPress, InputTriggerZone, InputCheckpointZone |
| **Actions** | Event targets that perform actions | ActionSpawnObject, ActionDisplayText, ActionDialogueSequence |
| **Physics** | Movement, forces, collisions | PhysicsBumper, CharacterControllerCC, PhysicsPlatformAnimator |
| **Game** | Managers for health, score, timer, audio, etc. | GameHealthManager, GameStateManager, GameAudioManager |
| **Puzzle** | Switch and checker mechanics | PuzzleSwitch, PuzzleSwitchChecker, PuzzleSequenceChecker |
| **UI** | User interface helpers | FadeInFromBlackOnRestart |
| **Animation** | Transform animations | ActionAnimateTransform |

## Development Workflow

### Before Pushing to Git — Maintenance Checklist

When adding, renaming, or modifying scripts, update these files before committing:

| What changed | Files to update |
|---|---|
| New script added | `runtime-structure.md`, `ComponentQuickReference.md`, CLAUDE.md script count |
| Script renamed or removed | `runtime-structure.md`, `ComponentQuickReference.md`, `custom-editors.md` (if applicable) |
| New `[SerializeField]` on a script with a custom editor | The editor script (see [Custom Editors Guide](.claude/docs/custom-editors.md)) |
| New custom editor created | `custom-editors.md` table |
| Public API changed | XML doc comments in the script |

**`ComponentQuickReference.md`** (`Assets/eventGameToolKit/Documentation/`) is the student-facing one-page guide. It must stay current — students use it to discover what components exist.

### Adding New Fields to Components

**⚠️ CRITICAL: Check for custom editor scripts before adding fields!**

Some components have custom Inspector UI. When adding `[SerializeField]` fields to these scripts, you must update BOTH:
1. The MonoBehaviour script (`.cs` in Runtime folder)
2. The Editor script (`.cs` in Editor folder)

See **[Custom Editor Scripts Guide](.claude/docs/custom-editors.md)** for the complete list and workflow.

### Code Conventions

- **Naming**: `[Category][Purpose]` format (e.g., `InputKeyPress`, `ActionSpawnObject`)
- **Physics**: Use Unity's `linearVelocity` (new physics API)
- **UI**: TextMeshPro (`TMPro` namespace)
- **Events**: UnityEvents for all student-facing interactions
- **Documentation**: XML comments required for all public methods and UnityEvents

### XML Documentation Requirement

**ALL educational scripts MUST have XML documentation for the Documentation Generator:**

```csharp
/// <summary>
/// Brief description of component purpose
/// </summary>
public class MyComponent : MonoBehaviour
{
    /// <summary>
    /// Fires when the player enters the zone
    /// </summary>
    public UnityEvent onEnter;

    /// <summary>
    /// Manually triggers the enter event
    /// </summary>
    public void TriggerEnter() { }
}
```

See **[Documentation Generator Guide](.claude/docs/documentation-generator.md)** for complete requirements.

## Common Tasks

### Running the Project
1. Open in Unity 6 Editor
2. Open the Character Creation scene or Styling Room scene under `Assets/Scenes/`
3. Press Play (Ctrl+P) to test

### Rebuilding Scenes from Scratch
Use the Unity menu items created by the editor setup scripts:
- **Dress To Impress > Setup Scene — Character Creation**
- **Dress To Impress > Setup Scene — Styling Room**

These auto-build the full hierarchy and wire all component references. After rebuilding the Styling Room scene, two steps remain manual:
1. Wire `JudgeManager.onAvatarSet` → `[AvatarImage]` Image component (Inspector)
2. Wire `JudgeManager.onMoneyAwarded` → your money display (e.g. `GameCollectionManager.Increment`)
3. Assign `JudgeData` ScriptableObjects to `JudgeManager.judges` array
4. Assign `ClothingItemData` assets to `ClothingPanelManager.allClothingItems`

### Importing New Clothing Art
1. Drop PNGs into `Assets/Art/` using naming convention: `{itemName}_color{N}_x{X}_y{Y}.png`
2. Run **Dress To Impress > Import Clothing** (ClothingImporter EditorWindow)
3. Assets are created/updated in `Assets/DressToImpress/Data/{Category}/`

### Building
- Build Settings: Ctrl+Shift+B

## Custom Slash Commands

### `/desk-check <ScriptName>`

Performs a manual trace review (desk check) of a script. Traces every public method with forecasted inputs to verify logic without executing it. Produces per-method trace tables with BUG / EDGE CASE / OK verdicts, checks editor property name matches, and validates flag ordering and event timing. Asks before applying fixes.

**Usage**: `/desk-check GameHealthManager`, `/desk-check ActionDialogueSequence`

## Scene Persistence

Four managers support a `Persist Across Scenes` checkbox. The underlying mechanism is `GameData` — an auto-created runtime ScriptableObject singleton that is completely invisible to students. They just tick the box.

| Manager | Persists | Mechanism | Internal slot |
|---|---|---|---|
| `GameHealthManager` | Optional | `GameData` int slot 0 | Automatic |
| `GameCollectionManager` | Optional | `GameData` int slot 1 | Automatic |
| `GameInventoryManager` | Optional | `GameData` int slots 2–21 | Automatic (max 20 slots) |
| `GameCheckpointManager` | Always | DontDestroyOnLoad singleton | N/A |
| All others | Never | — | — |

**Rules:**
- Add the manager to **each scene** that needs it — only the *value* carries over, not the manager itself. This means event wiring in each scene is local and works normally.
- On the first scene load of a new play session, managers use their own Inspector defaults. On subsequent loads, they read the last written value.
- GameInventoryManager logs a warning if more than 20 slots are configured with persistence enabled.
- `GameData` resets automatically at the start of each play session — no student action needed for "new game."

## Self-Contained UI Pattern

**GameHealthManager**, **GameCollectionManager**, and **GameInventoryManager** all support optional self-contained UI toggled via `showUI`. When enabled, the manager creates its own Canvas at runtime — no GameUIManager wiring needed.

**Important**: Students should use EITHER the individual controls (Option A) OR GameUIManager (Option B), never both on the same manager. Mixing them creates duplicate overlapping displays. See [GameUI_QuickStart.md](Assets/eventGameToolKit/Documentation/GameUI_QuickStart.md) for student guidance.

**GameTimerManager** also supports self-contained UI: a clock text display (with optional gradient color) and a fill bar (with gradient), both with independent positioning controls. Count-down timers automatically use `startTime` as 100%; count-up timers require `totalTime` to be set for bar/gradient to work.

## GameInventoryManager

`GameInventoryManager` (`Runtime/Game/GameInventoryManager.cs`) replaces the single-slot `GameInventorySlot` with a configurable list of inventory slots in a single component.

`GameInventorySlot` has been removed — it's in git history if needed.

### Architecture

```
GameInventoryManager
├── List<InventorySlot> slots    // configurable in Inspector
├── showUI toggle                // self-contained UI (row of icon+count cards)
└── showCount toggle             // optional count number in each card

[Serializable] InventorySlot
├── string itemName
├── Sprite icon (optional)       // shown in UI card
├── int maxCapacity
├── int currentCount
├── UnityEvent onFull            // fires when count reaches maxCapacity
├── UnityEvent onEmpty           // fires when count reaches 0
└── UnityEvent<int> onChanged    // fires on any count change, passes new count
```

### Migration from GameInventorySlot

Students using `GameInventorySlot` will need to:
1. Remove `GameInventorySlot` components
2. Add `GameInventoryManager`
3. Re-configure slots and re-wire events

## Getting Help

- **Program Flow**: See [ProgramFlow.md](Assets/DressToImpress/Documentation/ProgramFlow.md)
- **Component Reference**: See [ComponentReference.md](Assets/DressToImpress/Documentation/ComponentReference.md)
- **Setup Guide**: See [QuickStart.md](Assets/DressToImpress/Documentation/QuickStart.md)
