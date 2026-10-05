using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Party/Experience Table")]
public sealed class ExperienceTable : ScriptableObject
{
    [Serializable]
    private struct LevelExperience
    {
        [Min(0)] public int experienceToNextLevel;
    }

    [SerializeField] private LevelExperience[] levels = CreateDefaultLevels();

    public int GetExperienceToNextLevel(int level)
    {
        int index = Mathf.Clamp(level, 1, PartyRules.MaximumLevel) - 1;

        if (index == PartyRules.MaximumLevel - 1)
        {

            return 0;
        }

        if (levels == null || index >= levels.Length)
        {
            throw new InvalidOperationException("Experience requires a table of 99 levels.");
        }

        return Mathf.Max(1, levels[index].experienceToNextLevel);
    }

    private static LevelExperience[] CreateDefaultLevels()
    {
        LevelExperience[] result = new LevelExperience[PartyRules.MaximumLevel];

        for (int index = 0; index < result.Length - 1; index++)
        {
            // Placeholder until the experience progression is designed.
            result[index].experienceToNextLevel = 100;
        }

        return result;
    }

    private void OnValidate()
    {
        if (levels == null || levels.Length != PartyRules.MaximumLevel)
        {
            LevelExperience[] previous = levels;
            levels = CreateDefaultLevels();

            if (previous != null)
            {
                Array.Copy(previous, levels, Math.Min(previous.Length, levels.Length));
            }
        }

        for (int index = 0; index < levels.Length - 1; index++)
        {
            levels[index].experienceToNextLevel = Mathf.Max(1, levels[index].experienceToNextLevel);
        }

        levels[levels.Length - 1].experienceToNextLevel = 0;
    }
}
