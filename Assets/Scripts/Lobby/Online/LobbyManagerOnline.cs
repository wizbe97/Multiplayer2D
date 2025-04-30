using UnityEngine;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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
        var runner = NetworkManagerOnline.Instance.Runner;

        if (!runner.IsSceneAuthority)
        {
            Debug.LogWarning("Not the scene authority, can't load scene.");
            return;
        }

        foreach (var lobbyPlayer in FindObjectsOfType<LobbyPlayerControllerOnline>())
        {
            Debug.Log($"[LobbyManagerOnline] Despawning lobby player {lobbyPlayer.PlayerName}");
            runner.Despawn(lobbyPlayer.GetComponent<NetworkObject>());
        }

        // Delay longer to give time for despawn sync
        StartCoroutine(LoadSceneAfterDelay(runner, 0.5f));
    }

    private IEnumerator LoadSceneAfterDelay(NetworkRunner runner, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds); // Allow time for despawns to propagate

        var sceneIndex = SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/Gameplay.unity");
        runner.LoadScene(SceneRef.FromIndex(sceneIndex), LoadSceneMode.Single);
    }


}
