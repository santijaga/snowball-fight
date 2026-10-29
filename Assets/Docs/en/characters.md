# Characters, classes, and party

[Contents](README.md) · [Русская версия](../characters.md). Updated: 2026-10-20; behavior was not checked in Unity for this task.

## Data and scene objects

| Layer | Responsibility |
| --- | --- |
| CharacterClassDefinition | ScriptableObject: class name key and a 99-level stat table |
| PartyCharacterDefinition | ScriptableObject: name, class, initial level, portrait, Character prefab, six starting equipment items |
| PartyMember | Session data: stats, health/mana, experience, class, portrait, prefab, equipment |
| PartyService | Member list, data changes, starting equipment grants, equipping; Changed event |
| PartyFormation | Scene instances, leader, movement chain, switching |
| PlayerController | Input, movement buffer, control locks, formation commands |
| Character | Binds an instance to PartyMember, facing, leader mode, interaction scanning/execution |
| CharacterMovementController | Rigidbody2D movement, steps, obstacles, teleportation, cancellation |
| CharacterAnimationController | Passes facing and movement to Animator |
| CharacterInteractionsController | Finds an available object in front of the leader and manages focus |

`Assets/Prefabs/Player.prefab` contains no character body, Animator, or sprite: it has PlayerController and PartyFormation. Do not manually place party characters inside Player. PartyFormation instantiates them from `PartyMember.CharacterPrefab`, owns a separate root of scene objects, and synchronizes Player's position for the camera.

## Existing characters and classes

Definitions: `Assets/Resources/Party`. Gameplay prefabs: `Assets/Prefabs/Characters`. Animations: `Assets/Animations/Characters/Darius` and `Shaya`.

| Character | Class / level | Initial strength/agility/intellect | Equipment | Calculated HP / MP / damage |
| --- | --- | --- | --- | --- |
| Darius | Necromancer / 1 | 2 / 2 / 3 | Scythe | 5 / 1 / 9 |
| Shaya | Hunter / 1 | 3 / 5 / 2 | Spear | 10 / 0 / 9 |

Other starting slots are empty; starting experience is 0. Portrait and prefab are assigned independently in the character definition. Editing an asset affects newly created members; do not expect an existing PartyMember to automatically reread its initial data.

| Class | Intellect, levels 1 → 99 | Strength | Agility |
| --- | --- | --- | --- |
| Warrior | 1 → 40 | 5 → 95 | 3 → 70 |
| Hunter | 2 → 55 | 3 → 60 | 5 → 95 |
| Necromancer | 3 → 75 | 2 → 55 | 2 → 55 |

Current class tables use linear progression with rounding; edit the `.asset` tables themselves. `levels[0]` is level 1, and `levels[98]` is level 99. OnValidate enforces 99 entries and clamps each stat to 1–99. A newly created class defaults to Warrior curves, so edit the entire table for a new class.

`Experience.asset` is shared: 100 at levels 1–98, 0 at level 99. This is the next-level requirement, not cumulative experience. The values of 100 after level 1 are placeholders. `SetExperience` changes the number without increasing the level; automatic leveling and experience awards are not implemented.

## Formulas and limits

PartyRules sets the maximum party size to three, levels to 1–99, and stats to 1–99. For current strength S: `MaxHealth = clamp(round(15 + 9984 × (S−5) × abs(S−5) / 94²), 1, 9999)`. For intellect I: `MaxMana = clamp(round(999 × I² / 99²), 0, 999)`. These formulas use System.Math.Round with MidpointRounding.AwayFromZero.

Initial health and mana are full. Reducing a maximum clamps the current value; increasing it does not automatically heal or restore mana. Unarmed damage equals current strength; one-handed weapons give `attack × strength`; two-handed weapons give `floor(attack × strength × 1.5)`. Damage is limited by int, not by 99. Agility is stored and displayed for future combat turn order; it does not change walking or animation speed. Current gameplay prefabs move at 4.5.

## Creating a class

1. In Project, use `Create > Party > Character Class` and save the asset, for example in `Assets/Resources/Party`.
2. Add `class.<id>.name` with RU/EN translations to GameText.json and assign Class Name Key.
3. Fill all 99 Levels entries: intellect, strength, agility. Check the first, last, and intermediate levels; changing two endpoints does not automatically recalculate a linear curve.
4. Assign the class in PartyCharacterDefinition. Changing formulas for all classes is a separate PartyRules code change, not a class asset setting.

## Creating a party character

