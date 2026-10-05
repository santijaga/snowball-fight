using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

// Owns session language and preloads the official String Tables without blocking the main thread.
[DisallowMultipleComponent]
public sealed class GameLocalization : MonoBehaviour
{
    public const string TableName = "GameText";
    private static GameLocalization instance;
    private readonly Locale[] locales = new Locale[2];
    private readonly StringTable[] tables = new StringTable[2];
    private Coroutine loading;
    private bool languageSelected;

    public static bool IsReady => instance != null && instance.isActiveAndEnabled && instance.ready;
    public static event Action Changed;
    private bool ready;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        instance = null;
        Changed = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Create()
    {
        if (instance == null)
        {
            new GameObject(nameof(GameLocalization)).AddComponent<GameLocalization>();
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);

            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        if (instance != this)
        {

            return;
        }

        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        loading = StartCoroutine(Initialize());
    }

    private IEnumerator Initialize()
    {
        var initialization = LocalizationSettings.InitializationOperation;
        yield return initialization;

        if (initialization.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError("Localization initialization failed. Import Assets/Localization/GameText.json in the Editor.", this);
            yield break;
        }

        locales[0] = LocalizationSettings.AvailableLocales.GetLocale(new LocaleIdentifier("ru"));
        locales[1] = LocalizationSettings.AvailableLocales.GetLocale(new LocaleIdentifier("en"));

        for (int index = 0; index < locales.Length; index++)
        {
            if (locales[index] == null)
            {
                Debug.LogError("GameLocalization requires Russian and English locales.", this);
                yield break;
            }

            var table = LocalizationSettings.StringDatabase.GetTableAsync(TableName, locales[index]);
            yield return table;

            if (table.Status != AsyncOperationStatus.Succeeded || table.Result == null)
            {
                Debug.LogError($"GameLocalization could not load {TableName} for {locales[index].Identifier}.", this);
                yield break;
            }

            // The StringDatabase owns these handles and retains tables for the session.
            tables[index] = table.Result;
        }

        // Also reset when entering Play Mode with domain reload disabled; never use PlayerPrefs.
        if (!languageSelected)
        {
            LocalizationSettings.SelectedLocale = locales[Application.systemLanguage == SystemLanguage.Russian ? 0 : 1];
            languageSelected = true;
        }

        ready = true;
        loading = null;
        Changed?.Invoke();
    }

    public static string Text(string key, params object[] arguments)
    {
        if (!IsReady || string.IsNullOrWhiteSpace(key))
        {

            return string.Empty;
        }

        int index = LocalizationSettings.SelectedLocale == instance.locales[0] ? 0 : 1;
        StringTableEntry entry = instance.tables[index].GetEntry(key);

        if (entry == null || string.IsNullOrEmpty(entry.Value))
        {
            entry = instance.tables[1].GetEntry(key);
        }

        if (entry == null)
        {
            Debug.LogWarning($"Missing localization key: {key}");

            return $"[{key}]";
        }

        return entry.GetLocalizedString(arguments);
    }

    public static string LanguageName => IsReady
        ? Text(LocalizationSettings.SelectedLocale == instance.locales[0] ? "language.russian" : "language.english")
        : string.Empty;

    public static void MoveLanguage(int direction)
    {
        if (!IsReady || direction == 0)
        {

            return;
        }

        int index = LocalizationSettings.SelectedLocale == instance.locales[0] ? 0 : 1;
        LocalizationSettings.SelectedLocale = instance.locales[1 - index];
    }

    private void OnLocaleChanged(Locale locale)
    {
        if (ready && isActiveAndEnabled)
        {
            Changed?.Invoke();
        }
    }

    private void OnDisable()
    {
        if (instance != this)
        {

            return;
        }

        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;

        if (loading != null)
        {
            StopCoroutine(loading);
            loading = null;
        }

        ready = false;
        Changed?.Invoke();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
