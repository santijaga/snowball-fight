# Код и сервисы

[К оглавлению](README.md) · [English version](en/code-services.md). Актуализировано: 2026-10-19; назначение установлено по исходникам.

## Время жизни и зависимости

| Система | Создание и жизнь | Основные зависимости / данные |
| --- | --- | --- |
| SessionStateService | Автоматически BeforeSceneLoad, DontDestroyOnLoad | Ожидающий переход, сведения об источнике, выбранный PartyMember, множество нанятых Npc Id; при инициализации ищет точки в сцене игрока |
| InventoryService | Автоматически; EnsureCreated вызывается также PartyService; DontDestroyOnLoad | Resources Inventory/Items, стеки, учёт надетого через PartyService |
| PartyService | Автоматически BeforeSceneLoad, DontDestroyOnLoad | Resources Party/Darius и Party/Experience, InventoryService для начального комплекта |
| DoorStateService | Автоматически BeforeSceneLoad, DontDestroyOnLoad | Записи по ID, владельцы привязки, Locked/Opened |
| ChestStateService | Автоматически BeforeSceneLoad, DontDestroyOnLoad | Записи по ID, владельцы привязки, Locked/Opened/ContentItemId/ContentQuantity |
| GameLocalization | Автоматически BeforeSceneLoad, DontDestroyOnLoad | Unity Localization, GameText ru/en, асинхронная готовность и Changed; наружу предоставлен статический API |
| InputContextService | Сценовый компонент, DefaultExecutionOrder −100 | Собственный экземпляр генерированных InputSystem_Actions, стек контекстов, схема устройства |
| UIService | Сценовый компонент | Набор зарегистрированных открытых MonoBehaviour, события состояния UI |
| DialogService | Сценовый компонент со ссылкой DialogView | InputContextService, UIService, GameLocalization; текущее сообщение/последовательность и callback владельца |

Singleton Instance у большинства систем — текущая ссылка, а не гарантия их доступности при отключении/разрушении. Автоматические сервисы сбрасывают static на SubsystemRegistration и не требуют ручных экземпляров на сценах. Сценовые сервисы нужно обеспечить в каждой игровой сцене; их ссылки после загрузки другой сцены должны быть получены заново либо переназначены сценовыми объектами.

Нет центрального DI-контейнера, единого save-файла, менеджера квестов или общего графа сюжетных событий. `_GlobalServices` содержит также обычные определения и структуры данных — название папки не делает каждый файл сервисом.

## Каталог собственного C#

Все пути следующей таблицы указаны относительно `Assets/Scripts/`.

