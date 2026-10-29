# Code and services

[Contents](README.md) · [Русская версия](../code-services.md). Updated: 2026-10-19; responsibilities established from source inspection.

## Lifetimes and dependencies

| System | Creation and lifetime | Main dependencies / data |
| --- | --- | --- |
| SessionStateService | Automatic BeforeSceneLoad, DontDestroyOnLoad | Pending transition, source information, selected PartyMember, recruited Npc Id set; finds points in the player's scene during initialization |
| InventoryService | Automatic; PartyService also calls EnsureCreated; DontDestroyOnLoad | Resources Inventory/Items, stacks, equipped counts through PartyService |
| PartyService | Automatic BeforeSceneLoad, DontDestroyOnLoad | Resources Party/Darius and Party/Experience, InventoryService for starting equipment |
| DoorStateService | Automatic BeforeSceneLoad, DontDestroyOnLoad | Records by ID, binding owners, Locked/Opened |
| ChestStateService | Automatic BeforeSceneLoad, DontDestroyOnLoad | Records by ID, binding owners, Locked/Opened/ContentItemId/ContentQuantity |
| GameLocalization | Automatic BeforeSceneLoad, DontDestroyOnLoad | Unity Localization, GameText ru/en, asynchronous readiness and Changed; exposes a static API |
| InputContextService | Scene component, DefaultExecutionOrder −100 | Own instance of generated InputSystem_Actions, context stack, device scheme |
| UIService | Scene component | Set of registered open MonoBehaviours, UI-state events |
| DialogService | Scene component referencing DialogView | InputContextService, UIService, GameLocalization; current message/sequence and owner callback |

For most systems, singleton Instance is a current reference, not a guarantee of availability when disabled/destroyed. Automatic services reset statics at SubsystemRegistration and do not require manually placed scene instances. Provide scene services in every gameplay scene; after another scene loads, obtain their references again or reassign them through scene objects.

There is no central DI container, unified save file, quest manager, or general story-event graph. `_GlobalServices` also contains plain definitions and data structures: the directory name does not make every file a service.

## Project C# catalog

Paths in this table are relative to `Assets/Scripts/`.

