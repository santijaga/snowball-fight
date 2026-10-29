# Snowball Fight project handbook

Updated: **2026-10-29**. Entry point for developers and the assistant. [Русская версия](../README.md).

**Before analyzing or planning changes, read this README and the relevant topic pages. When changing code, assets, scenes, settings, or working rules, update the affected pages and catalogs in the same task and record the change in the [development log](../../../docs/devlog.md).** Include the date and verification status. The handbook does not replace inspecting current code and Inspector settings. Keep the Russian and English editions synchronized; identifiers, asset paths, configuration details, and verification claims must agree.

## Navigation

| Page | Contents |
| --- | --- |
| [Project map and startup](project-map.md) | Directories, Unity version, dependencies, initialization, development rules |
| [Characters and classes](characters.md) | Data, prefabs, animation, party, stats, creating characters and NPCs |
| [Dialogue and localization](dialogue-localization.md) | Lines, conditions, introduction scenarios, JSON, translation import |
| [Items and interactive props](items-props.md) | Inventory, equipment, doors, chests, markers, stable IDs |
| [Scenes, input, and UI](scenes-ui.md) | Transitions, spawn points, camera, input actions, UI Toolkit |
| [Asset catalog](catalog.md) | All prefabs and scenes, transitions, Resources data, controllers, templates |
| [Code and service catalog](code-services.md) | All project C# files, responsibilities, service lifetimes |

## Quick context

This is a Unity 2D project with grid-based exploration and RPG data. The directory name does not imply that a multiplayer snowball battle exists: the current project code implements movement, the party, interactions, dialogue, items, equipment, and menus. Combat, a turn-order timeline, effects, disk saves, and automatic leveling are not implemented.

The starting character is Darius, a level 1 Necromancer carrying Scythe. Shaya is a level 1 Hunter carrying Spear; she joins after her introduction in Cathedral_North_West_Room. The party supports up to three members. Player coordinates input and formation; physical bodies and graphics live in separate Character prefabs.

The game supports Russian and English. The editable source is `Assets/Localization/GameText.json`; do not manually edit `Assets/Localization/Generated`. Party data, inventory, doors, chests, NPC recruitment flags, and language selection survive scene changes during a session but are not saved to disk.

## Evidence and reliability

This handbook describes the current working-tree files, including uncommitted assets, rather than only HEAD. Structure and assigned references were inspected statically. Unity, imports, builds, Play Mode, and tests were not run for this documentation task. Behavior descriptions reflect implementation and configuration, not confirmation from gameplay.

If the documentation disagrees with the project, inspect code and serialized settings, then update the handbook. The [devlog](../../../docs/devlog.md) records history and verification limits. [gamedesign.md](../gamedesign.md) contains design parameters and decisions; plans there must not be treated as implemented systems. Root-level `ARCHITECTURE.md`, `game design.md`, `docs/Драфт.txt`, and `docs/Наброски.txt` provide historical/design material; `ARCHITECTURE.md` still references old, removed Player components. These supporting documents have not been translated as part of the handbook edition.

## Maintaining the handbook

1. When changing a system, update its topic page, configuration fields, limitations, and content-creation procedure in both languages.
2. When adding, renaming, or removing a prefab, scene, script, asset, or service, update the relevant catalogs and affected links in both editions.
3. For serialized configuration changes, inspect scene instances and overrides as well as the base prefab.
4. Update the relevant page date and development log. Distinguish static inspection from behavior actually checked by the user in Unity.
5. Preserve CRLF and existing encodings. Do not manually edit generated files. Unity creates `.meta` files for new Markdown files when importing them; no import was performed in this task.

Update 2026-10-19: stats are strength/agility/intellect (1–99); HP derives from current strength, MP from intellect. CharacterClassDefinition stores intellect/strength/agility for 99 levels; PartyMember/PartyService use CurrentAgility/SetCurrentAgility instead of Speed; stamina APIs were removed. Warrior, Hunter, and Necromancer retain strength/intellect tables and migrate speed to agility. Darius: 2/2/3, HP 5, MP 1, damage 9; Shaya: 3/5/2, HP 10, MP 0, damage 9 (strength/agility/intellect). Status shows current stats with number colors relative to base; equipment shows white current values. Combat and effects remain planned. Static source review only; Unity, import, builds, Play Mode, and tests were not run.

## Combat sprite sheets — 2026-10-20

Combat sprite sheets: `Assets/Sprites/Characters/Huntress Combat Sprtitesheet.png` and `Assets/Sprites/Characters/Necromancer Combat Sritesheet.png`. Each .meta defines 12 sprites (_0–_11) at 32×32: Sprite Mode Multiple, 16 Pixels Per Unit, Point filtering, mipmaps disabled. No references to their GUIDs were found in other Assets files; the sheets are not assigned to prefabs or clips. The artwork does not imply implemented combat. Static inspection on 2026-10-20; Unity, import, builds, Play Mode, and tests were not run; slicing and rendering remain unverified.

## Weapon combat sprites — 2026-10-21

Standalone weapon combat sprites were added under `Assets/Sprites/Weapons` (filenames preserved):

- `Assets/Sprites/Weapons/Scythe Combat Sprite.png`
- `Assets/Sprites/Weapons/Long Sword Combat Sprite.png`
- `Assets/Sprites/Weapons/Spear Combat Sprite.png`

Each PNG is 16×16. The .meta settings are Texture Type Sprite (2D and UI), Sprite Mode Single, 16 Pixels Per Unit, Point filtering, and mipmaps disabled. Static GUID searches found no references in other Assets files; the sprites are not yet assigned to item assets, prefabs, or clips. ItemDefinition has no weapon sprite field. These images do not imply implemented weapon rendering in combat. Static inspection on 2026-10-21; Unity, import, builds, Play Mode, and tests were not run; in-game rendering remains unverified.

## Cathedral combat backdrop — 2026-10-21

`Assets/Sprites/Battle Backgrounds/Cathedral Backdrop Sprite.png` — cathedral combat backdrop, PNG 472×232. The .meta defines Sprite Mode Multiple with one full-image sprite, `Cathedral Backdrop Sprite_0` (472×232), 16 Pixels Per Unit, Point filtering, and mipmaps disabled. Static GUID searches found no references in other Assets files; the backdrop is not yet assigned to scenes, prefabs, or other assets. The image does not imply an implemented combat scene. Static inspection on 2026-10-21; Unity, import, builds, Play Mode, and tests were not run; rendering remains unverified.

Update 2026-10-29: Build Settings contains four cathedral scenes, with Cathedral_Center first. The unused prototype animation set was removed; the Darius and Shaya sets remain. Obsolete references to deleted scenes and graphics were removed. Static searches by name and GUID were performed; Unity, import, builds, Play Mode, and tests were not run.
