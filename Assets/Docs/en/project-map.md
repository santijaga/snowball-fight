# Project map and startup

[Contents](README.md) · [Русская версия](../project-map.md). Updated: 2026-10-29; static file inspection.

## Environment

Version from `ProjectSettings/ProjectVersion.txt`: **Unity 6000.5.4f1**. The project uses URP 2D and Input System. There are no project-owned `.asmdef` files in Assets: gameplay code uses the default assembly, while `Assets/Scripts/Editor` belongs to the editor assembly. Generated `.csproj` and `.sln` files are not the source of project configuration.

Main direct dependencies from `Packages/manifest.json`:

| Package | Version | Purpose |
| --- | --- | --- |
| Universal RP | 17.6.0 | Rendering, Renderer2D |
| Input System | 1.19.0 | Player/UI/Dialog action maps |
| Localization | 1.5.13 | Locales and String Tables |
| Cinemachine | 3.1.7 | Follow camera |
| 2D Animation | 15.1.0 | 2D animation tools |
| 2D Aseprite / PSD Importer | 5.0.3 / 14.0.3 | Art import |
| Tilemap / Tilemap Extras | 1.0.0 / 8.0.3 | Maps and palettes |
| Timeline / UGUI / Visual Scripting | 1.8.12 / 2.5.0 / 1.9.11 | Installed packages; installation does not imply use by every gameplay system |
| Test Framework | 1.7.0 | Installed; project rules prohibit adding tests and running them without a request |

The complete direct dependency list is in the manifest; resolved transitive versions are in `Packages/packages-lock.json`. Localization uses Addressables; its settings live in `Assets/AddressableAssetsData`. Packages were not updated for this handbook task.

## Directories

| Path | Contents |
| --- | --- |
| `Assets/Scripts` | Project code: Characters, Player, Props, Dialog, UI, Input, Localization, _GlobalServices, Editor |
| `Assets/Prefabs` | Player, Characters, NPC, Props, UI, Cameras, Environment, and scene points |
| `Assets/Resources/Party` | Darius, Shaya, three classes, shared experience table |
| `Assets/Resources/Inventory` | Shared Items database and six item definitions |
| `Assets/Resources/Dialogs` | Shaya's introduction and the opening sequence for her approach |
| `Assets/Scenes` | Four cathedral gameplay scenes included in Build Settings |
| `Assets/Animations/Characters` | Darius and Shaya controllers and clips |
| `Assets/Sprites` | Source images for characters, environment, and UI |
| `Assets/Tiles` | Room/Cathedral/Dev tile assets and palette/tile prefabs |
| `Assets/UI Toolkit` | UXML, shared USS, PanelSettings |
| `Assets/Fonts`, `Assets/TextMesh Pro` | Fonts, SDF assets, package resources |
| `Assets/Localization` | Source JSON and generated settings/tables |
| `Assets/Settings` | Input Actions, generated input C#, URP, Renderer2D, Volume Profile, scene template |
| `Assets/Docs` | This handbook, game design, and PNG screen mockups |
| `Assets/Docs/en` | English edition of the handbook; keep synchronized with the Russian pages |
| `docs` | Development log, earlier topic guides, drafts |
| `ProjectSettings` | Editor version, Build Settings, layers, physics, project configuration |
| `Packages` | Dependencies |
| `Library`, `Temp`, `obj`, `Logs`, `.vs`, `UIElementsSchema` | Caches and editor/tool output; do not edit as source files |
| `UserSettings` | Local user settings |

On Windows, `Docs/devlog.md` in AGENTS.md and the actual `docs/devlog.md` refer to the same directory. New links use the actual `docs` casing; account for the difference when moving to a case-sensitive filesystem.

## Startup sequence

1. `RuntimeInitializeOnLoadMethod(SubsystemRegistration)` resets the relevant systems' static references/state.
2. Before the scene loads, SessionStateService, InventoryService, PartyService, DoorStateService, ChestStateService, and GameLocalization are created automatically. They survive via `DontDestroyOnLoad`. Do not place copies in every scene.
3. InventoryService loads `Resources.Load("Inventory/Items")`. PartyService loads `Party/Darius` and `Party/Experience` and explicitly ensures the inventory exists before granting starting equipment. Do not assume a fixed order for the other independent initialization methods.
4. Scene-local InputContextService, UIService, and DialogService provide input and windows. They do not use the same automatic creation mechanism and do not have `DontDestroyOnLoad`.
5. PartyFormation creates scene Character instances from members' prefabs. PlayerSceneEntry passes the scene Player to SessionStateService, which selects the target TransitionPoint or a StartingPoint for a direct entry.
6. GameLocalization prepares tables asynchronously. Menus and new dialogues cannot open until it is ready. The menu displays the startup controls screen once per session; that state resets on a new Play Mode session.

Cathedral_Center is the first enabled scene in Build Settings. The other scenes are Cathedral_North_East_Closet, Cathedral_North_West_Room, and Cathedral_Center_Room; actual routes are listed in the [catalog](catalog.md). For manual checks, open the relevant gameplay scene after verifying its PlayerSceneEntry, StartingPoint, input, and UI.

## Working rules

The authoritative rules are in [AGENTS.md](../../../AGENTS.md). Before implementation, discuss ambiguities and the proposed approach; use numbered questions and proposals. Modify project files only when directly requested. Do not add tests; run Unity, builds, Play Mode, or tests only on explicit request. Record every change in the development log and handbook; do not describe static inspection as gameplay verification.

Architecture has been under reconsideration since 2026-09-29: prioritize convenient, self-contained components and prefabs with cohesive responsibilities. Existing services do not require every new task to become another service. Avoid unnecessary dependencies from reusable behavior to specific effects or player input. Action owners release locks, contexts, and subscriptions when disabled or destroyed.

Save text files with CRLF and preserve their encoding. Do not manually modify generated files. C# must follow the braces and blank-line rules for `if` and `return` in AGENTS.md.

Update 2026-10-29: Build Settings contains four cathedral scenes, with Cathedral_Center first. The unused prototype animation set was removed; the Darius and Shaya sets remain. Obsolete references to deleted scenes and graphics were removed. Static searches by name and GUID were performed; Unity, import, builds, Play Mode, and tests were not run.
