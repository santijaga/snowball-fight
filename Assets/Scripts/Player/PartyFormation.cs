using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Owns the scene's character instances and their order along the travelled route.
[DefaultExecutionOrder(-100)]
[DisallowMultipleComponent]
public sealed class PartyFormation : MonoBehaviour
{
    private readonly List<Character> characters = new();
    private readonly List<Character> formation = new();
    private readonly List<Vector3> stepOrigins = new();
    private PartyService party;
    private GameObject characterRoot;
    private bool refreshPending;

    public Character ActiveCharacter => formation.Count > 0 ? formation[0] : null;
    public event Action<Vector2> StepComplete;
    public event Action ActiveCharacterChanged;

    public bool IsMoving
    {
        get
        {
            foreach (Character character in characters)
            {
                if (character != null && character.Movement.IsMoving)
                {

                    return true;
                }
            }

            return false;
        }
    }

    private void OnEnable()
    {
        BindParty();

        if (characterRoot != null)
        {
            characterRoot.SetActive(true);
        }
    }

    public void EnsureInitialized()
    {
        if (!isActiveAndEnabled)
        {

            return;
        }

        if (party != PartyService.Instance)
        {
            BindParty();
        }

        if (refreshPending && !IsMoving)
        {
            RefreshCharacters();
        }
    }

    private void BindParty()
    {
        if (party != null)
        {
            party.Changed -= OnPartyChanged;
        }

        party = PartyService.Instance;

        if (party != null)
        {
            party.Changed += OnPartyChanged;
        }

        refreshPending = true;
    }

    private void OnPartyChanged()
    {
        refreshPending = true;
    }

    private void Update()
    {
        EnsureInitialized();
    }

    private void LateUpdate()
    {
        // Keep the existing scene camera target stable when the leader changes.
        if (ActiveCharacter != null)
        {
            transform.position = ActiveCharacter.transform.position;
        }
    }

    private void RefreshCharacters()
    {
        refreshPending = false;

        if (party == null)
        {
            ClearCharacters();

            return;
        }

        Vector3 position = ActiveCharacter != null ? ActiveCharacter.transform.position : transform.position;
        Vector2 facing = ActiveCharacter != null ? ActiveCharacter.FacingDirection : Vector2.down;
        Character previousLeader = ActiveCharacter;
        List<Vector3> slotPositions = new();
        List<Vector2> slotFacings = new();
        bool compositionChanged = false;

        foreach (Character character in formation)
        {
            slotPositions.Add(character != null ? character.transform.position : position);
            slotFacings.Add(character != null ? character.FacingDirection : facing);
        }

        for (int index = characters.Count - 1; index >= 0; index--)
        {
            Character character = characters[index];

            if (character == null || !ContainsMember(character.Member))
            {
                compositionChanged = true;
                formation.Remove(character);
                characters.RemoveAt(index);

                if (character != null)
                {
                    character.Movement.StepComplete -= OnCharacterStepComplete;
                    character.gameObject.SetActive(false);
                    Destroy(character.gameObject);
                }
            }
        }

        if (characterRoot == null)
        {
            characterRoot = new GameObject("Party Characters");
            SceneManager.MoveGameObjectToScene(characterRoot, gameObject.scene);
        }

        foreach (PartyMember member in party.Members)
        {
            if (FindCharacter(member) != null)
            {
                continue;
            }

            if (member.CharacterPrefab == null)
            {
                Debug.LogError($"Party member '{member.Name}' requires a Character prefab to appear in the world.", this);
                continue;
            }

            Character character = Instantiate(member.CharacterPrefab, position, Quaternion.identity, characterRoot.transform);
            character.Bind(member);
            character.TeleportTo(position, facing);
            compositionChanged = true;
            characters.Add(character);
            formation.Add(character);
        }

        // Save control selection separately; never reorder the party data.
        PartyMember savedMember = SessionStateService.Instance?.ControlledMember;
        Character savedCharacter = FindCharacter(savedMember);

        if (savedCharacter != null && previousLeader == null)
        {
            int savedSlot = formation.IndexOf(savedCharacter);
            formation[savedSlot] = formation[0];
            formation[0] = savedCharacter;
        }

        // Fill the existing route slots after membership changes, without cutting corners.
        if (compositionChanged && slotPositions.Count > 0)
        {
            for (int index = 0; index < formation.Count; index++)
            {
                int slot = Mathf.Min(index, slotPositions.Count - 1);
                formation[index].TeleportTo(slotPositions[slot], slotFacings[slot]);
            }
        }

        UpdateRoles();

        if (previousLeader != ActiveCharacter)
        {
            SaveSelection();
            ActiveCharacterChanged?.Invoke();
        }
    }

    private bool ContainsMember(PartyMember member)
    {
        foreach (PartyMember candidate in party.Members)
        {
            if (ReferenceEquals(candidate, member))
            {

                return true;
            }
        }

        return false;
    }

