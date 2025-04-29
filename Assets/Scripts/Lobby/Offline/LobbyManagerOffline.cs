using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class LobbyManagerOffline : MonoBehaviour
{
    public Transform leftCharacterPanel;
    public Transform rightCharacterPanel;
    public GameObject lobbyPlayerPrefab;
    public GameObject startGameButton;

    private HashSet<InputDevice> joinedDevices = new HashSet<InputDevice>();

    // Track ready players
    private Dictionary<int, LobbyPlayerControllerOffline> characterLocks = new Dictionary<int, LobbyPlayerControllerOffline>();

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (!joinedDevices.Contains(Keyboard.current))
            {
                SpawnLocalPlayer(Keyboard.current);
            }
        }

        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            if (!joinedDevices.Contains(Gamepad.current))
            {
                SpawnLocalPlayer(Gamepad.current);
            }
        }
    }

    private void SpawnLocalPlayer(InputDevice device)
    {
        Debug.Log($"[LobbyOfflineManager] Spawning a new local player for device {device.displayName}");

        GameObject newPlayer = Instantiate(lobbyPlayerPrefab, leftCharacterPanel);

        var lobbyPlayer = newPlayer.GetComponent<LobbyPlayerControllerOffline>();
        if (lobbyPlayer != null)
        {
            lobbyPlayer.leftCharacterPanel = leftCharacterPanel;
            lobbyPlayer.rightCharacterPanel = rightCharacterPanel;
            lobbyPlayer.assignedDevice = device;
            lobbyPlayer.lobbyManager = this; // inject reference
            lobbyPlayer.Initialize(joinedDevices.Count);
        }
        else
        {
            Debug.LogError("[LobbyOfflineManager] Spawned player missing LobbyPlayerControllerOffline!");
        }

        joinedDevices.Add(device);
    }

    // === READY SYSTEM ===

    public bool CanReadyUp(LobbyPlayerControllerOffline player, int selectedCharacter)
    {
        if (characterLocks.ContainsKey(selectedCharacter))
        {
            return false;
        }
        return true;
    }

    public void RegisterReady(LobbyPlayerControllerOffline player, int selectedCharacter)
    {
        if (!characterLocks.ContainsKey(selectedCharacter))
        {
            characterLocks[selectedCharacter] = player;
        }

        CheckIfAllReady();
    }

    public void UnregisterReady(LobbyPlayerControllerOffline player, int selectedCharacter)
    {
        if (characterLocks.ContainsKey(selectedCharacter) && characterLocks[selectedCharacter] == player)
        {
            characterLocks.Remove(selectedCharacter);
        }

        CheckIfAllReady();
    }

    private void CheckIfAllReady()
    {
        if (characterLocks.Count == 2) // Exactly 2 players ready
        {
            startGameButton.SetActive(true);
            Debug.Log("[LobbyOfflineManager] Both players ready! Start button enabled.");
        }
        else
        {
            startGameButton.SetActive(false);
            Debug.Log("[LobbyOfflineManager] Not all players ready. Start button hidden.");
        }
    }

    public void StartGame()
    {
        Debug.Log("[LobbyManagerOffline] Starting game!");

        LobbyData.players.Clear();
        LobbyData.isOnlineGame = false; // <<< Offline!

        foreach (var entry in characterLocks)
        {
            PlayerSelection p = new PlayerSelection
            {
                selectedCharacter = entry.Key,
                playerName = entry.Value.playerNameText.text
            };
            LobbyData.players.Add(p);
        }

        SceneManager.LoadScene("Gameplay"); 
    }

}
