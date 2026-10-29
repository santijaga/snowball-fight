using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class PartyService : MonoBehaviour
{
    public static PartyService Instance { get; private set; }

    [SerializeField] private PartyCharacterDefinition startingCharacter;
    [SerializeField] private ExperienceTable experienceTable;
    // Runtime data is owned by the service, not editable through the Inspector.
    private readonly List<PartyMember> members = new();
    private IReadOnlyList<PartyMember> readOnlyMembers;

    public IReadOnlyList<PartyMember> Members => readOnlyMembers;
    public event Action Changed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        Instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateParty()
    {
        if (Instance != null)
        {

            return;
        }

        GameObject partyObject = new(nameof(PartyService));
        partyObject.AddComponent<PartyService>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);

            return;
        }

        if (startingCharacter == null)
        {
            startingCharacter = Resources.Load<PartyCharacterDefinition>("Party/Darius");
        }

        if (experienceTable == null)
        {
            experienceTable = Resources.Load<ExperienceTable>("Party/Experience");
        }

        if (startingCharacter == null || startingCharacter.CharacterClass == null
            || string.IsNullOrWhiteSpace(startingCharacter.CharacterNameKey) || experienceTable == null)
        {
            Debug.LogError("PartyService requires a valid Party/Darius character and Party/Experience resource.", this);
            Destroy(gameObject);

            return;
        }

        readOnlyMembers = members.AsReadOnly();
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InventoryService.EnsureCreated();

        if (!TryAddMember(startingCharacter))
        {
            Debug.LogError("Unable to add the starting character and initial equipment.", this);
        }
    }

    public bool SetCurrentAgility(PartyMember member, int value)
    {
        if (!CanChange(member))
        {

            return false;
        }

        member.SetCurrentAgility(value);
        Changed?.Invoke();

        return true;
    }

    public bool TryAddMember(PartyCharacterDefinition definition)
    {
        InventoryService inventory = InventoryService.Instance;

        if (!isActiveAndEnabled || Instance != this || members.Count >= PartyRules.MaximumMembers
            || definition == null || definition.CharacterClass == null
            || string.IsNullOrWhiteSpace(definition.CharacterNameKey) || inventory == null)
        {

            return false;
        }

        ItemDefinition[] loadout = new ItemDefinition[6];

        for (int index = 0; index < loadout.Length; index++)
        {
            EquipmentSlot slot = (EquipmentSlot)index;
            loadout[index] = definition.GetInitialEquipment(slot);

            if (loadout[index] != null && !loadout[index].Fits(slot))
            {

                return false;
            }
        }

        if (loadout[0]?.IsTwoHanded == true && loadout[1] != null)
        {

            return false;
        }

        PartyMember member = new(definition.CharacterNameKey, definition.CharacterClass,
            definition.InitialLevel, experienceTable, definition.Portrait, definition.CharacterPrefab);

        return inventory.TryGrantEquipment(loadout, () =>
        {
            members.Add(member);

            for (int index = 0; index < loadout.Length; index++)
            {
                member.SetEquipment((EquipmentSlot)index, loadout[index]);
            }

            Changed?.Invoke();
        });
    }

    public int GetEquippedQuantity(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {

            return 0;
        }

        int quantity = 0;

        foreach (PartyMember member in members)
        {
            for (int index = 0; index < 6; index++)
            {
                if (member.GetEquipment((EquipmentSlot)index)?.Id == itemId)
                {
                    quantity++;
                }
            }
        }

        return quantity;
    }

    public bool TryEquip(PartyMember member, EquipmentSlot slot, ItemDefinition item)
    {
        InventoryService inventory = InventoryService.Instance;

        if (!CanChange(member) || !Enum.IsDefined(typeof(EquipmentSlot), slot)
            || inventory == null || !inventory.isActiveAndEnabled)
        {

            return false;
        }

        if (item != null && (!item.Fits(slot)
            || !inventory.Database.TryGet(item.Id, out ItemDefinition known) || known != item))
        {

            return false;
        }

        if (member.GetEquipment(slot) == item)
        {

            return true;
        }

        if (item != null && inventory.GetAvailableQuantity(item.Id) < 1)
        {

            return false;
        }

        member.SetEquipment(slot, item);
        Changed?.Invoke();
        inventory.NotifyEquipmentChanged();

        return true;
    }

    public bool SetCurrentStrength(PartyMember member, int value)
    {
        if (!CanChange(member))
        {

            return false;
        }

        member.SetCurrentStrength(value);
        Changed?.Invoke();

        return true;
    }

    // nameKey is an entry key in GameText, not a translated display name.
    public bool TryAddMember(string nameKey, CharacterClassDefinition definition, int level, Sprite portrait = null, Character characterPrefab = null)
    {
        if (!isActiveAndEnabled || Instance != this || members.Count >= PartyRules.MaximumMembers
            || string.IsNullOrWhiteSpace(nameKey) || definition == null)
        {

            return false;
        }

        members.Add(new PartyMember(nameKey, definition, level, experienceTable, portrait, characterPrefab));
        Changed?.Invoke();

        return true;
    }

    public bool TryRemoveMember(PartyMember member)
    {
        if (!isActiveAndEnabled || Instance != this || members.Count <= 1 || !members.Remove(member))
        {

            return false;
        }

        Changed?.Invoke();
        InventoryService.Instance?.NotifyEquipmentChanged();

        return true;
    }

    public bool SetCurrentHealth(PartyMember member, int value)
    {
        if (!CanChange(member))
        {

            return false;
        }

        member.SetCurrentHealth(value);
        Changed?.Invoke();

        return true;
    }

    public bool SetCurrentIntellect(PartyMember member, int value)
    {
        if (!CanChange(member))
        {

            return false;
        }

        member.SetCurrentIntellect(value);
        Changed?.Invoke();

        return true;
    }

    public bool SetCurrentMana(PartyMember member, int value)
    {
        if (!CanChange(member))
        {

            return false;
        }

        member.SetCurrentMana(value);
        Changed?.Invoke();

        return true;
    }

    public bool SetExperience(PartyMember member, int value)
    {
        if (!CanChange(member))
        {

            return false;
        }

        member.SetExperience(value);
        Changed?.Invoke();

        return true;
    }

    public bool SetPortrait(PartyMember member, Sprite portrait)
    {
        if (!CanChange(member))
        {

            return false;
        }

        member.SetPortrait(portrait);
        Changed?.Invoke();

        return true;
    }

    private bool CanChange(PartyMember member)
    {

        return isActiveAndEnabled && Instance == this && member != null && members.Contains(member);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            Changed?.Invoke();
            InventoryService.Instance?.NotifyEquipmentChanged();
        }
    }
}
