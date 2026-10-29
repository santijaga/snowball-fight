# Сцены, ввод, камера и интерфейс

[К оглавлению](README.md) · [English version](en/scenes-ui.md). Актуализировано: 2026-10-29; статический просмотр файлов.

## Сборка сцены

Игровые сцены перечислены в [каталоге](catalog.md) и `ProjectSettings/EditorBuildSettings.asset`. `Assets/Settings/Scenes/URP2DSceneTemplate.unity` — отдельный шаблон, не включённый в игровой список.

Для новой сцены удобнее копировать подходящую игровую сцену и настраивать её, учитывая перенесённые ID и ссылки. Нужны Player с PartyFormation, PlayerSceneEntry со ссылкой на него, активный StartingPoint для прямого входа, сценовые InputContextService/UIService/DialogService, UI и настроенная камера. Наличие префаба UI само по себе не заменяет все сценовые зависимости: InputContextService нужно обеспечить отдельно и сверить по рабочей сцене.

Окружение строится на Grid/Tilemap; тайлы и палитры находятся в `Assets/Tiles/Room`, `Cathedral`, `Dev`, спрайты — в `Assets/Sprites`. Твёрдым препятствиям нужен Impassable и нетриггерный 2D-коллайдер. Для новой комнаты сверить проходимость, расположение коллайдеров, слой и ограничитель камеры. Набор всех префабов, в том числе палитр, приведён в каталоге.

## Переходы и появление

StartingPoint задаёт позицию и Facing Direction для прямого запуска сцены без ожидающего перехода. Несколько активных StartingPoint вызывают предупреждение и выбор первого в порядке иерархии; отсутствие точки — ошибку без перестановки игрока.

TransitionPoint — самостоятельный префаб, не обычный триггер входа. Он подписывается на PlayerController.StepComplete и проверяет точное совпадение позиции законченного шага со своей позицией. Поэтому располагать его нужно на достижимой клетке.

| Поле | Как настроить |
| --- | --- |
| Id | Уникальный ID точки внутри текущей сцены |
| Target Scene | Имя либо полный путь доступной сцены из Build Settings |
| Target Id | Id активного TransitionPoint в целевой сцене |
| Facing Direction | Направление персонажа при появлении **на этой** точке |
| Spawn Offset | Локальное смещение точки появления; преобразуется TransformPoint |
| Player | Сценовый PlayerController |
| Transition Screen | Активный компонент с ISceneTransitionScreen, сейчас SceneRevealScreen |

Чтобы связать две комнаты, создать точку в каждой, назначить собственные Id и взаимные Target Scene/Target Id; проверить направление, смещение, доступную клетку и добавить новую сцену в Build Settings. Не копировать ссылки Player/Transition Screen из другой сцены без переназначения.

Переход блокирует управление, немедленно закрывает изображение экраном, пропускает два кадра для его показа, записывает назначение в SessionStateService и загружает сцену. PlayerSceneEntry вызывает инициализацию игрока в новой сцене. SessionStateService ищет целевой активный TransitionPoint, использует его SpawnPosition/SpawnFacing и очищает ожидающий переход. Если ID отсутствует или сцена не совпала, выводится ошибка; переход не заменяется молча StartingPoint. Дубликаты ID вызывают предупреждение и выбор первого.

Сервис хранит сведения об источнике/назначении и защищает от одновременных переходов. Отмена до запроса загрузки освобождает управление и экран. SceneRevealScreen реализует ISceneTransitionScreen; holdDuration и fadeDuration по умолчанию 0.5/0.5 секунды, значения экземпляра могут отличаться.

## Камера и слои

`Assets/Prefabs/Cameras/Cameras.prefab` содержит Main Camera, FollowCamera и Camera Confiner, использует Cinemachine. Для сцены сверять Follow target на Player и границы Confiner. PartyFormation.LateUpdate перемещает Player к ведущему, поэтому цель камеры остаётся стабильной при переключении персонажей. Сами Character создаются отдельно.

| Индекс | Layer | Роль |
| --- | --- | --- |
| 6 | Camera | Объекты камеры |
| 7 | No Collisions | Объекты без игровых столкновений |
| 8 | Player | Тело ведущего |
| 9 | Impassable | Стены и блокирующие части объектов |
| 10 | Interactive | Обнаружение интерактивных объектов |
| 11 | PartyFollower | Тела ведомых |
| 12 | NPC | Твёрдые тела NPC |

Sorting Layers — отдельная система: Void, Background, Mid, Foreground, Default. Character назначает sortingOrder ведущего 1, ведомых 0. Матрица `ProjectSettings/Physics2DSettings.asset` согласована с прохождением участников друг сквозь друга и столкновением PartyFollower с NPC. Не заменять слои «похожими» без просмотра ручных масок: движение явно использует Impassable/NPC, поиск взаимодействий — маску компонента.

## Ввод

Источник: `Assets/Settings/InputSystem_Actions.inputactions`. Соседний `InputSystem_Actions.cs` генерируется Unity, вручную не правится. Вводом владеет сценовый InputContextService. Активна одна карта — Player, UI или Dialog; временный контекст получают PushContext и освобождают IDisposable.Dispose. PlayerController.BlockControls отдельно блокирует команды мира и тоже возвращает освобождаемый handle.

