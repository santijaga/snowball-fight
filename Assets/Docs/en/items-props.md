# Items, equipment, and interactive props

[Contents](README.md) · [Русская версия](../items-props.md). Updated: 2026-10-21; static inspection.

## Items and database

Create ItemDefinition ScriptableObjects with `Create > Inventory > Item`, and ItemDatabase with `Create > Inventory > Database`. The runtime database loads automatically from `Assets/Resources/Inventory/Items.asset` and contains six items:

| Asset | Id | Category / Type | Settings |
| --- | --- | --- | --- |
| Potion | potion | Inventory / Consumable | Potion data/description; applying an effect is not implemented |
| LongSword | long_sword | Inventory / Equipment | Weapon, OneHanded, Attack 3 |
| Scythe | scythe | Inventory / Equipment | Weapon, TwoHanded, Attack 3; Darius's starting weapon |
| Spear | spear | Inventory / Equipment | Weapon, TwoHanded, Attack 2; Shaya's starting weapon |
| BronzeKey | bronze_key | KeyItem / Consumable | Key for assigned locks; Consumable alone does not imply automatic consumption |
| RustyKey | rusty_key | KeyItem / Consumable | Shaya's encounter/line conditions and assigned objects |

To add an item:

1. Create an `.asset` under `Assets/Resources/Inventory` and assign a unique nonempty Id. Filename and Id are separate values.
2. Add `item.<id>.name` and `item.<id>.description` to GameText.json. Set Name Key and Description (a key, not literal text).
3. Choose Category: Inventory or KeyItem. Equipment requires Inventory, Type = Equipment, valid Slot and Handedness; weapons use Attack.
4. Add **the asset itself** to Items in `Inventory/Items`. Saving it in Resources is insufficient. The database rejects null entries, duplicate IDs, empty keys, and invalid enum combinations.
5. Assign a grant source: Chest.Content, a character's starting loadout, or separate InventoryService logic. Inventory starts empty, then receives the equipment of added characters.

ItemDefinition.GetDescription appends localized slot, handedness, and attack information. There is no separate item-effect or accessory-stat-modifier system.

## Shared inventory and equipment

InventoryService owns stacks and publishes Changed. `SetQuantity(id, quantity)` sets total quantity; `GetQuantity` includes equipped copies, `GetEquippedQuantity` counts every slot across the party, and `GetAvailableQuantity` subtracts equipped copies. Quantity cannot be reduced below what is equipped. Internal exchange/grant operations validate every change before applying it, including int overflow and the same item being both consumed and granted.

Character slots: Weapon, Artifact, Amulet, Ring1, Ring2, Trinket. Items have one Ring type that fits either ring slot. Equip through PartyService.TryEquip; null removes equipment. Only a free copy of the correct asset from the shared database can be equipped. Unequipping does not create an item; it frees an already counted copy.

A two-handed weapon automatically unequips Artifact; equipping Artifact unequips a two-handed weapon. Artifacts work with one-handed weapons, and the artifact slot remains selectable. Removing a member leaves their items in shared inventory and frees them. Slot counts are explicitly coordinated across the enum, PartyMember, PartyService, and UI: adding a slot requires code and UI changes.

## Shared interaction mechanism

CharacterInteractionsController casts Physics2D.Raycast in front of the leader using the Interactive/NPC mask, focuses IPlayerVisible, and calls IInteractable.TryInteract on input. The contracts are independent of specific doors or dialogue lines.

InteractionMarker is a separate child prefab at `Assets/Prefabs/UI/InteractionMarker.prefab`. It finds the nearest parent MonoBehaviour implementing both contracts and subscribes to visibility/interaction/UIService events. Configure sprites before/after interaction and Should Toggle. Open UI hides the marker.

For a simple message prop, copy Closet/Cabinet/SecretDialog: assign DialogProp.Dialog Key, Sprite Renderer, Default/Interacted Sprite, and Should Toggle Back; preserve the detection layer/collider and add a marker if needed. Bed and Banquette do not contain DialogProp themselves. DialogProp changes appearance after successfully opening a message; it does not grant items. A new action type can implement the contracts in a separate cohesive component.

## Doors

Code: `Assets/Scripts/Props/Door.cs`. Samples: `Assets/Prefabs/Props/Room/Door.prefab` and `Cathedral/Door.prefab`.

| Field | Purpose |
| --- | --- |
| Door Id | Permanent unique ID of this placed door |
| Sprite Renderer, Default Sprite, Opened Sprite | Door appearance |
| Blocker | Separate obstacle object; disabled after opening |
| Is Locked | Initial lock state |
| Reset Open On Scene Reload | Reset only Opened for a new instance; defaults to true |
| Unlock Item | Required ItemDefinition from the database |
| Consume Unlock Item | Consume one free copy when unlocking |
| Show Locked Dialog, Locked Dialog Key | Show the configured message when the key is unavailable |
| Locked Dialog Service | Optional explicit service; otherwise current DialogService.Instance |
| Show Unlock Dialog, Unlock Dialog Key | Optional used-item message; default key dialog.unlock.used_item |

