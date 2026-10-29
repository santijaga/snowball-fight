# Asset catalog

[Contents](README.md) · [Русская версия](../catalog.md). Working-tree snapshot: 2026-10-29. Compiled without starting Unity; GUID references resolved through existing `.meta` files.

## All prefabs in Assets

Paths are relative to the project root. Components listed here are project scripts directly serialized in the prefab YAML; variants and nested objects also require inspecting source prefabs. Unity and package components are not listed as gameplay services.

| Prefab | Project components in the file | Source/nested prefabs |
| --- | --- | --- |
| `Assets/Prefabs/Cameras/Cameras.prefab` | — | — |
| `Assets/Prefabs/Characters/Darius.prefab` | Character, CharacterAnimationController, CharacterInteractionsController, CharacterMovementController | — |
| `Assets/Prefabs/Characters/Shaya.prefab` | Character, CharacterAnimationController, CharacterInteractionsController, CharacterMovementController | — |
| `Assets/Prefabs/Environment/Grid.prefab` | — | — |
| `Assets/Prefabs/NPC/ShayaNPC.prefab` | CharacterAnimationController, CharacterMovementController, RecruitableNpc | Assets/Prefabs/UI/InteractionMarker.prefab |
| `Assets/Prefabs/Player.prefab` | PartyFormation, PlayerController | — |
| `Assets/Prefabs/Props/Cathedral/Chest.prefab` | Chest | Assets/Prefabs/UI/InteractionMarker.prefab |
| `Assets/Prefabs/Props/Cathedral/Door.prefab` | Door | — |
| `Assets/Prefabs/Props/Misc/SecretDialog.prefab` | DialogProp | Assets/Prefabs/UI/InteractionMarker.prefab |
| `Assets/Prefabs/Props/Room/Banquette.prefab` | — | — |
| `Assets/Prefabs/Props/Room/Bed.prefab` | — | — |
| `Assets/Prefabs/Props/Room/Cabinet.prefab` | DialogProp | Assets/Prefabs/UI/InteractionMarker.prefab |
| `Assets/Prefabs/Props/Room/Closet.prefab` | DialogProp | Assets/Prefabs/UI/InteractionMarker.prefab |
| `Assets/Prefabs/Props/Room/Door.prefab` | Door | — |
| `Assets/Prefabs/StartingPoint.prefab` | StartingPoint | — |
| `Assets/Prefabs/TransitionPoint.prefab` | TransitionPoint | — |
| `Assets/Prefabs/UI/InteractionMarker.prefab` | InteractionMarker | — |
| `Assets/Prefabs/UI/UI.prefab` | ControlsPanelView, DialogView, EquipmentPanelView, InventoryPanelView, MenuPanel, PartyPanelView, SceneRevealScreen, SettingsPanelView, StatusPanelView | — |
| `Assets/Tiles/Cathedral/Cathedral.prefab` | — | — |
| `Assets/Tiles/Dev/Impassable.prefab` | — | — |
| `Assets/Tiles/Room/Room.prefab` | — | — |

Player coordinates the party; Characters contains member bodies, and NPC/ShayaNPC is a separate recruitable NPC. Props/Room contains furniture and a door: Cabinet/Closet have DialogProp; Bed/Banquette provide graphics/colliders without their own interaction behavior. Props/Cathedral contains a door and chest; Props/Misc/SecretDialog provides a hidden message. UI/UI assembles panels; InteractionMarker is a child indicator. Environment/Grid supplies the grid foundation. Prefabs under Tiles are palettes/tiles and should not automatically be treated as finished gameplay rooms.

## All scenes

| Scene | Included in Build Settings | Project components directly in scene YAML |
| --- | --- | --- |
| `Assets/Scenes/Cathedral_Center_Room.unity` | Yes | DialogService, DialogView, InputContextService, PlayerController, PlayerSceneEntry, SceneRevealScreen, UIService |
| `Assets/Scenes/Cathedral_Center.unity` | Yes | DialogService, DialogView, InputContextService, PlayerController, PlayerSceneEntry, SceneRevealScreen, UIService |
| `Assets/Scenes/Cathedral_North_East_Closet.unity` | Yes | DialogService, DialogView, InputContextService, PlayerController, PlayerSceneEntry, SceneRevealScreen, UIService |
| `Assets/Scenes/Cathedral_North_West_Room.unity` | Yes | DialogService, DialogView, InputContextService, LostShayaTrigger, PlayerController, PlayerSceneEntry, RecruitableNpc, SceneRevealScreen, UIService |
| `Assets/Settings/Scenes/URP2DSceneTemplate.unity` | No (template) |  |