| Действие | Клавиатура | Геймпад |
| --- | --- | --- |
| Player/Move | WASD / стрелки | Левый стик |
| Player/Interact | E | Южная кнопка |
| Player/Open Menu | Escape | Северная кнопка |
| Player/Next Character | ] | Правый бампер |
| Player/Previous Character | [ | Левый бампер |
| UI/Up, Down, Left, Right | WASD / стрелки | D-pad; UI/Navigate также содержит стики |
| UI/Select | Enter / Space | Южная кнопка |
| UI/Back | Backspace / Escape | Восточная кнопка |
| Dialog/Continue | E | Восточная кнопка |

В asset остались стандартные UI/Point, Click, ScrollWheel, Submit, Cancel, Navigate и привязки иных устройств. Это не означает поддержку мыши в игровом меню: MenuPanel отключает picking и стандартные события навигации, затем обрабатывает игровые действия. Схема подсказок выбирается по последней обработанной команде клавиатуры/мыши либо геймпада; наличие Gamepad без клавиатуры/мыши учитывается при начальном выборе.

PlayerController: bufferTime = 0.5, holdRepeatDelay = 0.2 в текущем префабе. Буферизация и переключение ждут остановки строя. Для новых действий менять Input Actions через редактор, обновлять потребителя/контексты и при необходимости экран управления; генерированный класс пересоздаёт Unity.

## UI Toolkit

Рабочий префаб — `Assets/Prefabs/UI/UI.prefab`; основные дочерние панели — MenuPanel, DialogPanel, SceneRevealPanel. Используется PanelRenderer с регистрацией UI reload callbacks, а не предположение о неизменном VisualElement на весь запуск. При изменении UXML нужно учитывать кодовые `Q(...name...)` и обязательные имена элементов.

| Ресурс в Assets/UI Toolkit | Потребитель / назначение |
| --- | --- |
| MenuTemplate.uxml, MenuSidebarTemplate.uxml | MenuPanel; рамка, пункты и общая навигация |
| PartyPanelTemplate.uxml, CharacterInfoCardTemplate.uxml | PartyPanelView; три карточки участников |
| InventoryScreenTemplate.uxml | InventoryPanelView; вкладки обычных/ключевых вещей и список |
| EquipmentScreenTemplate.uxml | EquipmentPanelView; участник, слоты, кандидаты |
| StatusScreenTemplate.uxml | StatusPanelView; выбор участника и его показатели |
| SettingsScreenTemplate.uxml | SettingsPanelView; язык |
| ControlsScreenTemplate.uxml | ControlsPanelView; подсказки управления, вкладки, продолжение |
| DialogTemplate.uxml | DialogView |
| SceneRevealTemplate.uxml | SceneRevealScreen |
| MenuStyles.uss | Общие стили, фокус, размеры и внешний вид |
| PanelSettings.asset | Настройки UI-панелей |

MenuPanel координирует Main/Inventory/Equipment/Status/Settings/Controls. Представления читают PartyService/InventoryService и локализацию и обновляются на события; меню не является хранилищем данных. UIService считает открытые элементы и публикует Opened/Closed/OpenCountChanged — это используется для блокировки взаимодействий и маркеров.

Инвентарь: Select переводит с вкладок в список, Back возвращает на вкладки, затем в общее меню; применения Potion по Select нет. Экипировка имеет уровни выбора участника, слота и предмета; Back поднимается по ним. Статус показывает текущие силу/ловкость/интеллект с цветом числа относительно базы (белый/зелёный/красный), ОЗ/ОМ/опыт и урон; область эффектов пока заготовка. Настройки меняют язык сразу. Экран управления автоматически появляется один раз за запуск и доступен из меню повторно.

Чтобы изменить UI, редактировать соответствующий UXML/USS, при изменении имён элементов обновлять представление, добавить переводы новым подписям, сверить ссылки префаба. Не вносить игровые данные в шаблон вместо сервисов. PNG-макеты в Assets/Docs — визуальные ориентиры, не выполняемый интерфейс.

Ручная проверка новой сцены/экрана: прямой вход, переход в обе стороны, правильный ведущий, остановка цепочки, камера и препятствия, открытие/закрытие всех вложенных экранов, возврат управления, смена языка, перезагрузка/отключение UI и отсутствие повторных подписок. Unity этой задачей не запускалась.

Изменение 2026-10-19: StatusStamina удалён, StatusSpeed заменён на StatusAgility; только числовая часть трёх статов использует rich text для цвета, остальные подписи — обычный текст. EquipmentStat0–5: ОЗ, ОМ, сила, ловкость, интеллект, урон; два ряда по три значения, числа всегда белые. GameText.json содержит status.agility вместо status.speed; три подписи статов принимают только {0}. Генерированные таблицы должен обновить штатный импорт локализации в Unity. Статический просмотр, без запуска Unity.

Актуализация 2026-10-29: в Build Settings четыре сцены собора, первая — Cathedral_Center. Удалён неиспользуемый набор анимаций прототипа; рабочие наборы Darius и Shaya сохранены. Устаревшие ссылки на удалённые сцены и графику исключены. Выполнен статический поиск по именам и GUID; Unity, импорт, сборки, Play Mode и тесты не запускались.
