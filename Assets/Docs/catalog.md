# Каталог ресурсов

[К оглавлению](README.md) · [English version](en/catalog.md). Снимок файлов рабочего дерева: 2026-10-29. Список составлен без запуска Unity; ссылки GUID разрешены по существующим `.meta`.

## Все префабы в Assets

Пути — относительно корня проекта. Компоненты в таблице — собственные скрипты, непосредственно записанные в YAML префаба; у вариантов/вложенных объектов дополнительно учитывать исходные префабы. Компоненты Unity и пакетов здесь не перечисляются как игровые сервисы.

| Префаб | Собственные компоненты в файле | Исходные/вложенные префабы |
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

Player — координатор группы; Characters — тела участников, NPC/ShayaNPC — отдельный нанимаемый NPC. Props/Room — мебель и дверь: Cabinet/Closet содержат DialogProp, Bed/Banquette — графику/коллайдеры без собственного поведения взаимодействия. Props/Cathedral — дверь и сундук, Props/Misc/SecretDialog — скрытая реплика. UI/UI собирает панели; InteractionMarker — дочерняя индикация. Environment/Grid — основа сетки. Префабы внутри Tiles относятся к палитрам/тайлам, их не следует автоматически считать готовой игровой комнатой.

## Все сцены

| Сцена | В Build Settings | Собственные компоненты непосредственно в YAML сцены |
| --- | --- | --- |
| `Assets/Scenes/Cathedral_Center_Room.unity` | Да | DialogService, DialogView, InputContextService, PlayerController, PlayerSceneEntry, SceneRevealScreen, UIService |
| `Assets/Scenes/Cathedral_Center.unity` | Да | DialogService, DialogView, InputContextService, PlayerController, PlayerSceneEntry, SceneRevealScreen, UIService |
| `Assets/Scenes/Cathedral_North_East_Closet.unity` | Да | DialogService, DialogView, InputContextService, PlayerController, PlayerSceneEntry, SceneRevealScreen, UIService |
| `Assets/Scenes/Cathedral_North_West_Room.unity` | Да | DialogService, DialogView, InputContextService, LostShayaTrigger, PlayerController, PlayerSceneEntry, RecruitableNpc, SceneRevealScreen, UIService |
| `Assets/Settings/Scenes/URP2DSceneTemplate.unity` | Нет (шаблон) |  |

Компоненты внутри экземпляров префабов могут отсутствовать в этом списке: Unity хранит ссылку на префаб и overrides. Первая сцена сборки — Cathedral_Center. Состав префабов каждой сцены:

| Сцена | Исходные префабы (уникальные пути, без числа экземпляров) |
| --- | --- |
| Cathedral_Center_Room | Assets/Prefabs/Cameras/Cameras.prefab; Assets/Prefabs/Environment/Grid.prefab; Assets/Prefabs/Player.prefab; Assets/Prefabs/StartingPoint.prefab; Assets/Prefabs/TransitionPoint.prefab; Assets/Prefabs/UI/UI.prefab |
| Cathedral_Center | Assets/Prefabs/Cameras/Cameras.prefab; Assets/Prefabs/Environment/Grid.prefab; Assets/Prefabs/Player.prefab; Assets/Prefabs/Props/Cathedral/Door.prefab; Assets/Prefabs/StartingPoint.prefab; Assets/Prefabs/TransitionPoint.prefab; Assets/Prefabs/UI/UI.prefab |
| Cathedral_North_East_Closet | Assets/Prefabs/Cameras/Cameras.prefab; Assets/Prefabs/Environment/Grid.prefab; Assets/Prefabs/Player.prefab; Assets/Prefabs/Props/Cathedral/Chest.prefab; Assets/Prefabs/StartingPoint.prefab; Assets/Prefabs/TransitionPoint.prefab; Assets/Prefabs/UI/UI.prefab |
| Cathedral_North_West_Room | Assets/Prefabs/Cameras/Cameras.prefab; Assets/Prefabs/Environment/Grid.prefab; Assets/Prefabs/NPC/ShayaNPC.prefab; Assets/Prefabs/Player.prefab; Assets/Prefabs/Props/Cathedral/Chest.prefab; Assets/Prefabs/StartingPoint.prefab; Assets/Prefabs/TransitionPoint.prefab; Assets/Prefabs/UI/UI.prefab |
| URP2DSceneTemplate |  |

## Настроенные переходы сцен

Значения извлечены из overrides экземпляров TransitionPoint. Это конфигурация, а не подтверждение проходимости маршрутов в игре.

| Сцена | Id | Target Scene | Target Id |
| --- | --- | --- | --- |
| Cathedral_Center_Room | toCenter | Cathedral_Center | toCenterRoom |
| Cathedral_Center | toWestRoom | Cathedral_North_West_Room | toCenter |
| Cathedral_Center | toCenterRoom | Cathedral_Center_Room | toCenter |
| Cathedral_Center | toCloset | Cathedral_North_East_Closet | toCenter |
| Cathedral_North_East_Closet | toCenter | Cathedral_Center | toCloset |
| Cathedral_North_West_Room | toCenter | Cathedral_Center | toWestRoom |