    private Character FindCharacter(PartyMember member)
    {
        foreach (Character character in characters)
        {
            if (character != null && ReferenceEquals(character.Member, member))
            {

                return character;
            }
        }

        return null;
    }

    private void UpdateRoles()
    {
        for (int index = 0; index < formation.Count; index++)
        {
            formation[index].Movement.StepComplete -= OnCharacterStepComplete;
            formation[index].SetControlled(index == 0);

            if (index == 0)
            {
                formation[index].Movement.StepComplete += OnCharacterStepComplete;
            }
        }
    }

    private void SaveSelection()
    {
        if (SessionStateService.Instance != null)
        {
            SessionStateService.Instance.SetControlledMember(ActiveCharacter?.Member);
        }
    }

    public bool TrySwitch(int offset)
    {
        EnsureInitialized();

        if (!isActiveAndEnabled || IsMoving || ActiveCharacter == null || party == null || offset == 0)
        {

            return false;
        }

        int currentIndex = -1;

        for (int index = 0; index < party.Members.Count; index++)
        {
            if (ReferenceEquals(party.Members[index], ActiveCharacter.Member))
            {
                currentIndex = index;
                break;
            }
        }

        if (currentIndex < 0)
        {

            return false;
        }

        int nextIndex = (int)(((long)currentIndex + offset) % party.Members.Count);

        if (nextIndex < 0)
        {
            nextIndex += party.Members.Count;
        }

        Character next = FindCharacter(party.Members[nextIndex]);

        if (next == null || !next.isActiveAndEnabled)
        {

            return false;
        }

        if (next == ActiveCharacter)
        {

            return true;
        }

        Character previous = ActiveCharacter;
        int slot = formation.IndexOf(next);
        Vector3 previousPosition = previous.transform.position;
        Vector2 previousFacing = previous.FacingDirection;
        previous.ClearFocus();
        next.ClearFocus();
        previous.TeleportTo(next.transform.position, next.FacingDirection);
        next.TeleportTo(previousPosition, previousFacing);
        formation[0] = next;
        formation[slot] = previous;
        UpdateRoles();
        SaveSelection();
        ActiveCharacterChanged?.Invoke();

        return true;
    }

    public bool TryMove(Vector2 direction)
    {
        EnsureInitialized();

        if (!isActiveAndEnabled || IsMoving || ActiveCharacter == null)
        {

            return false;
        }

        stepOrigins.Clear();

        foreach (Character character in formation)
        {
            if (!character.isActiveAndEnabled || !character.Movement.isActiveAndEnabled)
            {

                return false;
            }

            stepOrigins.Add(character.transform.position);
        }

        ActiveCharacter.SetFacingDirection(direction);

        if (!ActiveCharacter.Movement.TryMove(direction))
        {

            return false;
        }

        for (int index = 1; index < formation.Count; index++)
        {
            Character follower = formation[index];
            follower.Movement.Speed = ActiveCharacter.Movement.Speed;
            Vector2 displacement = stepOrigins[index - 1] - follower.transform.position;

            if (displacement != Vector2.zero)
            {
                follower.SetFacingDirection(displacement.normalized);
                follower.Movement.FollowTo(stepOrigins[index - 1]);
            }
        }

        return true;
    }

    private void OnCharacterStepComplete(Vector2 position)
    {
        // Followers never publish world events, even when they cross a transition.
        if (isActiveAndEnabled && ActiveCharacter != null && ActiveCharacter.isActiveAndEnabled
            && !ActiveCharacter.Movement.IsMoving && ActiveCharacter.Movement.Position == position)
        {
            StepComplete?.Invoke(position);
        }
    }

    public void TeleportTo(Vector3 position, Vector2 facing)
    {
        EnsureInitialized();
        transform.position = position;

        foreach (Character character in formation)
        {
            character.TeleportTo(position, facing);
        }
    }

    private void OnDisable()
    {
        // Restore route slots before cancelling a partially travelled formation step.
        if (IsMoving && stepOrigins.Count == formation.Count)
        {
            for (int index = 0; index < formation.Count; index++)
            {
                Character character = formation[index];

                if (character != null)
                {
                    character.TeleportTo(stepOrigins[index], character.FacingDirection);
                }
            }
        }

        if (party != null)
        {
            party.Changed -= OnPartyChanged;
        }

        party = null;

        if (characterRoot != null)
        {
            characterRoot.SetActive(false);
        }
    }

    private void ClearCharacters()
    {
        foreach (Character character in characters)
        {
            if (character != null)
            {
                character.Movement.StepComplete -= OnCharacterStepComplete;
            }
        }

        characters.Clear();
        formation.Clear();

        if (characterRoot != null)
        {
            characterRoot.SetActive(false);
            Destroy(characterRoot);
            characterRoot = null;
        }
    }

    private void OnDestroy()
    {
        ClearCharacters();
    }
}
