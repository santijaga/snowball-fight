using UnityEngine;

public enum ItemCategory { Inventory, KeyItem }

[CreateAssetMenu(menuName = "Inventory/Item")]
public sealed class ItemDefinition : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private string nameKey;
    [SerializeField] private ItemCategory category;
    [SerializeField, InspectorName("Description")] private string descriptionKey;

    public string Id => id;
    public string NameKey => nameKey;
    public ItemCategory Category => category;
    public string DescriptionKey => descriptionKey;
}
