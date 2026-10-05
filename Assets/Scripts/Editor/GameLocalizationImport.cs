using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

// The JSON is the editable source. Unity APIs create and maintain package-owned assets.
[InitializeOnLoad]
public static class GameLocalizationImport
{
    public const string SourcePath = "Assets/Localization/GameText.json";
    private const string OutputPath = "Assets/Localization/Generated";
    private static bool queued;

    [Serializable]
    private sealed class Catalog
    {
        public Translation[] entries;
    }

    [Serializable]
    private sealed class Translation
    {
        public string key;
        public string ru;
        public string en;
    }

    static GameLocalizationImport()
    {
        QueueImport();
    }

    public static void QueueImport()
    {
        if (!queued)
        {
            queued = true;
            EditorApplication.delayCall += ImportWhenReady;
        }
    }

    private static void ImportWhenReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.delayCall += ImportWhenReady;

            return;
        }

        queued = false;
        Import();
    }

    [MenuItem("Tools/Localization/Import Game Text")]
    private static void Import()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || !File.Exists(SourcePath))
        {

            return;
        }

        try
        {
            Catalog catalog = JsonUtility.FromJson<Catalog>(File.ReadAllText(SourcePath));
            Validate(catalog);
            EnsureFolder(OutputPath);
            LocalizationSettings settings = LocalizationEditorSettings.ActiveLocalizationSettings;

            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<LocalizationSettings>();
                AssetDatabase.CreateAsset(settings, $"{OutputPath}/LocalizationSettings.asset");
                LocalizationEditorSettings.ActiveLocalizationSettings = settings;
                LocalizationSettings.StartupLocaleSelectors.Clear();
                LocalizationSettings.StartupLocaleSelectors.Add(new SystemLocaleSelector());
                LocalizationSettings.StartupLocaleSelectors.Add(new SpecificLocaleSelector { LocaleId = new LocaleIdentifier("en") });
                EditorUtility.SetDirty(settings);
            }

            Locale russian = EnsureLocale("ru");
            Locale english = EnsureLocale("en");
            StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(GameLocalization.TableName);

            if (collection == null)
            {
                collection = LocalizationEditorSettings.CreateStringTableCollection(GameLocalization.TableName, OutputPath,
                    new List<Locale> { russian, english });
            }

            ImportTable(collection, russian, catalog.entries, true);
            ImportTable(collection, english, catalog.entries, false);
            AssetDatabase.SaveAssets();
        }
        catch (Exception exception)
        {
            Debug.LogError($"Game text import failed: {exception}");
        }
    }

    private static void Validate(Catalog catalog)
    {
        if (catalog?.entries == null || catalog.entries.Length == 0)
        {
            throw new InvalidDataException("GameText.json must contain entries.");
        }

        HashSet<string> keys = new(StringComparer.Ordinal);

        foreach (Translation entry in catalog.entries)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.key) || !keys.Add(entry.key)
                || string.IsNullOrWhiteSpace(entry.ru) || string.IsNullOrWhiteSpace(entry.en))
            {
                throw new InvalidDataException("Each translation must have a unique key and nonempty ru/en values.");
            }
        }
    }

    private static Locale EnsureLocale(string code)
    {
        LocaleIdentifier identifier = new(code);
        Locale locale = LocalizationEditorSettings.GetLocale(identifier);

        if (locale == null)
        {
            string path = $"{OutputPath}/{code}.asset";
            locale = AssetDatabase.LoadAssetAtPath<Locale>(path);

            if (locale == null)
            {
                locale = Locale.CreateLocale(identifier);
                AssetDatabase.CreateAsset(locale, path);
            }

            LocalizationEditorSettings.AddLocale(locale);
        }

        return locale;
    }

    private static void ImportTable(StringTableCollection collection, Locale locale, Translation[] entries, bool russian)
    {
        StringTable table = collection.GetTable(locale.Identifier) as StringTable;

        if (table == null)
        {
            table = collection.AddNewTable(locale.Identifier) as StringTable;
        }

        if (table == null)
        {
            throw new InvalidOperationException($"Could not create table for {locale.Identifier}.");
        }

        foreach (Translation translation in entries)
        {
            string value = russian ? translation.ru : translation.en;
            StringTableEntry entry = table.GetEntry(translation.key);

            if (entry == null || entry.Value != value || entry.IsSmart)
            {
                entry = table.AddEntry(translation.key, value);
                // String.Format placeholders are sufficient for current numeric labels.
                entry.IsSmart = false;
                EditorUtility.SetDirty(table);
                EditorUtility.SetDirty(collection.SharedData);
            }
        }

        if (!LocalizationEditorSettings.GetPreloadTableFlag(table))
        {
            LocalizationEditorSettings.SetPreloadTableFlag(table, true);
        }
    }

    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}

public sealed class GameTextSourcePostprocessor : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
    {
        if (Array.IndexOf(imported, GameLocalizationImport.SourcePath) >= 0
            || Array.IndexOf(moved, GameLocalizationImport.SourcePath) >= 0)
        {
            GameLocalizationImport.QueueImport();
        }
    }
}