| File | Responsibility |
| --- | --- |
| `_GlobalServices/CharacterClassDefinition.cs` | Class definition, four-stat table by level, OnValidate |
| `_GlobalServices/ExperienceTable.cs` | Shared experience requirements for 99 levels |
| `_GlobalServices/PartyCharacterDefinition.cs` | Initial character data and loadout |
| `_GlobalServices/PartyMember.cs` | In-memory member data, damage calculation, six slots |
| `_GlobalServices/PartyRules.cs` | Ranges and maximum HP/MP formulas |
| `_GlobalServices/PartyService.cs` | Composition, stat changes, adding, removing, equipping |
| `_GlobalServices/ItemDefinition.cs` | Item and category/type/slot/handedness enums |
| `_GlobalServices/ItemDatabase.cs` | Definition list, Id lookup, database validation |
| `_GlobalServices/InventoryService.cs` | Shared inventory, available/equipped quantities, atomic operations |
| `_GlobalServices/SessionStateService.cs` | Session data and spawn-point selection |
| `_GlobalServices/DoorStateService.cs` | Door states, bindings, duplicate prevention |
| `_GlobalServices/ChestStateService.cs` | Chest states, loot, bindings, duplicate prevention |
| `Characters/Character.cs` | Binds PartyMember to a body, facing, control mode |
| `Characters/CharacterMovementController.cs` | Rigidbody2D movement, obstacles, step completion |
| `Characters/CharacterAnimationController.cs` | Facing/movement → Animator parameters |
| `Characters/CharacterInteractionsController.cs` | Object detection in front of a character, focus, TryInteract |
| `Characters/RecruitableNpc.cs` | NPC dialogue, locks, approach, party recruitment |
| `Characters/LostShayaTrigger.cs` | Specific encounter with Shaya when left behind, pathfinding, dialogue handoff |
| `Player/PlayerController.cs` | Gameplay input, buffer, locks, formation commands |
| `Player/PartyFormation.cs` | Scene Character instances, follower route, leader switching |
| `Props/IInteractable.cs` | Interaction-attempt contract and events |
| `Props/IPlayerVisible.cs` | Player focus/visibility contract |
| `Props/InteractionMarker.cs` | Presentation of availability and interaction state |
| `Props/DialogProp.cs` | Single message and optional sprite change |
| `Props/Door.cs` | Lock, key, messages, opening, Blocker |
| `Props/Chest.cs` | Lock, loot grant, messages, chest sprite |
| `Dialog/DialogSequence.cs` | Conversation asset, lines, conditions |
| `Dialog/DialogService.cs` | Dialogue opening, continuation, filtering, cancellation |
| `Dialog/DialogView.cs` | Dialogue-window display and localization |
| `Input/InputContext.cs` | Player / Dialog / UI enum |
| `Input/InputContextService.cs` | Actions instance, single-map activation, temporary contexts |
| `Localization/GameLocalization.cs` | Table loading, Text, language, Changed |
| `Editor/GameLocalizationImport.cs` | Editor JSON import; also contains GameTextSourcePostprocessor |
| `UI/UIService.cs` | Open-window registry |
| `UI/MenuPanel.cs` | Main/nested screen navigation, startup controls |
| `UI/PartyPanelView.cs` | Three party cards |
| `UI/InventoryPanelView.cs` | Tabs, item list, descriptions |
| `UI/EquipmentPanelView.cs` | Member, slot, equipment selection |
| `UI/StatusPanelView.cs` | Detailed member stats |
| `UI/SettingsPanelView.cs` | Language switching |
| `UI/ControlsPanelView.cs` | Controls screen and Continue button |
| `UI/SceneRevealScreen.cs` | Scene cover/reveal screen, ISceneTransitionScreen |
| `ISceneTransitionScreen.cs` | Small transition-screen contract |
| `TransitionPoint.cs` | Configurable transition after a completed step |
| `StartingPoint.cs` | Position and facing for direct entry |
| `SpawnFacingDirection.cs` | Spawn-facing enum and conversion to Vector2 |
| `PlayerSceneEntry.cs` | Passes the scene player to SessionStateService |

Separately, `Assets/Settings/InputSystem_Actions.cs` is the generated input wrapper. Edit only `.inputactions` and regenerate through the editor. Unity/package code in Library/PackageCache and TextMesh Pro resources are not project-owned gameplay systems.

## Relationships to consider when extending systems

PartyService.Changed refreshes formation and UI. InventoryService.Changed refreshes items/equipment candidates; equipment operations notify both sides. GameLocalization.Changed refreshes labels and open messages. UIService reports open windows so markers/initiators can account for UI occupancy.

Dialogue owners cancel their own callbacks; context and lock owners release their own handles. Do not retain a temporary lock without OnDisable/OnDestroy cleanup. Disabled components must not begin new actions; interrupted actions must release or restore acquired resources. Apply these requirements to new initiators without necessarily splitting every operation into another service.

Update 2026-10-19: stats are strength/agility/intellect (1–99); HP derives from current strength, MP from intellect. CharacterClassDefinition stores intellect/strength/agility for 99 levels; PartyMember/PartyService use CurrentAgility/SetCurrentAgility instead of Speed; stamina APIs were removed. Warrior, Hunter, and Necromancer retain strength/intellect tables and migrate speed to agility. Darius: 2/2/3, HP 5, MP 1, damage 9; Shaya: 3/5/2, HP 10, MP 0, damage 9 (strength/agility/intellect). Status shows current stats with number colors relative to base; equipment shows white current values. Combat and effects remain planned. Static source review only; Unity, import, builds, Play Mode, and tests were not run.
