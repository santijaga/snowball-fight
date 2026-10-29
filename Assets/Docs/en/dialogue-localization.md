# Dialogue and localization

[Contents](README.md) · [Русская версия](../dialogue-localization.md). Updated: 2026-10-18; static inspection. Import and rendering were not verified.

## Where text lives

**The editable source of all game translations is `Assets/Localization/GameText.json`.** Its root is an object containing an `entries` array:

```json
{
  "entries": [
    { "key": "dialog.example.greeting", "ru": "Привет.", "en": "Hello." }
  ]
}
```

This illustrates the structure; do not replace the existing file with this example. Each entry needs a unique nonempty key and nonempty ru/en values. Main key families include `character.*`, `class.*`, `item.*`, `dialog.*`, `menu.*`, `party.*`, and `equipment.*`; refer to the actual JSON keys and avoid duplicates.

Assets and code store keys, not Russian text. Dynamic parameters use `{0}`, `{1}`, in the same order in both languages. Formatting uses String.Format; imported entries have Smart Strings disabled. Literal braces must be escaped according to String.Format rules.

## Updating translations

1. Add or edit GameText.json entries, preserving valid JSON and CRLF.
2. Wait for automatic import in the editor. `Assets/Scripts/Editor/GameLocalizationImport.cs` queues import after editor scripts load and after the source JSON is changed or moved. It waits for compilation/updating to finish and for Play Mode to end.
3. If needed, run `Tools > Localization > Import Game Text` in Edit Mode. Complete import before starting the game or building.
4. The importer validates the whole catalog, creates missing ru/en Locale assets, LocalizationSettings, and the GameText collection, then adds keys and updates values while retaining existing numeric IDs. Tables are marked for preload. **Removing an entry from JSON does not remove it from the table:** current ImportTable only adds/updates entries. Renaming a key requires updating every reference; references are not migrated automatically.
5. Do not manually edit generated `.asset` files or Addressables. Translations changed only in Localization Tables are overwritten by the next JSON import. Import errors appear in Console as `Game text import failed`.
6. Manually check RU/EN in the affected window and language switching while the view is open. The editor was not started for this task.

`Assets/Localization/Generated` contains LocalizationSettings, en/ru, GameText (collection), GameText Shared Data, and GameText_en/GameText_ru. Earlier guide: [docs/localization.md](../../../docs/localization.md); current details are described here.

Adding a third language requires more than another JSON field: ru/en are hardcoded in the importer and language-selection logic. Code, Locale assets, loading, and settings UI require a separate change.

## Runtime localization

GameLocalization is created automatically before the scene and survives transitions. Initial language is Russian for a Russian system and English otherwise. Settings selection lasts only for the current session; PlayerPrefs is not used. `GameLocalization.IsReady` indicates preparation has finished. Until then, Text returns an empty string, and new menus/dialogues cannot open.

`GameLocalization.Text(key, arguments)` returns the current language's text, falling back to English if missing. If neither table contains the key, it returns `[key]` and logs a warning. `Changed` fires when ready and after language changes. Views subscribe when enabled and unsubscribe when disabled. Dialogue `Func<object[]>` arguments allow localized names/parameters to be recalculated when text refreshes.

## Single messages and sequences

| Mechanism | Configured in | Purpose |
| --- | --- | --- |
| DialogProp | Object's Dialog Key | Short environmental message, optional sprite change |
| Door / Chest | Component's message keys | Lock, used item, found item, empty chest |
| DialogSequence | ScriptableObject with Lines array | Conversation with speaker names, line filtering, completion result |
| DialogService | Scene service referencing DialogView | Input context, opening, continuing, cancellation, callbacks |
| DialogView | UI Toolkit / DialogTemplate | Key/argument display, localization, open-window registration |

A single message uses `DialogService.TryShowDialog(dialogKey, onCompleted, arguments)`. Its callback runs when closed and does not distinguish natural completion from forced hiding. Sequences have a separate `Action<bool>`: true only after continuing past the last available line; hiding/disabling counts as cancellation.