Components inside prefab instances may be absent from this list: Unity stores a prefab reference and overrides. Cathedral_Center is the first build scene. Prefabs used by each scene:

| Scene | Source prefabs (unique paths, without instance counts) |
| --- | --- |
| Cathedral_Center_Room | Assets/Prefabs/Cameras/Cameras.prefab; Assets/Prefabs/Environment/Grid.prefab; Assets/Prefabs/Player.prefab; Assets/Prefabs/StartingPoint.prefab; Assets/Prefabs/TransitionPoint.prefab; Assets/Prefabs/UI/UI.prefab |
| Cathedral_Center | Assets/Prefabs/Cameras/Cameras.prefab; Assets/Prefabs/Environment/Grid.prefab; Assets/Prefabs/Player.prefab; Assets/Prefabs/Props/Cathedral/Door.prefab; Assets/Prefabs/StartingPoint.prefab; Assets/Prefabs/TransitionPoint.prefab; Assets/Prefabs/UI/UI.prefab |
| Cathedral_North_East_Closet | Assets/Prefabs/Cameras/Cameras.prefab; Assets/Prefabs/Environment/Grid.prefab; Assets/Prefabs/Player.prefab; Assets/Prefabs/Props/Cathedral/Chest.prefab; Assets/Prefabs/StartingPoint.prefab; Assets/Prefabs/TransitionPoint.prefab; Assets/Prefabs/UI/UI.prefab |
| Cathedral_North_West_Room | Assets/Prefabs/Cameras/Cameras.prefab; Assets/Prefabs/Environment/Grid.prefab; Assets/Prefabs/NPC/ShayaNPC.prefab; Assets/Prefabs/Player.prefab; Assets/Prefabs/Props/Cathedral/Chest.prefab; Assets/Prefabs/StartingPoint.prefab; Assets/Prefabs/TransitionPoint.prefab; Assets/Prefabs/UI/UI.prefab |
| URP2DSceneTemplate |  |

## Configured scene transitions

Values were extracted from TransitionPoint instance overrides. These are configuration values, not confirmation that the routes were traversed successfully in game.

| Scene | Id | Target Scene | Target Id |
| --- | --- | --- | --- |
| Cathedral_Center_Room | toCenter | Cathedral_Center | toCenterRoom |
| Cathedral_Center | toWestRoom | Cathedral_North_West_Room | toCenter |
| Cathedral_Center | toCenterRoom | Cathedral_Center_Room | toCenter |
| Cathedral_Center | toCloset | Cathedral_North_East_Closet | toCenter |
| Cathedral_North_East_Closet | toCenter | Cathedral_Center | toCloset |
| Cathedral_North_West_Room | toCenter | Cathedral_Center | toWestRoom |

## All definitions under Assets/Resources

