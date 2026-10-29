using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Database")]
public sealed class ItemDatabase : ScriptableObject
{
    [SerializeField] private ItemDefinition[] items = Array.Empty<ItemDefinition>();

    public IReadOnlyList<ItemDefinition> Items => Array.AsReadOnly(items);

    public bool TryGet(string id, out ItemDefinition item)
    {
        foreach (ItemDefinition candidate in items)
        {
            if (candidate != null && candidate.Id == id)
            {
                item = candidate;

                return true;
            }
        }

        item = null;

        return false;
    }

    public bool IsValid()
    {
        HashSet<string> ids = new(StringComparer.Ordinal);

        foreach (ItemDefinition item in items)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.Id) || string.IsNullOrWhiteSpace(item.NameKey)
                || string.IsNullOrWhiteSpace(item.DescriptionKey) || !ids.Add(item.Id)
                || !Enum.IsDefined(typeof(ItemCategory), item.Category)
                || !Enum.IsDefined(typeof(ItemType), item.Type)
                || (item.Type == ItemType.Equipment && (item.Category != ItemCategory.Inventory
                    || !Enum.IsDefined(typeof(ItemEquipmentSlot), item.Slot)
                    || !Enum.IsDefined(typeof(WeaponHandedness), item.Handedness))))
            {

                return false;
            }
        }

        return true;
    }
}
