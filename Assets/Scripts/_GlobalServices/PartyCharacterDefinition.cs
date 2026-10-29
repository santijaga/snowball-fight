using UnityEngine;

[CreateAssetMenu(menuName = "Party/Character")]
public sealed class PartyCharacterDefinition : ScriptableObject
{
    [SerializeField] private string characterNameKey = "character.darius.name";
    [SerializeField] private CharacterClassDefinition characterClass;
    [SerializeField, Range(1, PartyRules.MaximumLevel)] private int initialLevel = 1;
    [SerializeField] private Sprite portrait;
    [SerializeField] private Character characterPrefab;

    [SerializeField] private ItemDefinition initialWeapon;
    [SerializeField] private ItemDefinition initialArtifact;
    [SerializeField] private ItemDefinition initialAmulet;
    [SerializeField] private ItemDefinition initialRing1;
    [SerializeField] private ItemDefinition initialRing2;
    [SerializeField] private ItemDefinition initialTrinket;

    public ItemDefinition GetInitialEquipment(EquipmentSlot slot)
    {

        return slot switch
        {
            EquipmentSlot.Weapon => initialWeapon,
            EquipmentSlot.Artifact => initialArtifact,
            EquipmentSlot.Amulet => initialAmulet,
            EquipmentSlot.Ring1 => initialRing1,
            EquipmentSlot.Ring2 => initialRing2,
            EquipmentSlot.Trinket => initialTrinket,
            _ => null
        };
    }

    public string CharacterNameKey => characterNameKey;
    public string CharacterName => GameLocalization.Text(characterNameKey);
    public CharacterClassDefinition CharacterClass => characterClass;
    public int InitialLevel => Mathf.Clamp(initialLevel, 1, PartyRules.MaximumLevel);
    public Sprite Portrait => portrait;
    public Character CharacterPrefab => characterPrefab;
}
