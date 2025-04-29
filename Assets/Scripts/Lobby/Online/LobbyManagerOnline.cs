using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LobbyManagerOnline : NetworkBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private LobbyPlayerControllerOnline playerPrefab;

    public Transform leftCharacterPanel;
    public Transform rightCharacterPanel;
    public GameObject startGameButton;

    private Dictionary<int, LobbyPlayerControllerOnline> characterLocks = new Dictionary<int, LobbyPlayerControllerOnline>();


    private NetworkRunner runner;

    private async void Start()
    {
        runner = new GameObject("NetworkRunner (Lobby)").AddComponent<NetworkRunner>();
        runner.ProvideInput = true;
        runner.AddCallbacks(this);

        DontDestroyOnLoad(runner.gameObject); // <<< ADD THIS LINE!

        runner.transform.SetParent(transform); // Keep hierarchy clean


        var result = await runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = "LobbySession",
            SceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        if (!result.Ok)
        {
            Debug.LogError($"[LobbyManagerOnline] Failed to start: {result.ShutdownReason}");
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[LobbyManagerOnline] OnPlayerJoined triggered for {player}");

        // Only spawn for *local* player
        if (player == runner.LocalPlayer)
        {
            Debug.Log($"[LobbyManagerOnline] Spawning LobbyPlayer for local player {player}");
            runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, player);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[LobbyManagerOnline] Player {player} left.");
    }


    public void RegisterReady(LobbyPlayerControllerOnline player, int selectedCharacter)
    {
        if (!characterLocks.ContainsKey(selectedCharacter))
        {
            characterLocks[selectedCharacter] = player;
        }

        CheckIfAllReady();
    }

    public void UnregisterReady(LobbyPlayerControllerOnline player, int selectedCharacter)
    {
        if (characterLocks.ContainsKey(selectedCharacter) && characterLocks[selectedCharacter] == player)
        {
            characterLocks.Remove(selectedCharacter);
        }

        CheckIfAllReady();
    }
    private void CheckIfAllReady()
    {
        if (characterLocks.Count == 2) // Exactly 2 players
        {
            if (runner.IsSharedModeMasterClient)
            {
                startGameButton.SetActive(true);
            }
            Debug.Log("[LobbyManagerOnline] Both players ready! Start button enabled.");
        }
        else
        {
            startGameButton.SetActive(false);
            Debug.Log("[LobbyManagerOnline] Not all players ready. Start button hidden.");
        }
    }

    public void StartGame()
    {
        if (!runner.IsSharedModeMasterClient)
            return; // Only Host can start the game

        Debug.Log("[LobbyManagerOnline] Starting online game!");

        LobbyData.players.Clear();
        LobbyData.isOnlineGame = true; // <<< Online now!

        foreach (var entry in characterLocks)
        {
            PlayerSelection p = new PlayerSelection
            {
                selectedCharacter = entry.Key,
                playerName = entry.Value.PlayerName
            };
            LobbyData.players.Add(p);
        }

        DontDestroyOnLoad(runner.gameObject); // Keep the runner alive across scenes
        SceneManager.LoadScene("Gameplay"); // <<< This instead
    }


    // Empty callbacks
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
}
