using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialog/Sequence")]
public sealed class DialogSequence : ScriptableObject
{
    public enum ItemCondition { Always, HasItem, MissingItem }
    public enum StartMode { PlayerInteraction, NpcApproach }
    public enum StartCondition { Always, PlayerInteraction, NpcApproach }

    [Serializable]
    public sealed class Line
    {
        [SerializeField] private string speakerKey;
        [SerializeField] private string textKey;
        [SerializeField] private ItemCondition condition;
        [SerializeField] private ItemDefinition item;
        [SerializeField] private StartCondition startCondition;
        public string SpeakerKey => speakerKey;
        public string TextKey => textKey;
        public bool IsValid => !string.IsNullOrWhiteSpace(speakerKey)
            && !string.IsNullOrWhiteSpace(textKey)
            && (condition == ItemCondition.Always || item != null);

        public bool CanPlay(StartMode startMode)
        {
            if ((startCondition == StartCondition.PlayerInteraction && startMode != StartMode.PlayerInteraction)
                || (startCondition == StartCondition.NpcApproach && startMode != StartMode.NpcApproach))
            {

                return false;
            }

            if (condition == ItemCondition.Always)
            {

                return true;
            }

            InventoryService inventory = InventoryService.Instance;

            if (item == null || inventory == null || !inventory.isActiveAndEnabled)
            {

                return false;
            }

            bool hasItem = inventory.GetQuantity(item.Id) > 0;

            return condition == ItemCondition.HasItem ? hasItem : !hasItem;
        }
    }

    [SerializeField] private Line[] lines = Array.Empty<Line>();
    public int Count => lines == null ? 0 : lines.Length;
    public Line GetLine(int index) => lines[index];

    public bool IsValid
    {
        get
        {
            if (Count == 0)
            {

                return false;
            }

            foreach (Line line in lines)
            {
                if (line == null || !line.IsValid)
                {

                    return false;
                }
            }

            return true;
        }
    }
}