| Asset | Type / key references |
| --- | --- |
| `Assets/Resources/Dialogs/ShayaCatchUp.asset` | m_Script: Assets/Scripts/Dialog/DialogSequence.cs |
| `Assets/Resources/Dialogs/ShayaIntroduction.asset` | m_Script: Assets/Scripts/Dialog/DialogSequence.cs |
| `Assets/Resources/Inventory/BronzeKey.asset` | m_Script: Assets/Scripts/_GlobalServices/ItemDefinition.cs; id: bronze_key |
| `Assets/Resources/Inventory/Items.asset` | m_Script: Assets/Scripts/_GlobalServices/ItemDatabase.cs |
| `Assets/Resources/Inventory/LongSword.asset` | m_Script: Assets/Scripts/_GlobalServices/ItemDefinition.cs; id: long_sword |
| `Assets/Resources/Inventory/Potion.asset` | m_Script: Assets/Scripts/_GlobalServices/ItemDefinition.cs; id: potion |
| `Assets/Resources/Inventory/RustyKey.asset` | m_Script: Assets/Scripts/_GlobalServices/ItemDefinition.cs; id: rusty_key |
| `Assets/Resources/Inventory/Scythe.asset` | m_Script: Assets/Scripts/_GlobalServices/ItemDefinition.cs; id: scythe |
| `Assets/Resources/Inventory/Spear.asset` | m_Script: Assets/Scripts/_GlobalServices/ItemDefinition.cs; id: spear |
| `Assets/Resources/Party/Darius.asset` | m_Script: Assets/Scripts/_GlobalServices/PartyCharacterDefinition.cs; characterClass: Assets/Resources/Party/Necromancer.asset; initialWeapon: Assets/Resources/Inventory/Scythe.asset; characterPrefab: Assets/Prefabs/Characters/Darius.prefab; characterNameKey: character.darius.name; initialLevel: 1 |
| `Assets/Resources/Party/Experience.asset` | m_Script: Assets/Scripts/_GlobalServices/ExperienceTable.cs |
| `Assets/Resources/Party/Hunter.asset` | m_Script: Assets/Scripts/_GlobalServices/CharacterClassDefinition.cs; classNameKey: class.hunter.name |
| `Assets/Resources/Party/Necromancer.asset` | m_Script: Assets/Scripts/_GlobalServices/CharacterClassDefinition.cs; classNameKey: class.necromancer.name |
| `Assets/Resources/Party/Shaya.asset` | m_Script: Assets/Scripts/_GlobalServices/PartyCharacterDefinition.cs; characterClass: Assets/Resources/Party/Hunter.asset; initialWeapon: Assets/Resources/Inventory/Spear.asset; characterPrefab: Assets/Prefabs/Characters/Shaya.prefab; characterNameKey: character.shaya.name; initialLevel: 1 |
| `Assets/Resources/Party/Warrior.asset` | m_Script: Assets/Scripts/_GlobalServices/CharacterClassDefinition.cs; classNameKey: class.warrior.name |

## Animation controllers and clips

Each of the two directories, Characters/Darius and Characters/Shaya, contains its own controller and eight Idle/Move clips for Down/Up/Left/Right. Gameplay definitions reference Characters/Darius and Characters/Shaya.

- `Assets/Animations/Characters/Darius/Darius Idle Down.anim`
- `Assets/Animations/Characters/Darius/Darius Idle Left.anim`
- `Assets/Animations/Characters/Darius/Darius Idle Right.anim`
- `Assets/Animations/Characters/Darius/Darius Idle Up.anim`
- `Assets/Animations/Characters/Darius/Darius Move Down.anim`
- `Assets/Animations/Characters/Darius/Darius Move Left.anim`
- `Assets/Animations/Characters/Darius/Darius Move Right.anim`
- `Assets/Animations/Characters/Darius/Darius Move Up.anim`
- `Assets/Animations/Characters/Darius/Darius.controller`
- `Assets/Animations/Characters/Shaya/Shaya Idle Down.anim`
- `Assets/Animations/Characters/Shaya/Shaya Idle Left.anim`
- `Assets/Animations/Characters/Shaya/Shaya Idle Right.anim`
- `Assets/Animations/Characters/Shaya/Shaya Idle Up.anim`
- `Assets/Animations/Characters/Shaya/Shaya Move Down.anim`
- `Assets/Animations/Characters/Shaya/Shaya Move Left.anim`
- `Assets/Animations/Characters/Shaya/Shaya Move Right.anim`
- `Assets/Animations/Characters/Shaya/Shaya Move Up.anim`
- `Assets/Animations/Characters/Shaya/Shaya.controller`

## UI and settings

