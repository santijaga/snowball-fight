using System;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Party/Character Class")]
public sealed class CharacterClassDefinition : ScriptableObject
{
    [Serializable]
    private struct LevelStats
    {
        [Range(PartyRules.MinimumStat, PartyRules.MaximumStat)] public int intellect;
        [Range(PartyRules.MinimumStat, PartyRules.MaximumStat)] public int strength;
        [FormerlySerializedAs("speed")]
        [Range(PartyRules.MinimumStat, PartyRules.MaximumStat)] public int agility;
    }

    [SerializeField] private string classNameKey = "class.warrior.name";
    [SerializeField] private LevelStats[] levels = CreateDefaultLevels();

    public string ClassName => GameLocalization.Text(classNameKey);

    public int GetBaseIntellect(int level)
    {
        int index = Mathf.Clamp(level, 1, PartyRules.MaximumLevel) - 1;

        if (levels == null || index >= levels.Length)
        {
            throw new InvalidOperationException("Character class requires a table of 99 levels.");
        }

        return PartyRules.ClampStat(levels[index].intellect);
    }

    public int GetBaseStrength(int level)
    {
        int index = Mathf.Clamp(level, 1, PartyRules.MaximumLevel) - 1;

        if (levels == null || index >= levels.Length)
        {
            throw new InvalidOperationException("Character class requires a table of 99 levels.");
        }

        return PartyRules.ClampStat(levels[index].strength);
    }

    public int GetBaseAgility(int level)
    {
        int index = Mathf.Clamp(level, 1, PartyRules.MaximumLevel) - 1;

        if (levels == null || index >= levels.Length)
        {
            throw new InvalidOperationException("Character class requires a table of 99 levels.");
        }

        return PartyRules.ClampStat(levels[index].agility);
    }

    private static LevelStats[] CreateDefaultLevels()
    {
        LevelStats[] result = new LevelStats[PartyRules.MaximumLevel];

        for (int index = 0; index < result.Length; index++)
        {
            result[index].intellect = Mathf.RoundToInt(1f + index * 39f / 98f);
            result[index].strength = Mathf.RoundToInt(5f + index * 90f / 98f);
            result[index].agility = Mathf.RoundToInt(3f + index * 67f / 98f);
        }

        return result;
    }

    private void OnValidate()
    {
        if (levels == null || levels.Length != PartyRules.MaximumLevel)
        {
            LevelStats[] previous = levels;
            levels = CreateDefaultLevels();

            if (previous != null)
            {
                Array.Copy(previous, levels, Math.Min(previous.Length, levels.Length));
            }
        }

        for (int index = 0; index < levels.Length; index++)
        {
            levels[index].intellect = PartyRules.ClampStat(levels[index].intellect);
            levels[index].strength = PartyRules.ClampStat(levels[index].strength);
            levels[index].agility = PartyRules.ClampStat(levels[index].agility);
        }
    }
}