DialogService accepts one dialogue at a time. It needs an active service, ready DialogView and GameLocalization, UIService, and InputContextService. Individual initiators may additionally prohibit starting while another UI is open. Dialogue acquires a temporary Dialog context, released by its owner when finished. Continue uses `Dialog/Continue`, currently E on keyboard and the east gamepad button; do not substitute UI/Select.

## Adding a conversation

1. Add speaker-name keys (`character.<id>.name`) and line-text keys (`dialog.<topic>.<line>`) to GameText.json with both translations. Do not duplicate the speaker name inside the line text: `dialog.sequence.line` formats the name and line separately.
2. Use `Create > Dialog > Sequence`, normally saving under `Assets/Resources/Dialogs`. Resources placement alone does not start a conversation; an initiator must reference it.
3. Fill Lines in order. Every line requires Speaker Key and Text Key, including conditional lines. An empty sequence or any invalid line invalidates the entire asset.
4. Set Condition to Always, HasItem, or MissingItem. The last two require an Item reference. If inventory is missing/disabled, conditional lines do not play; that is not treated as a missing item. The condition checks total quantity, not only free copies.
5. Set Start Condition to Always, PlayerInteraction, or NpcApproach. This applies **together with** the item condition. StartMode is explicitly supplied at launch and remains fixed until the conversation ends; an introduction does not automatically imply NpcApproach.
6. For a recruitable NPC, assign Dialog in RecruitableNpc. Another initiator can call `TryShowSequence(sequence, onFinished, introduction, startMode)` and own its action's cancellation/locks. There is no generic component for an arbitrary talking NPC without recruitment: RecruitableNpc includes joining the party, while DialogProp supports one message.
7. Conditions are checked before the first display and whenever advancing to another line. Ineligible lines are skipped; if none qualify, launch returns false without opening a window. Introduction is prepended without copying the main asset.
8. Cancel your sequence with `CancelSequence` using the same owner callback; for a single message, use HideDialog with its callback. The parameterless HideDialog closes the current window regardless of ownership and requires an appropriate calling context.

There are no answer choices, branching graphs, node jumps, quest variables, or arbitrary per-line commands. Current conditional dialogue is a linear list filtered by item and start mode. The initiator implements story actions after the conversation.

## Current example: Shaya

`ShayaNPC.prefab`: Npc Id `shaya.introduction`, Dialog `ShayaIntroduction.asset`, Recruit `Party/Shaya.asset`. Normal interaction uses PlayerInteraction. Line .15 about the wall uses MissingItem for RustyKey and is skipped when the key is present.

Cathedral_North_West_Room contains LostShayaTrigger with Player, NPC, Required Item = RustyKey, Introduction = ShayaCatchUp, and Search Radius = 32. It checks the stopped leader's position inside BoxCollider2D rather than relying on OnTriggerEnter. It requires the key, an active available unrecruited NPC, no open UI, and Player context.

The trigger finds a shortest four-direction route to a free cell adjacent to the leader, accounting for obstacles, the NPC's solid collider dimensions, and party characters. It blocks controls and menus during the approach, then passes the conversation to RecruitableNpc with introduction and NpcApproach. If no route exists, no locks are acquired; leaving and re-entering the zone allows retrying. Interrupted approaches return the NPC; after conversation handoff, RecruitableNpc owns the lifecycle.

`ShayaCatchUp` contains one opening line. For NpcApproach, `ShayaIntroduction` skips .01, uses shared .02–.05, substitutes “And who are you?” for normal .06, uses shared .07–.09, then two separate closing lines instead of .10–.14. This English sentence describes the Russian line's meaning; the exact localized text and keys are in the assets/JSON. After a successful conversation, the NPC performs the short approach used to join the party.

Manual scenarios: normal introduction with and without RustyKey; trigger approach with the key; no available route; cancellation/disabling; full party; scene re-entry after recruitment; RU/EN switching. These were not verified in this task.