- `Assets/AddressableAssetsData/AddressableAssetSettings.asset`
- `Assets/Docs/Settings Screen Mockup.png`
- `Assets/Localization/Generated/LocalizationSettings.asset`
- `Assets/Scripts/UI/SettingsPanelView.cs`
- `Assets/Settings/DefaultVolumeProfile.asset`
- `Assets/Settings/InputSystem_Actions.cs`
- `Assets/Settings/InputSystem_Actions.inputactions`
- `Assets/Settings/Lit2DSceneTemplate.scenetemplate`
- `Assets/Settings/Renderer2D.asset`
- `Assets/Settings/Scenes/URP2DSceneTemplate.unity`
- `Assets/Settings/UniversalRenderPipelineGlobalSettings.asset`
- `Assets/Settings/UniversalRP.asset`
- `Assets/TextMesh Pro/Resources/TMP Settings.asset`
- `Assets/UI Toolkit/CharacterInfoCardTemplate.uxml`
- `Assets/UI Toolkit/ControlsScreenTemplate.uxml`
- `Assets/UI Toolkit/DialogTemplate.uxml`
- `Assets/UI Toolkit/EquipmentScreenTemplate.uxml`
- `Assets/UI Toolkit/InventoryScreenTemplate.uxml`
- `Assets/UI Toolkit/MenuSidebarTemplate.uxml`
- `Assets/UI Toolkit/MenuStyles.uss`
- `Assets/UI Toolkit/MenuTemplate.uxml`
- `Assets/UI Toolkit/PanelSettings.asset`
- `Assets/UI Toolkit/PartyPanelTemplate.uxml`
- `Assets/UI Toolkit/SceneRevealTemplate.uxml`
- `Assets/UI Toolkit/SettingsScreenTemplate.uxml`
- `Assets/UI Toolkit/StatusScreenTemplate.uxml`

## Graphics, tiles, and fonts

All source images and other assets in these directories are listed below, excluding `.meta` files. Tile `.asset` files are listed to make the full collection navigable; inclusion does not imply that every tile is used in scenes.

### Assets/Sprites

Sprite catalog update 2026-10-29: an unused experimental character sprite sheet and its .meta were removed; no references to the asset were found in gameplay files before removal. Static searches by name and GUID were performed; Unity, import, builds, Play Mode, and tests were not run.

Combat sprite sheets: `Assets/Sprites/Characters/Huntress Combat Sprtitesheet.png` and `Assets/Sprites/Characters/Necromancer Combat Sritesheet.png`. Each .meta defines 12 sprites (_0–_11) at 32×32: Sprite Mode Multiple, 16 Pixels Per Unit, Point filtering, mipmaps disabled. No references to their GUIDs were found in other Assets files; the sheets are not assigned to prefabs or clips. The artwork does not imply implemented combat. Static inspection on 2026-10-20; Unity, import, builds, Play Mode, and tests were not run; slicing and rendering remain unverified.

- `Assets/Sprites/Battle Backgrounds/Cathedral Backdrop Sprite.png`
- `Assets/Sprites/Characters/Huntress Combat Sprtitesheet.png`
- `Assets/Sprites/Characters/Huntress.png`
- `Assets/Sprites/Characters/Necromancer Combat Sritesheet.png`
- `Assets/Sprites/Characters/Necromancer.png`
- `Assets/Sprites/Dev/Impassable.png`
- `Assets/Sprites/Environment/Cathedral/Cathedral Tileset v.0 1.png`
- `Assets/Sprites/Environment/Cathedral/Cathedral Tileset v.0 Door Part.png`
- `Assets/Sprites/Environment/Cathedral/Cathedral Tileset v.0 Doors.png`
- `Assets/Sprites/Environment/Cathedral/Cathedral Tileset v.0 Shadow Part.png`
- `Assets/Sprites/Environment/Cathedral/Chest 1.png`
- `Assets/Sprites/Environment/Room/Banquette.png`
- `Assets/Sprites/Environment/Room/Bed.png`
- `Assets/Sprites/Environment/Room/Cabinet.png`
- `Assets/Sprites/Environment/Room/Closet_Opened_Joke.png`
- `Assets/Sprites/Environment/Room/Closet_Opened.png`
- `Assets/Sprites/Environment/Room/Closet.png`
- `Assets/Sprites/Environment/Room/Door Frame.png`
- `Assets/Sprites/Environment/Room/Door.png`
- `Assets/Sprites/Environment/Room/Parquet 2.png`
- `Assets/Sprites/Environment/Room/Walls 2 2.png`
- `Assets/Sprites/Environment/Room/Walls 2.png`
- `Assets/Sprites/UI/Arrow.png`
- `Assets/Sprites/UI/Black Screen.png`
- `Assets/Sprites/UI/Cursor.png`
- `Assets/Sprites/UI/Dark Frame.png`
- `Assets/Sprites/UI/Dialog.png`
- `Assets/Sprites/UI/Frame.png`
- `Assets/Sprites/UI/Interactive.png`