| Файл | Ответственность |
| --- | --- |
| `_GlobalServices/CharacterClassDefinition.cs` | Определение класса, таблица четырёх характеристик по уровням, OnValidate |
| `_GlobalServices/ExperienceTable.cs` | Общие пороги опыта на 99 уровней |
| `_GlobalServices/PartyCharacterDefinition.cs` | Исходные данные персонажа и его комплект |
| `_GlobalServices/PartyMember.cs` | Данные участника в памяти, расчёт урона, шесть слотов |
| `_GlobalServices/PartyRules.cs` | Диапазоны и формулы максимальных ОЗ/ОМ |
| `_GlobalServices/PartyService.cs` | Состав, изменения характеристик, добавление, удаление и экипировка |
| `_GlobalServices/ItemDefinition.cs` | Предмет и перечисления категорий, типов, слотов и рук |
| `_GlobalServices/ItemDatabase.cs` | Список определений, поиск Id, валидация базы |
| `_GlobalServices/InventoryService.cs` | Общий инвентарь, свободное/надетое количество, атомарные операции |
| `_GlobalServices/SessionStateService.cs` | Сессионные данные и выбор точки появления |
| `_GlobalServices/DoorStateService.cs` | Состояние дверей, привязки и защита дубликатов |
| `_GlobalServices/ChestStateService.cs` | Состояние сундуков, лут, привязки и защита дубликатов |
| `Characters/Character.cs` | Связь PartyMember с телом, направление и режим управления |
| `Characters/CharacterMovementController.cs` | Rigidbody2D-движение, препятствия и завершение шага |
| `Characters/CharacterAnimationController.cs` | Направление/движение → параметры Animator |
| `Characters/CharacterInteractionsController.cs` | Поиск объекта перед персонажем, фокус, TryInteract |
| `Characters/RecruitableNpc.cs` | Диалог NPC, блокировка, подход и вступление в группу |
| `Characters/LostShayaTrigger.cs` | Конкретная встреча с оставленной Шаей, поиск пути и передача разговора |
| `Player/PlayerController.cs` | Игровой ввод, буфер, блокировки, команды строю |
| `Player/PartyFormation.cs` | Сценовые Character, маршрут ведомых и смена ведущего |
| `Props/IInteractable.cs` | Контракт попытки взаимодействия и события |
| `Props/IPlayerVisible.cs` | Контракт фокуса/видимости игроком |
| `Props/InteractionMarker.cs` | Представление доступности и факта взаимодействия |
| `Props/DialogProp.cs` | Одна реплика и возможная смена спрайта |
| `Props/Door.cs` | Замок, ключ, диалоги, открытие и Blocker |
| `Props/Chest.cs` | Замок, выдача лута, сообщения и спрайт сундука |
| `Dialog/DialogSequence.cs` | Ресурс разговора, строки и условия |
| `Dialog/DialogService.cs` | Открытие, продолжение, фильтрация и отмена диалогов |
| `Dialog/DialogView.cs` | Отображение и локализация диалогового окна |
| `Input/InputContext.cs` | Enum Player / Dialog / UI |
| `Input/InputContextService.cs` | Экземпляр Actions, включение одной карты и временные контексты |
| `Localization/GameLocalization.cs` | Загрузка таблиц, Text, язык, Changed |
| `Editor/GameLocalizationImport.cs` | Редакторский импорт JSON; также содержит GameTextSourcePostprocessor |
| `UI/UIService.cs` | Реестр открытых окон |
| `UI/MenuPanel.cs` | Навигация главного/вложенных экранов, стартовое управление |
| `UI/PartyPanelView.cs` | Три карточки группы |
| `UI/InventoryPanelView.cs` | Вкладки, список предметов и описания |
| `UI/EquipmentPanelView.cs` | Выбор участника, слота и экипировки |
| `UI/StatusPanelView.cs` | Подробные показатели участника |
| `UI/SettingsPanelView.cs` | Переключение языка |
| `UI/ControlsPanelView.cs` | Экран управления и кнопка продолжения |
| `UI/SceneRevealScreen.cs` | Экран закрытия/раскрытия сцены, ISceneTransitionScreen |
| `ISceneTransitionScreen.cs` | Небольшой контракт экрана перехода |
| `TransitionPoint.cs` | Настраиваемый переход по завершённому шагу |
| `StartingPoint.cs` | Место и направление прямого входа |
| `SpawnFacingDirection.cs` | Enum направления появления и перевод в Vector2 |
| `PlayerSceneEntry.cs` | Передача сценового игрока в SessionStateService |

Отдельно: `Assets/Settings/InputSystem_Actions.cs` — генерированный класс ввода, редактировать только `.inputactions` и регенерировать редактором. Код Unity/пакетов в Library/PackageCache и ресурсы TextMesh Pro не являются собственными игровыми системами.

## Связи, важные при расширении

PartyService.Changed обновляет строй и UI. InventoryService.Changed обновляет предметы/кандидаты экипировки; операции экипировки уведомляют обе стороны. GameLocalization.Changed обновляет подписи и открытые сообщения. UIService сообщает об открытых окнах, чтобы маркеры/инициаторы учитывали занятость интерфейса.

Владелец диалога отменяет именно свой callback, владелец контекста и блокировки освобождает свой handle. Не хранить временную блокировку без обработки OnDisable/OnDestroy. Отключённый компонент не должен начинать новое действие; прерванное действие должно восстанавливать занятые ресурсы. Эти требования учитывать при добавлении новых инициаторов, без обязательного дробления каждой операции на новый сервис.

Актуализация 2026-10-19: характеристики — сила/ловкость/интеллект (1–99); ОЗ от текущей силы, ОМ от интеллекта. CharacterClassDefinition хранит intellect/strength/agility на 99 уровней; PartyMember/PartyService используют CurrentAgility/SetCurrentAgility вместо Speed, API выносливости удалён. Warrior, Hunter, Necromancer сохраняют прежние таблицы силы/интеллекта, скорость перенесена в ловкость. Дариус: 2/2/3, ОЗ 5, ОМ 1, урон 9; Шая: 3/5/2, ОЗ 10, ОМ 0, урон 9 (сила/ловкость/интеллект). Статус показывает только текущие статы с цветом чисел относительно базы, снаряжение — белые текущие значения. Бой и эффекты остаются планами. Статический просмотр исходников; Unity, импорт, сборки, Play Mode и тесты не запускались.