An unlocked door opens automatically when focused. A locked door unlocks by interaction with its assigned key; consumption defaults to off. The used-key message's name argument must be localized again when refreshed. Open UI and a disabled component prevent new actions.

DoorStateService preserves Locked/Opened across scenes. With Reset Open On Scene Reload enabled, the door returns closed but unlocked and opens automatically without another key consumption. Disabling/re-enabling the same component does not reset it. Door North West in Cathedral_Center has explicit ID `cathedral_center.door_north_west`; check current key references and instance overrides in the scene rather than relying on older devlog entries.

## Chests

Code: `Assets/Scripts/Props/Chest.cs`; sample: `Assets/Prefabs/Props/Cathedral/Chest.prefab`. Additional detailed guide: [docs/chests.md](../../../docs/chests.md). Its historical statement that the prefab was not placed does not describe current scenes.

1. Place a prefab instance; configure Closed/Opened Sprite and collider. Set a unique Chest Id on the instance; do not put a shared fixed ID in the base prefab.
2. Content is one item type from Items; Quantity sets its count. Null means an empty chest. The base prefab contains Potion ×1.
3. For a lock, enable Locked, assign Unlock Item, and choose Consume Unlock Item. The base prefab references BronzeKey, but Locked and consumption are off.
4. Found Dialog Key defaults to `dialog.chest.found`: `{0}` is the localized item name, `{1}` is quantity. Empty Dialog Key is `dialog.chest.empty`; Locked Dialog Key is `dialog.chest.locked`.
5. Reset Open On Scene Reload defaults to false. When enabled, returning resets only the appearance; the unlocked state and emptied contents persist.

Opening proceeds after successfully displaying the message. Unlocking, optional key consumption, and granting all content form one coordinated operation; inventory failure grants nothing. State is cleared before inventory subscribers are notified. Repeated interaction shows the empty message without further loot/consumption; the chest's solid collider remains.

## Identifiers and saving

DoorStateService and ChestStateService reject simultaneously registered objects with duplicate IDs. With no explicit ID, the scene path and hierarchy names/sibling indices are used. Renaming or reordering objects changes this ID; assign explicit IDs unique throughout the game for stable content. Change the ID when duplicating an instance.

Services store state only in memory. Future Save/Load must preserve inventory, equipment, party, session flags, doors, and chests coherently. Chest entries include Locked, Opened, ContentItemId, and ContentQuantity; doors include Locked/Opened. Restore state before binding scene objects. There is currently no complete save mechanism or complete new-game command inside one session; SessionStateService.ResetSession resets only its own portion.

## Current cathedral content settings

This table accounts for instance references and base Chest values (Quantity 1, Locked false). It describes static configuration, not a completed playthrough.

| Scene / object | Contents / lock |
| --- | --- |
| Cathedral_North_East_Closet / Chest | BronzeKey ×1, unlocked; instance Unlock Item is cleared |
| Cathedral_North_West_Room / Left Chest | LongSword ×1, unlocked |
| Cathedral_North_West_Room / Hidden Chest | RustyKey ×1, unlocked |
| Cathedral_North_West_Room / Right Chest | Potion ×1 inherited from the base prefab, unlocked |
| Cathedral_Center / Door North West | Locked; BronzeKey without consumption; locked/used-key messages enabled |
| Cathedral_Center / Door Central | Locked; RustyKey without consumption; locked/used-key messages enabled |
| Cathedral_Center / Door North East | Unlocked |

Manual checks: database membership, first/repeated collection, keys with/without consumption, available/equipped quantities, two-handed/artifact conflicts, scene returns with both reset settings, duplicate IDs, disabling the owner of an open dialogue, RU/EN. These were not run in this task.

## Weapon combat sprites — 2026-10-21

Standalone weapon combat sprites were added under `Assets/Sprites/Weapons` (filenames preserved):

- `Assets/Sprites/Weapons/Scythe Combat Sprite.png`
- `Assets/Sprites/Weapons/Long Sword Combat Sprite.png`
- `Assets/Sprites/Weapons/Spear Combat Sprite.png`

Each PNG is 16×16. The .meta settings are Texture Type Sprite (2D and UI), Sprite Mode Single, 16 Pixels Per Unit, Point filtering, and mipmaps disabled. Static GUID searches found no references in other Assets files; the sprites are not yet assigned to item assets, prefabs, or clips. ItemDefinition has no weapon sprite field. These images do not imply implemented weapon rendering in combat. Static inspection on 2026-10-21; Unity, import, builds, Play Mode, and tests were not run; in-game rendering remains unverified.