1. Prepare a sprite sheet and portrait in `Assets/Sprites/Characters`. Existing pixel art uses 16×16 frames and 16 Pixels Per Unit; compare new artwork against the environment scale.
2. Copy Darius or Shaya into `Assets/Prefabs/Characters`, rename it, and replace its visuals. Retain Character, CharacterMovementController, CharacterAnimationController, CharacterInteractionsController, Rigidbody2D, Collider2D, and Animator. Assign Character.Visual to the correct SpriteRenderer.
3. Create a separate Animator Controller and clips in `Assets/Animations/Characters/<Name>`. The existing layout uses eight states: Idle/Move × Down/Up/Left/Right. Default parameters are `Direction X` and `Direction Y` (float), plus `Is Moving` (bool). Animator is on the object with CharacterAnimationController; clips address Visual through its hierarchy path.
4. Preserve the working sample's body and collider settings. Prefabs initially use PartyFollower; Character.SetControlled changes the root to Player for the leader. Interaction scanning uses Interactive and NPC, mask 1536 in current prefabs; scan distance is configured separately.
5. Use `Create > Party > Character`; set Character Name Key, Character Class, Initial Level, Portrait, Character Prefab, and Initial Weapon/Artifact/Amulet/Ring1/Ring2/Trinket. Add the name translation. Starting items must belong to Inventory/Items and fit their slots; a starting two-handed weapon cannot coexist with an artifact.
6. Add the character through `PartyService.Instance.TryAddMember(definition)`. It checks capacity and the whole loadout, grants the items to the shared inventory, and equips them. Repeated successful additions grant another loadout; this method does not provide general duplicate-character prevention.
7. For recruitment after dialogue, configure RecruitableNpc as described below. To change the starting character, account for PartyService loading `Party/Darius`: creating another asset alone is insufficient. The automatic service is created before the scene, so a scene copy is not the normal way to override its starting settings.

The overload `TryAddMember(nameKey, class, level, portrait, characterPrefab)` remains available for programmatic addition without granting a loadout. A characterPrefab is needed for world rendering. Change runtime data through PartyService so views receive Changed. The last member cannot be removed.

## Recruitable NPCs

Sample: `Assets/Prefabs/NPC/ShayaNPC.prefab`. This is a separate world object, not the instance that later walks in the party formation.

1. Copy the sample; assign a unique nonempty Npc Id, Dialog, Recruit (PartyCharacterDefinition), Initial Facing, and Solid Collider.
2. Keep the root and its solid collider on NPC. A separate Interaction child uses Interactive for detection; do not turn the solid body into a trigger.
3. Configure Animator, graphics, and CharacterMovementController. RecruitableNpc requires movement and animation controllers; this NPC does not require a separate Character component.
4. After the full DialogSequence completes, the NPC approaches the leader with its solid collider and obstacle checks temporarily disabled, then calls TryAddMember. On success it records Npc Id in SessionStateService and hides. Cancelling the dialogue does not recruit; a failed addition returns the NPC and allows retrying the conversation.
5. On returning to the scene, an already recruited NPC hides based on the session flag or a party member using the same prefab. Unique Npc Id values are required for independent events. A full party is an important manual-check scenario.

LostShayaTrigger is a specific encounter event, not a general quest system. See [dialogue](dialogue-localization.md).

## Party movement

The leader moves one grid cell in one of four directions. Solid Impassable/NPC obstacles are checked before and during movement. Followers repeat their neighbors' previous cells without another obstacle search. All members initially overlap; the chain unfolds gradually. A new step waits until the entire chain stops.

Leader switching cycles through Members: the old and new leaders exchange positions and facing, without reordering party data. Presses during movement accumulate until the chain stops. Menus, dialogue, and control locks clear pending commands. SessionStateService preserves the leader across scenes. Teleportation does not raise StepComplete, so spawning on a transition point should not immediately trigger a return transition.

Manual checks for a new character: world appearance and portrait; idle/walk in four directions; following and leader switching; interaction; scene transitions; every status field; granting exactly one loadout; full-party rejection; RU/EN. These checks were not run for this task.

## Three stats — 2026-10-19

Strength, agility, and intellect have base/current values in 1–99. Stamina was removed; Speed values were migrated to Agility in all three class tables. FormerlySerializedAs preserves old serialized speed values on rename. Changing current strength recalculates maximum HP and clamps current HP; increasing the maximum does not heal. The constructor initializes strength before health. MP still depends on intellect.

Status shows current stats only: white captions, white numbers when equal to base, green above, red below. Equipment shows current values in white. HP/MP use current/maximum; experience uses current/next-level requirement. Combat, effects, MP recovery per activation, and the progression tree are not implemented. Sources were reviewed statically; Unity, import, builds, and tests were not run.

## Combat sprite sheets — 2026-10-20

Combat sprite sheets: `Assets/Sprites/Characters/Huntress Combat Sprtitesheet.png` and `Assets/Sprites/Characters/Necromancer Combat Sritesheet.png`. Each .meta defines 12 sprites (_0–_11) at 32×32: Sprite Mode Multiple, 16 Pixels Per Unit, Point filtering, mipmaps disabled. No references to their GUIDs were found in other Assets files; the sheets are not assigned to prefabs or clips. The artwork does not imply implemented combat. Static inspection on 2026-10-20; Unity, import, builds, Play Mode, and tests were not run; slicing and rendering remain unverified.
