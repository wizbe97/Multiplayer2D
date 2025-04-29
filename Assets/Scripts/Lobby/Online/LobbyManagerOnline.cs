using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LobbyManagerOnline : MonoBehaviour
{
    public static LobbyManagerOnline Instance { get; private set; }

    [SerializeField] private LobbyPlayerControllerOnline playerPrefab;

    public Transform leftCharacterPanel;
    public Transform rightCharacterPanel;
    public GameObject startGameButton;

    private Dictionary<int, LobbyPlayerControllerOnline> characterLocks = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void HandlePlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[LobbyManagerOnline] Handling player joined: {player}");

        if (player == runner.LocalPlayer)
        {
            Debug.Log($"[LobbyManagerOnline] Spawning lobby player prefab for local player {player}");
            runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, player);
        }
    }

    public void RegisterReady(LobbyPlayerControllerOnline player, int selectedCharacter)
    {
        if (!characterLocks.ContainsKey(selectedCharacter))
        {
            characterLocks[selectedCharacter] = player;
            Debug.Log($"[LobbyManagerOnline] Player {player.PlayerName} locked character {selectedCharacter}");
        }
        CheckIfAllReady();
    }

    public void UnregisterReady(LobbyPlayerControllerOnline player, int selectedCharacter)
    {
        if (characterLocks.TryGetValue(selectedCharacter, out var existing) && existing == player)
        {
            characterLocks.Remove(selectedCharacter);
            Debug.Log($"[LobbyManagerOnline] Player {player.PlayerName} unlocked character {selectedCharacter}");
            CheckIfAllReady();
        }
    }

    private void CheckIfAllReady()
    {
        bool allReady = characterLocks.Count == 2;
        startGameButton.SetActive(allReady && NetworkManagerOnline.Instance.Runner.IsSharedModeMasterClient);

        Debug.Log(allReady
            ? "[LobbyManagerOnline] Both players ready, start button active"
            : "[LobbyManagerOnline] Not all players ready, start button hidden");
    }

    public void StartGame()
    {
        if (!NetworkManagerOnline.Instance.Runner.IsSharedModeMasterClient)
        {
            Debug.LogWarning("[LobbyManagerOnline] Only the host can start the game.");
            return;
        }

        Debug.Log("[LobbyManagerOnline] Starting the online game!");

        LobbyData.players.Clear();
        LobbyData.isOnlineGame = true;

        foreach (var kvp in characterLocks)
        {
            LobbyData.players.Add(new PlayerSelection
            {
                selectedCharacter = kvp.Key,
                playerName = kvp.Value.PlayerName
            });
        }

        // Destroy this lobby manager when leaving the lobby
        Destroy(gameObject);

        SceneManager.LoadScene("Gameplay", LoadSceneMode.Single);
    }
}
