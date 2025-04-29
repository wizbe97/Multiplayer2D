using UnityEngine;
using TMPro;
using Fusion;

public class GameManager : MonoBehaviour
{
    [Header("Offline Prefabs")]
    public GameObject characterAOfflinePrefab;
    public GameObject characterBOfflinePrefab;

    [Header("Online Prefabs")]
    public GameObject characterAOnlinePrefab;
    public GameObject characterBOnlinePrefab;

    public Transform spawnPointA;
    public Transform spawnPointB;

    private bool isOnlineGame;
    private NetworkRunner runner;

    private GameObject prefabA;
    private GameObject prefabB;

    private bool hasSpawned = false; // Add a flag to prevent double spawning

    private void Start()
    {
        runner = FindObjectOfType<NetworkRunner>();
        isOnlineGame = LobbyData.isOnlineGame;

        Debug.Log($"[GameManager] Starting Game. Online: {isOnlineGame}");

        SetupPrefabs();
    }

    private void Update()
    {
        if (isOnlineGame && !hasSpawned)
        {
            if (runner != null && runner.IsRunning && LobbyData.players.Count > 0)
            {
                Debug.Log("[GameManager] Runner ready and LobbyData has players. Spawning...");
                SpawnPlayers();
                hasSpawned = true;
            }
        }
        else if (!isOnlineGame && !hasSpawned)
        {
            SpawnPlayers();
            hasSpawned = true;
        }
    }

    private void SetupPrefabs()
    {
        if (isOnlineGame)
        {
            prefabA = characterAOnlinePrefab;
            prefabB = characterBOnlinePrefab;
        }
        else
        {
            prefabA = characterAOfflinePrefab;
            prefabB = characterBOfflinePrefab;
        }
    }

    private void SpawnPlayers()
    {
        foreach (var player in LobbyData.players)
        {
            GameObject prefabToSpawn = (player.selectedCharacter == 1) ? prefabA : prefabB;
            Transform spawnPoint = (player.selectedCharacter == 1) ? spawnPointA : spawnPointB;

            if (prefabToSpawn != null && spawnPoint != null)
            {
                if (isOnlineGame)
                {
                    if (runner.IsServer)
                    {
                        var networkObj = runner.Spawn(prefabToSpawn, spawnPoint.position, Quaternion.identity);
                        AttachName(networkObj.gameObject, player.playerName.ToString());
                    }
                }
                else
                {
                    var character = Instantiate(prefabToSpawn, spawnPoint.position, Quaternion.identity);
                    AttachName(character, player.playerName.ToString());
                }
            }
        }
    }

    private void AttachName(GameObject character, string playerName)
    {
        GameObject nameTextObj = new GameObject("PlayerName");
        nameTextObj.transform.SetParent(character.transform);
        nameTextObj.transform.localPosition = new Vector3(0, 1f, 0);

        TextMeshPro text = nameTextObj.AddComponent<TextMeshPro>();
        text.text = playerName;
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 5;
        text.color = Color.white;
    }
}
