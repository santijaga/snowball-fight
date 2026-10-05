using System;
using UnityEngine;

[Serializable]
public sealed class PartyMember
{
    [SerializeField] private string characterNameKey;
    [SerializeField] private CharacterClassDefinition characterClass;
    [SerializeField] private Sprite portrait;
    [SerializeField] private int level;
    [SerializeField] private int experience;
    [SerializeField] private int baseStamina;
    [SerializeField] private int currentStamina;
    [SerializeField] private int currentHealth;
    [SerializeField] private int baseIntellect;
    [SerializeField] private int currentIntellect;
    [SerializeField] private int currentMana;
    private readonly ExperienceTable experienceTable;

    public string Name => GameLocalization.Text(characterNameKey);
    public CharacterClassDefinition CharacterClass => characterClass;
    public Sprite Portrait => portrait;
    public string ClassName => characterClass.ClassName;
    public int Level => Mathf.Clamp(level, 1, PartyRules.MaximumLevel);
    public int Experience => Mathf.Max(0, experience);
    public int ExperienceToNextLevel => experienceTable.GetExperienceToNextLevel(Level);
    public int BaseStamina => PartyRules.ClampStat(baseStamina);
    public int CurrentStamina => PartyRules.ClampStat(currentStamina);
    public int MaximumHealth => PartyRules.CalculateMaximumHealth(CurrentStamina);
    public int CurrentHealth => Mathf.Clamp(currentHealth, 0, MaximumHealth);
    public int BaseIntellect => PartyRules.ClampStat(baseIntellect);
    public int CurrentIntellect => PartyRules.ClampStat(currentIntellect);
    public int MaximumMana => PartyRules.CalculateMaximumMana(CurrentIntellect);
    public int CurrentMana => Mathf.Clamp(currentMana, 0, MaximumMana);

    internal PartyMember(string nameKey, CharacterClassDefinition definition, int initialLevel,
        ExperienceTable progression, Sprite portrait = null)
    {
        if (string.IsNullOrWhiteSpace(nameKey) || definition == null || progression == null)
        {
            throw new ArgumentException("A party member requires a name, class and experience table.");
        }

        characterNameKey = nameKey;
        characterClass = definition;
        this.portrait = portrait;
        experienceTable = progression;
        level = Mathf.Clamp(initialLevel, 1, PartyRules.MaximumLevel);
        baseStamina = definition.GetBaseStamina(level);
        currentStamina = baseStamina;
        currentHealth = MaximumHealth;
        baseIntellect = definition.GetBaseIntellect(level);
        currentIntellect = baseIntellect;
        currentMana = MaximumMana;
    }

    internal void SetCurrentStamina(int value)
    {
        currentStamina = PartyRules.ClampStat(value);
        currentHealth = Mathf.Clamp(currentHealth, 0, MaximumHealth);
    }

    internal void SetCurrentHealth(int value)
    {
        currentHealth = Mathf.Clamp(value, 0, MaximumHealth);
    }

    internal void SetCurrentIntellect(int value)
    {
        currentIntellect = PartyRules.ClampStat(value);
        currentMana = Mathf.Clamp(currentMana, 0, MaximumMana);
    }

    internal void SetCurrentMana(int value)
    {
        currentMana = Mathf.Clamp(value, 0, MaximumMana);
    }

    internal void SetExperience(int value)
    {
        experience = Mathf.Max(0, value);
    }

    internal void SetPortrait(Sprite value)
    {
        portrait = value;
    }
}
