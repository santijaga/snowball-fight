using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public sealed class PartyMember
{
    [SerializeField] private string characterNameKey;
    [SerializeField] private CharacterClassDefinition characterClass;
    [SerializeField] private Sprite portrait;
    [SerializeField] private int level;
    [SerializeField] private int experience;
    [SerializeField] private int currentHealth;
    [SerializeField] private int baseIntellect;
    [SerializeField] private int currentIntellect;
    [SerializeField] private int currentMana;
    [SerializeField] private int baseStrength;
    [SerializeField] private int currentStrength;
    private readonly ItemDefinition[] equipment = new ItemDefinition[6];
    private readonly ExperienceTable experienceTable;
    private readonly Character characterPrefab;
    [FormerlySerializedAs("baseSpeed")]
    [SerializeField] private int baseAgility;
    [FormerlySerializedAs("currentSpeed")]
    [SerializeField] private int currentAgility;

    public Character CharacterPrefab => characterPrefab;
    public int BaseAgility => PartyRules.ClampStat(baseAgility);
    public int CurrentAgility => PartyRules.ClampStat(currentAgility);

    internal void SetCurrentAgility(int value)
    {
        currentAgility = PartyRules.ClampStat(value);
    }

    public int BaseStrength => PartyRules.ClampStat(baseStrength);
    public int CurrentStrength => PartyRules.ClampStat(currentStrength);
    public int Damage
    {
        get
        {
            ItemDefinition weapon = GetEquipment(EquipmentSlot.Weapon);
            long damage = weapon == null ? CurrentStrength : (long)weapon.Attack * CurrentStrength;

            if (weapon != null && weapon.IsTwoHanded)
            {
                damage = damage * 3 / 2;
            }

            return (int)Math.Min(int.MaxValue, damage);
        }
    }

    public ItemDefinition GetEquipment(EquipmentSlot slot)
    {

        return Enum.IsDefined(typeof(EquipmentSlot), slot) ? equipment[(int)slot] : null;
    }

    internal void SetEquipment(EquipmentSlot slot, ItemDefinition item)
    {
        equipment[(int)slot] = item;

        if (item != null && slot == EquipmentSlot.Weapon && item.IsTwoHanded)
        {
            equipment[(int)EquipmentSlot.Artifact] = null;
        }

        if (item != null && slot == EquipmentSlot.Artifact
            && GetEquipment(EquipmentSlot.Weapon)?.IsTwoHanded == true)
        {
            equipment[(int)EquipmentSlot.Weapon] = null;
        }
    }

    internal void SetCurrentStrength(int value)
    {
        currentStrength = PartyRules.ClampStat(value);
        currentHealth = Mathf.Clamp(currentHealth, 0, MaximumHealth);
    }

    public string Name => GameLocalization.Text(characterNameKey);
    public CharacterClassDefinition CharacterClass => characterClass;
    public Sprite Portrait => portrait;
    public string ClassName => characterClass.ClassName;
    public int Level => Mathf.Clamp(level, 1, PartyRules.MaximumLevel);
    public int Experience => Mathf.Max(0, experience);
    public int ExperienceToNextLevel => experienceTable.GetExperienceToNextLevel(Level);
    public int MaximumHealth => PartyRules.CalculateMaximumHealth(CurrentStrength);
    public int CurrentHealth => Mathf.Clamp(currentHealth, 0, MaximumHealth);
    public int BaseIntellect => PartyRules.ClampStat(baseIntellect);
    public int CurrentIntellect => PartyRules.ClampStat(currentIntellect);
    public int MaximumMana => PartyRules.CalculateMaximumMana(CurrentIntellect);
    public int CurrentMana => Mathf.Clamp(currentMana, 0, MaximumMana);

    internal PartyMember(string nameKey, CharacterClassDefinition definition, int initialLevel,
        ExperienceTable progression, Sprite portrait = null, Character characterPrefab = null)
    {
        if (string.IsNullOrWhiteSpace(nameKey) || definition == null || progression == null)
        {
            throw new ArgumentException("A party member requires a name, class and experience table.");
        }

        characterNameKey = nameKey;
        characterClass = definition;
        this.portrait = portrait;
        this.characterPrefab = characterPrefab;
        experienceTable = progression;
        level = Mathf.Clamp(initialLevel, 1, PartyRules.MaximumLevel);
        baseIntellect = definition.GetBaseIntellect(level);
        currentIntellect = baseIntellect;
        currentMana = MaximumMana;
        baseStrength = definition.GetBaseStrength(level);
        currentStrength = baseStrength;
        currentHealth = MaximumHealth;
        baseAgility = definition.GetBaseAgility(level);
        currentAgility = baseAgility;
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
