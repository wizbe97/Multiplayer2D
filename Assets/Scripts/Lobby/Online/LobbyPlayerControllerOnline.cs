using UnityEngine;
using TMPro;
using Fusion;
using UnityEngine.InputSystem;

public class LobbyPlayerControllerOnline : NetworkBehaviour
{
    public TextMeshProUGUI playerNameText;

    [HideInInspector] public LobbyManagerOnline lobbyManager;
    [HideInInspector] public Transform leftCharacterPanel;
    [HideInInspector] public Transform rightCharacterPanel;

    private LobbyInputActions inputActions;

    [Networked] public string PlayerName { get; set; }
    [Networked] public int SelectedCharacter { get; set; } = 1;
    [Networked] public bool IsReady { get; set; }

    private void Awake()
    {
        Debug.Log("[LobbyPlayerController] Awake");
        inputActions = new LobbyInputActions();
    }

    public override void Spawned()
    {
        Debug.Log($"[LobbyPlayerController] Spawned. InputAuthority: {HasInputAuthority}");

        lobbyManager = FindObjectOfType<LobbyManagerOnline>();
        leftCharacterPanel = lobbyManager.leftCharacterPanel;
        rightCharacterPanel = lobbyManager.rightCharacterPanel;

        if (HasInputAuthority)
        {
            inputActions.Enable();
            inputActions.LobbyActions.MoveLeft.performed += ctx => OnMoveLeft();
            inputActions.LobbyActions.MoveRight.performed += ctx => OnMoveRight();
            inputActions.LobbyActions.Ready.performed += ctx => OnReady();

            RPC_SetPlayerName($"Player {Runner.LocalPlayer.PlayerId}");
        }

        UpdateUI();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetPlayerName(string name)
    {
        Debug.Log($"[LobbyPlayerController] RPC_SetPlayerName called with {name}");
        PlayerName = name;
    }

    private void OnMoveLeft()
    {
        if (!IsReady)
        {
            RPC_SetCharacter(1);
        }
    }

    private void OnMoveRight()
    {
        if (!IsReady)
        {
            RPC_SetCharacter(2);
        }
    }

    private void OnReady()
    {
        Debug.Log("[LobbyPlayerController] Ready input detected");

        bool newReadyState = !IsReady;
        RPC_UpdateReadyUI(newReadyState);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_UpdateReadyUI(bool ready)
    {
        Debug.Log($"[LobbyPlayerController] RPC_UpdateReadyUI called with ready={ready}");
        IsReady = ready;
        UpdateUI();

        // Only the host should register ready state
        if (Runner.IsSharedModeMasterClient)
        {
            if (IsReady)
            {
                lobbyManager.RegisterReady(this, SelectedCharacter);
            }
            else
            {
                lobbyManager.UnregisterReady(this, SelectedCharacter);
            }
        }
    }


    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_SetCharacter(int character)
    {
        Debug.Log($"[LobbyPlayerController] RPC_SetCharacter: {character}");
        SelectedCharacter = character;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (playerNameText != null)
        {
            playerNameText.text = PlayerName;

            // Color based on PlayerId
            if (Object.InputAuthority.PlayerId == 1)
                playerNameText.color = Color.red;
            else if (Object.InputAuthority.PlayerId == 2)
                playerNameText.color = Color.green;

            // Bold/Underline if Ready
            playerNameText.fontStyle = IsReady ? (FontStyles.Bold | FontStyles.Underline) : FontStyles.Normal;
        }

        // Move to correct character panel
        Transform targetPanel = (SelectedCharacter == 1) ? leftCharacterPanel : rightCharacterPanel;
        if (targetPanel != null && transform.parent != targetPanel)
        {
            transform.SetParent(targetPanel, false);
        }

        // Set proper anchored position for Player 2
        RectTransform rect = GetComponent<RectTransform>();
        if (rect != null)
        {
            if (Object.InputAuthority.PlayerId == 2)
            {
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, -200f); // Fixed Y offset
            }
            else
            {
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, 0f); // Player 1 stays at default
            }
        }
    }
}