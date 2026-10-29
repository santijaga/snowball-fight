using UnityEngine;

public enum ItemCategory { Inventory, KeyItem }
public enum ItemType { Consumable, Equipment }
public enum ItemEquipmentSlot { Weapon, Artifact, Amulet, Ring, Trinket }
public enum EquipmentSlot { Weapon, Artifact, Amulet, Ring1, Ring2, Trinket }
public enum WeaponHandedness { OneHanded, TwoHanded }

[CreateAssetMenu(menuName = "Inventory/Item")]
public sealed class ItemDefinition : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private string nameKey;
    [SerializeField] private ItemCategory category;
    [SerializeField, InspectorName("Description")] private string descriptionKey;

    [SerializeField] private ItemType itemType;
    [SerializeField] private ItemEquipmentSlot slot;
    [SerializeField] private WeaponHandedness handedness;
    [SerializeField, Min(0)] private int attack;

    public ItemType Type => itemType;
    public ItemEquipmentSlot Slot => slot;
    public WeaponHandedness Handedness => handedness;
    public int Attack => Mathf.Max(0, attack);
    public bool IsTwoHanded => Type == ItemType.Equipment && Slot == ItemEquipmentSlot.Weapon
        && handedness == WeaponHandedness.TwoHanded;

    public bool Fits(EquipmentSlot target)
    {

        return Type == ItemType.Equipment && Category == ItemCategory.Inventory
            && (target switch
            {
                EquipmentSlot.Weapon => slot == ItemEquipmentSlot.Weapon,
                EquipmentSlot.Artifact => slot == ItemEquipmentSlot.Artifact,
                EquipmentSlot.Amulet => slot == ItemEquipmentSlot.Amulet,
                EquipmentSlot.Ring1 or EquipmentSlot.Ring2 => slot == ItemEquipmentSlot.Ring,
                EquipmentSlot.Trinket => slot == ItemEquipmentSlot.Trinket,
                _ => false
            });
    }

    public string GetDescription()
    {
        string text = GameLocalization.Text(descriptionKey);

        if (Type == ItemType.Equipment)
        {
            string slotKey = Slot switch
            {
                ItemEquipmentSlot.Weapon => "weapon",
                ItemEquipmentSlot.Artifact => "artifact",
                ItemEquipmentSlot.Amulet => "amulet",
                ItemEquipmentSlot.Ring => "ring",
                _ => "trinket"
            };
            text += "\n" + GameLocalization.Text("equipment.item_slot",
                GameLocalization.Text("equipment.slot." + slotKey));

            if (Slot == ItemEquipmentSlot.Weapon)
            {
                text += "\n" + GameLocalization.Text(IsTwoHanded ? "equipment.two_handed" : "equipment.one_handed")
                    + ", " + GameLocalization.Text("equipment.attack", Attack);
            }
        }

        return text;
    }

    public string Id => id;
    public string NameKey => nameKey;
    public ItemCategory Category => category;
    public string DescriptionKey => descriptionKey;
}
