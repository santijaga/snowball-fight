using UnityEngine;

[CreateAssetMenu(menuName = "Party/Character")]
public sealed class PartyCharacterDefinition : ScriptableObject
{
    [SerializeField] private string characterNameKey = "character.darius.name";
    [SerializeField] private CharacterClassDefinition characterClass;
    [SerializeField, Range(1, PartyRules.MaximumLevel)] private int initialLevel = 1;
    [SerializeField] private Sprite portrait;

    public string CharacterNameKey => characterNameKey;
    public string CharacterName => GameLocalization.Text(characterNameKey);
    public CharacterClassDefinition CharacterClass => characterClass;
    public int InitialLevel => Mathf.Clamp(initialLevel, 1, PartyRules.MaximumLevel);
    public Sprite Portrait => portrait;
}
