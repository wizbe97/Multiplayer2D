using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class CharacterSelectionManagerOnline : NetworkBehaviour
{
    public static CharacterSelectionManagerOnline Instance { get; private set; }

    [Networked] private NetworkDictionary<int, int> playerCharacterMap => default;

    [Header("Debug - Player Character Selections")]
    [SerializeField] private Dictionary<int, int> debugCharacterMap = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SubmitCharacterSelection(int characterId)
    {
        int playerId = Object.InputAuthority.RawEncoded;

        if (playerCharacterMap.ContainsKey(playerId))
        {
            playerCharacterMap.Set(playerId, characterId);
        }
        else
        {
            playerCharacterMap.Add(playerId, characterId);
        }

        Debug.Log($"[CharacterSelectionManagerOnline] Player {playerId} selected character {characterId}");
    }

    public bool TryGetCharacter(PlayerRef player, out int characterId)
    {
        return playerCharacterMap.TryGet(player.RawEncoded, out characterId);
    }
    public void SubmitSelectionFromServer(PlayerRef player, int characterId)
    {
        int id = player.RawEncoded;

        if (playerCharacterMap.ContainsKey(id))
        {
            playerCharacterMap.Set(id, characterId);
        }
        else
        {
            playerCharacterMap.Add(id, characterId);
        }

        debugCharacterMap[id] = characterId; // Mirror to serialized field

        Debug.Log($"[CharacterSelectionManagerOnline] Server recorded selection: Player {id} → Character {characterId}");
    }

}