#### Weapon combat sprites — 2026-10-21

Standalone weapon combat sprites were added under `Assets/Sprites/Weapons` (filenames preserved):

- `Assets/Sprites/Weapons/Scythe Combat Sprite.png`
- `Assets/Sprites/Weapons/Long Sword Combat Sprite.png`
- `Assets/Sprites/Weapons/Spear Combat Sprite.png`

Each PNG is 16×16. The .meta settings are Texture Type Sprite (2D and UI), Sprite Mode Single, 16 Pixels Per Unit, Point filtering, and mipmaps disabled. Static GUID searches found no references in other Assets files; the sprites are not yet assigned to item assets, prefabs, or clips. ItemDefinition has no weapon sprite field. These images do not imply implemented weapon rendering in combat. Static inspection on 2026-10-21; Unity, import, builds, Play Mode, and tests were not run; in-game rendering remains unverified.

#### Cathedral combat backdrop — 2026-10-21

`Assets/Sprites/Battle Backgrounds/Cathedral Backdrop Sprite.png` — cathedral combat backdrop, PNG 472×232. The .meta defines Sprite Mode Multiple with one full-image sprite, `Cathedral Backdrop Sprite_0` (472×232), 16 Pixels Per Unit, Point filtering, and mipmaps disabled. Static GUID searches found no references in other Assets files; the backdrop is not yet assigned to scenes, prefabs, or other assets. The image does not imply an implemented combat scene. Static inspection on 2026-10-21; Unity, import, builds, Play Mode, and tests were not run; rendering remains unverified.

### Assets/Tiles

- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_0.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_1.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_10.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_11.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_12.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_13.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_14.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_15.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_16.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_17.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_18.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_19.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_2.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_20.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_21.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_22.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_23.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_24.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_25.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_26.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_27.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_28.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_29.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_3.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_30.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_31.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_32.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_33.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_34.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_35.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_36.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_37.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_38.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_39.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_4.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_40.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_41.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_42.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_43.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_44.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_45.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_46.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_5.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_6.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_7.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_8.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 1_9.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 Door Part_0.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 Door Part_1.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0 Shadow Part_0.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_0.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_1.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_10.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_11.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_12.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_13.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_14.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_15.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_16.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_17.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_18.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_19.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_2.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_20.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_21.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_22.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_23.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_24.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_25.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_26.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_27.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_28.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_29.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_3.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_30.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_31.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_32.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_33.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_34.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_35.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_36.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_37.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_38.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_39.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_4.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_40.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_5.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_6.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_7.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_8.asset`
- `Assets/Tiles/Cathedral/Cathedral Tileset v.0_9.asset`
- `Assets/Tiles/Cathedral/Cathedral.prefab`
- `Assets/Tiles/Dev/Impassable.asset`
- `Assets/Tiles/Dev/Impassable.prefab`
- `Assets/Tiles/Room/Background/Parquet 2_0.asset`
- `Assets/Tiles/Room/Background/Parquet 2_1.asset`
- `Assets/Tiles/Room/Background/Parquet 2_2.asset`
- `Assets/Tiles/Room/Props/Bed_0.asset`
- `Assets/Tiles/Room/Props/Bed_1.asset`
- `Assets/Tiles/Room/Props/Door Frame_0.asset`
- `Assets/Tiles/Room/Room.prefab`
- `Assets/Tiles/Room/Walls/Walls 2 2_0.asset`
- `Assets/Tiles/Room/Walls/Walls 2 2_1.asset`
- `Assets/Tiles/Room/Walls/Walls 2_0.asset`
- `Assets/Tiles/Room/Walls/Walls 2_1.asset`
- `Assets/Tiles/Room/Walls/Walls 2_10.asset`
- `Assets/Tiles/Room/Walls/Walls 2_11.asset`
- `Assets/Tiles/Room/Walls/Walls 2_12.asset`
- `Assets/Tiles/Room/Walls/Walls 2_13.asset`
- `Assets/Tiles/Room/Walls/Walls 2_14.asset`
- `Assets/Tiles/Room/Walls/Walls 2_15.asset`
- `Assets/Tiles/Room/Walls/Walls 2_16.asset`
- `Assets/Tiles/Room/Walls/Walls 2_17.asset`
- `Assets/Tiles/Room/Walls/Walls 2_18.asset`
- `Assets/Tiles/Room/Walls/Walls 2_19.asset`
- `Assets/Tiles/Room/Walls/Walls 2_2.asset`
- `Assets/Tiles/Room/Walls/Walls 2_20.asset`
- `Assets/Tiles/Room/Walls/Walls 2_21.asset`
- `Assets/Tiles/Room/Walls/Walls 2_22.asset`
- `Assets/Tiles/Room/Walls/Walls 2_23.asset`
- `Assets/Tiles/Room/Walls/Walls 2_24.asset`
- `Assets/Tiles/Room/Walls/Walls 2_25.asset`
- `Assets/Tiles/Room/Walls/Walls 2_26.asset`
- `Assets/Tiles/Room/Walls/Walls 2_27.asset`
- `Assets/Tiles/Room/Walls/Walls 2_28.asset`
- `Assets/Tiles/Room/Walls/Walls 2_29.asset`
- `Assets/Tiles/Room/Walls/Walls 2_3.asset`
- `Assets/Tiles/Room/Walls/Walls 2_30.asset`
- `Assets/Tiles/Room/Walls/Walls 2_31.asset`
- `Assets/Tiles/Room/Walls/Walls 2_32.asset`
- `Assets/Tiles/Room/Walls/Walls 2_33.asset`
- `Assets/Tiles/Room/Walls/Walls 2_34.asset`
- `Assets/Tiles/Room/Walls/Walls 2_4.asset`
- `Assets/Tiles/Room/Walls/Walls 2_5.asset`
- `Assets/Tiles/Room/Walls/Walls 2_6.asset`
- `Assets/Tiles/Room/Walls/Walls 2_7.asset`
- `Assets/Tiles/Room/Walls/Walls 2_8.asset`
- `Assets/Tiles/Room/Walls/Walls 2_9.asset`

### Assets/Fonts

- `Assets/Fonts/OFL.txt`
- `Assets/Fonts/PressStart2P-Regular SDF 1.asset`
- `Assets/Fonts/PressStart2P-Regular SDF.asset`
- `Assets/Fonts/PressStart2P-Regular.ttf`

## Derived and third-party assets

`Assets/Localization/Generated` and `Assets/AddressableAssetsData` are maintained by Unity Localization/Addressables; they are not the manually editable translation source. `Assets/TextMesh Pro` contains package resources; distinguish them from project-owned gameplay prefabs/services. Complete dependency lists are in Packages/manifest.json and packages-lock.json.

This catalog is a maintained snapshot, not a live editor database. Update both language editions in the same task when adding, removing, or renaming assets.

Update 2026-10-20: stats are strength/agility/intellect (1–99); HP derives from current strength, MP from intellect. CharacterClassDefinition stores intellect/strength/agility for 99 levels; PartyMember/PartyService use CurrentAgility/SetCurrentAgility instead of Speed; stamina APIs were removed. Warrior, Hunter, and Necromancer retain strength/intellect tables and migrate speed to agility. Darius: 2/2/3, HP 5, MP 1, damage 9; Shaya: 3/5/2, HP 10, MP 0, damage 9 (strength/agility/intellect). Status shows current stats with number colors relative to base; equipment shows white current values. Combat and effects remain planned. Static source review only; Unity, import, builds, Play Mode, and tests were not run.

Update 2026-10-29: Build Settings contains four cathedral scenes, with Cathedral_Center first. The unused prototype animation set was removed; the Darius and Shaya sets remain. Obsolete references to deleted scenes and graphics were removed. Static searches by name and GUID were performed; Unity, import, builds, Play Mode, and tests were not run.