## Все определения Resources

| Ресурс | Тип / ключевые ссылки |
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

## Контроллеры и клипы анимации

В каждом из двух каталогов Characters/Darius и Characters/Shaya находятся собственный контроллер и восемь клипов Idle/Move для Down/Up/Left/Right. Игровые определения ссылаются на Characters/Darius и Characters/Shaya.

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

## UI и настройки

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

## Графика, тайлы и шрифты

Все исходные изображения и прочие ресурсы этих каталогов перечислены ниже; `.meta` не включены. Тайлы `.asset` перечислены отдельно для навигации по полному набору, без предположения об использовании каждого тайла в сценах.

### Assets/Sprites

Актуализация каталога спрайтов 2026-10-29: удалены неиспользуемый экспериментальный спрайт-лист персонажа и его .meta; ссылки на ресурс в игровых файлах до удаления отсутствовали. Выполнен статический поиск по имени и GUID; Unity, импорт, сборки, Play Mode и тесты не запускались.

Боевые спрайт-листы: `Assets/Sprites/Characters/Huntress Combat Sprtitesheet.png` и `Assets/Sprites/Characters/Necromancer Combat Sritesheet.png`. По .meta каждый содержит 12 спрайтов (_0–_11) размером 32×32: Sprite Mode Multiple, 16 Pixels Per Unit, Point, mipmaps отключены. Ссылок на их GUID в остальных файлах Assets не найдено; листы не подключены к префабам и клипам. Наличие графики не означает реализации боя. Статический просмотр 2026-10-20; Unity, импорт, сборки, Play Mode и тесты не запускались, нарезка и отображение не подтверждены.

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

#### Боевые спрайты оружия — 2026-10-21

В `Assets/Sprites/Weapons` добавлены отдельные боевые спрайты оружия (имена файлов сохранены):

- `Assets/Sprites/Weapons/Scythe Combat Sprite.png`
- `Assets/Sprites/Weapons/Long Sword Combat Sprite.png`
- `Assets/Sprites/Weapons/Spear Combat Sprite.png`

Каждое PNG имеет размер 16×16. По .meta: Texture Type Sprite (2D and UI), Sprite Mode Single, 16 Pixels Per Unit, фильтрация Point, mipmaps отключены. Статический поиск GUID не обнаружил ссылок в остальных файлах Assets; спрайты пока не подключены к ресурсам предметов, префабам или клипам. В ItemDefinition нет поля спрайта оружия. Наличие изображений не означает реализации отображения оружия в бою. Статический просмотр 2026-10-21; Unity, импорт, сборки, Play Mode и тесты не запускались, отображение в игре не подтверждено.

#### Фон собора для боя — 2026-10-21

`Assets/Sprites/Battle Backgrounds/Cathedral Backdrop Sprite.png` — фон собора для боя, PNG 472×232. По .meta: Sprite Mode Multiple с одним спрайтом `Cathedral Backdrop Sprite_0` на всё изображение (472×232), 16 Pixels Per Unit, Point, mipmaps отключены. Статический поиск GUID не обнаружил ссылок в остальных файлах Assets; фон пока не подключён к сценам, префабам или другим ресурсам. Наличие изображения не означает реализации боевой сцены. Статический просмотр 2026-10-21; Unity, импорт, сборки, Play Mode и тесты не запускались, отображение не подтверждено.

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

## Производные и сторонние ресурсы

`Assets/Localization/Generated` и `Assets/AddressableAssetsData` обслуживаются Unity Localization/Addressables; их наличие не означает ручного источника переводов. `Assets/TextMesh Pro` содержит ресурсы пакета; их не следует смешивать с собственными игровыми префабами/сервисами. Список всех зависимостей — Packages/manifest.json и packages-lock.json.

Каталог — поддерживаемый снимок, не автоматическая база редактора: при добавлении/удалении/переименовании ресурсов обновлять обе языковые версии в той же задаче.

Актуализация 2026-10-20: характеристики — сила/ловкость/интеллект (1–99); ОЗ от текущей силы, ОМ от интеллекта. CharacterClassDefinition хранит intellect/strength/agility на 99 уровней; PartyMember/PartyService используют CurrentAgility/SetCurrentAgility вместо Speed, API выносливости удалён. Warrior, Hunter, Necromancer сохраняют прежние таблицы силы/интеллекта, скорость перенесена в ловкость. Дариус: 2/2/3, ОЗ 5, ОМ 1, урон 9; Шая: 3/5/2, ОЗ 10, ОМ 0, урон 9 (сила/ловкость/интеллект). Статус показывает только текущие статы с цветом чисел относительно базы, снаряжение — белые текущие значения. Бой и эффекты остаются планами. Статический просмотр исходников; Unity, импорт, сборки, Play Mode и тесты не запускались.

Актуализация 2026-10-29: в Build Settings четыре сцены собора, первая — Cathedral_Center. Удалён неиспользуемый набор анимаций прототипа; рабочие наборы Darius и Shaya сохранены. Устаревшие ссылки на удалённые сцены и графику исключены. Выполнен статический поиск по именам и GUID; Unity, импорт, сборки, Play Mode и тесты не запускались.
