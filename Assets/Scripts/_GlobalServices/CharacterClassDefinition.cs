using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Party/Character Class")]
public sealed class CharacterClassDefinition : ScriptableObject
{
    [Serializable]
    private struct LevelStats
    {
        [Range(PartyRules.MinimumStat, PartyRules.MaximumStat)] public int stamina;
        [Range(PartyRules.MinimumStat, PartyRules.MaximumStat)] public int intellect;
    }

    [SerializeField] private string classNameKey = "class.warrior.name";
    [SerializeField] private LevelStats[] levels = CreateDefaultLevels();

    public string ClassName => GameLocalization.Text(classNameKey);

    public int GetBaseStamina(int level)
    {
        int index = Mathf.Clamp(level, 1, PartyRules.MaximumLevel) - 1;

        if (levels == null || index >= levels.Length)
        {
            throw new InvalidOperationException("Character class requires a table of 99 levels.");
        }

        return PartyRules.ClampStat(levels[index].stamina);
    }

    public int GetBaseIntellect(int level)
    {
        int index = Mathf.Clamp(level, 1, PartyRules.MaximumLevel) - 1;

        if (levels == null || index >= levels.Length)
        {
            throw new InvalidOperationException("Character class requires a table of 99 levels.");
        }

        return PartyRules.ClampStat(levels[index].intellect);
    }

    private static LevelStats[] CreateDefaultLevels()
    {
        LevelStats[] result = new LevelStats[PartyRules.MaximumLevel];

        for (int index = 0; index < result.Length; index++)
        {
            result[index].stamina = Mathf.RoundToInt(5f + index * 85f / 98f);
            result[index].intellect = Mathf.RoundToInt(1f + index * 39f / 98f);
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
            levels[index].stamina = PartyRules.ClampStat(levels[index].stamina);
            levels[index].intellect = PartyRules.ClampStat(levels[index].intellect);
        }
    }
}
