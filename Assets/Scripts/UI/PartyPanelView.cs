using UnityEngine;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
[RequireComponent(typeof(PanelRenderer))]
public sealed class PartyPanelView : MonoBehaviour
{
    private sealed class Card
    {
        public VisualElement Content;
        public Image Portrait;
        public Label Name;
        public Label Class;
        public Label Level;
        public Label Health;
        public Label Mana;
        public Label Experience;

        public void ResizePortrait(GeometryChangedEvent change)
        {
            // Keep the portrait square inside its vertical margins and the card's padding.
            float verticalMargins = Portrait.resolvedStyle.marginTop + Portrait.resolvedStyle.marginBottom;
            float size = Mathf.Max(0f, Content.contentRect.height - verticalMargins);
            Portrait.style.width = size;
            Portrait.style.height = size;
        }
    }

    private PanelRenderer panelRenderer;
    private PartyService partyService;
    private readonly Card[] cards = new Card[PartyRules.MaximumMembers];

    private void OnEnable()
    {
        panelRenderer = GetComponent<PanelRenderer>();
        GameLocalization.Changed += Refresh;
        BindService();
        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    private void Update()
    {
        // Rebind only when the service changes; data updates arrive through Changed.
        if (!ReferenceEquals(partyService, PartyService.Instance))
        {
            BindService();
        }
    }

    private void BindService()
    {
        if (partyService != null)
        {
            partyService.Changed -= Refresh;
        }

        partyService = PartyService.Instance;

        if (partyService != null)
        {
            partyService.Changed += Refresh;
        }

        Refresh();
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement rootElement)
    {
        UnbindCards();
        VisualElement panel = rootElement?.Q<VisualElement>("PartyPanel");

        if (panel == null)
        {
            Debug.LogError("PartyPanelView requires a PartyPanel element in its UI template.", this);

            return;
        }

        for (int index = 0; index < cards.Length; index++)
        {
            VisualElement slot = panel.Q<VisualElement>($"CharacterInfoCard{index + 1}");
            Card card = new()
            {
                Content = slot?.Q<VisualElement>("CharacterContent"),
                Portrait = slot?.Q<Image>("CharacterPortrait"),
                Name = slot?.Q<Label>("CharacterName"),
                Class = slot?.Q<Label>("CharacterClass"),
                Level = slot?.Q<Label>("CharacterLevel"),
                Health = slot?.Q<Label>("CharacterHealth"),
                Mana = slot?.Q<Label>("CharacterMana"),
                Experience = slot?.Q<Label>("CharacterExperience")
            };

            if (card.Content == null || card.Portrait == null || card.Name == null || card.Class == null
                || card.Level == null || card.Health == null || card.Mana == null || card.Experience == null)
            {
                Debug.LogError($"PartyPanelView: character slot {index + 1} is missing required elements.", this);
                continue;
            }

            card.Portrait.scaleMode = ScaleMode.ScaleToFit;
            card.Name.enableRichText = false;
            card.Class.enableRichText = false;
            card.Level.enableRichText = false;
            card.Health.enableRichText = false;
            card.Mana.enableRichText = false;
            card.Experience.enableRichText = false;
            card.Content.RegisterCallback<GeometryChangedEvent>(card.ResizePortrait);
            card.ResizePortrait(null);
            cards[index] = card;
        }

        Refresh();
    }

    private void Refresh()
    {
        if (!isActiveAndEnabled)
        {

            return;
        }

        for (int index = 0; index < cards.Length; index++)
        {
            Card card = cards[index];

            if (card == null)
            {
                continue;
            }

            PartyMember member = partyService != null && partyService.Members != null
                && index < partyService.Members.Count ? partyService.Members[index] : null;
            card.Content.style.display = member == null ? DisplayStyle.None : DisplayStyle.Flex;
            card.Portrait.sprite = member?.Portrait;
            card.Name.text = member?.Name ?? string.Empty;
            card.Class.text = member?.ClassName ?? string.Empty;
            card.Level.text = member == null ? string.Empty : GameLocalization.Text("party.level", member.Level);
            card.Health.text = member == null ? string.Empty : GameLocalization.Text("party.health", member.CurrentHealth, member.MaximumHealth);
            card.Mana.text = member == null ? string.Empty : GameLocalization.Text("party.mana", member.CurrentMana, member.MaximumMana);
            card.Experience.text = member == null ? string.Empty
                : GameLocalization.Text("party.experience", member.Experience, member.ExperienceToNextLevel);
        }
    }

    private void UnbindCards()
    {
        foreach (Card card in cards)
        {
            if (card != null)
            {
                card.Content.UnregisterCallback<GeometryChangedEvent>(card.ResizePortrait);
            }
        }

        System.Array.Clear(cards, 0, cards.Length);
    }

    private void OnDisable()
    {
        GameLocalization.Changed -= Refresh;

        if (partyService != null)
        {
            partyService.Changed -= Refresh;
        }

        if (panelRenderer != null)
        {
            panelRenderer.UnregisterUIReloadCallback(OnUIReload);
        }

        partyService = null;
        UnbindCards();
    }
}
