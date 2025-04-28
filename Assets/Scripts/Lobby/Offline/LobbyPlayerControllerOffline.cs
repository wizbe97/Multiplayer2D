using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class LobbyPlayerControllerOffline : MonoBehaviour
{
    public TextMeshProUGUI playerNameText;

    [HideInInspector] public Transform leftCharacterPanel;
    [HideInInspector] public Transform rightCharacterPanel;
    [HideInInspector] public InputDevice assignedDevice;
    [HideInInspector] public LobbyManagerOffline lobbyManager; // Add this!

    private LobbyInputActions inputActions;
    private InputUser inputUser;

    private int selectedCharacter = 0; // 1 = Left (Character A), 2 = Right (Character B)
    private bool isReady = false;

    private void Awake()
    {
        inputActions = new LobbyInputActions();
    }

    private void OnEnable()
    {
        inputActions.Enable();

        inputActions.LobbyActions.MoveLeft.performed += ctx => OnMoveLeft();
        inputActions.LobbyActions.MoveRight.performed += ctx => OnMoveRight();
        inputActions.LobbyActions.Ready.performed += ctx => OnReady();
    }

    private void OnDisable()
    {
        inputActions.Disable();

        inputActions.LobbyActions.MoveLeft.performed -= ctx => OnMoveLeft();
        inputActions.LobbyActions.MoveRight.performed -= ctx => OnMoveRight();
        inputActions.LobbyActions.Ready.performed -= ctx => OnReady();
    }

    public void Initialize(int playerIndex)
    {
        playerNameText.text = $"Player {playerIndex + 1}";

        selectedCharacter = 1; // Default to Left (Character A)
        UpdateCharacterSelection();

        // Color setup
        if (playerIndex == 0)
        {
            playerNameText.color = Color.red;
        }
        else if (playerIndex == 1)
        {
            playerNameText.color = Color.green;
        }

        // Lower second player's position slightly
        RectTransform rect = GetComponent<RectTransform>();
        if (playerIndex == 1 && rect != null)
        {
            rect.anchoredPosition -= new Vector2(0, 200f);
        }

        // Critical: Device pairing
        inputUser = InputUser.CreateUserWithoutPairedDevices();
        InputUser.PerformPairingWithDevice(assignedDevice, user: inputUser);
        inputUser.AssociateActionsWithUser(inputActions);
    }

    private void OnMoveLeft()
    {
        if (!isReady)
        {
            selectedCharacter = 1;
            UpdateCharacterSelection();
        }
    }

    private void OnMoveRight()
    {
        if (!isReady)
        {
            selectedCharacter = 2;
            UpdateCharacterSelection();
        }
    }

    private void OnReady()
    {
        if (!isReady)
        {
            // First time trying to ready up
            if (lobbyManager.CanReadyUp(this, selectedCharacter))
            {
                isReady = true;
                ApplyReadyVisual(true);
                lobbyManager.RegisterReady(this, selectedCharacter);
                Debug.Log($"[LobbyOfflinePlayerController] Player Ready with Character {selectedCharacter}");
            }
            else
            {
                Debug.Log($"[LobbyOfflinePlayerController] Cannot Ready: Character {selectedCharacter} already taken!");
            }
        }
        else
        {
            // Unready
            isReady = false;
            ApplyReadyVisual(false);
            lobbyManager.UnregisterReady(this, selectedCharacter);
            Debug.Log($"[LobbyOfflinePlayerController] Player Unready");
        }
    }

    private void ApplyReadyVisual(bool ready)
    {
        playerNameText.fontStyle = ready ? FontStyles.Underline : FontStyles.Normal;
        playerNameText.fontStyle = ready ? FontStyles.Bold : FontStyles.Normal;
    }

    private void UpdateCharacterSelection()
    {
        if (selectedCharacter == 1 && leftCharacterPanel != null)
        {
            transform.SetParent(leftCharacterPanel, false);
        }
        else if (selectedCharacter == 2 && rightCharacterPanel != null)
        {
            transform.SetParent(rightCharacterPanel, false);
        }
    }
}
