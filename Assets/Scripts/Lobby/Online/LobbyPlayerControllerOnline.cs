using UnityEngine;
using System;
using TMPro;
using Fusion;
using UnityEngine.InputSystem;

public class LobbyPlayerControllerOnline : NetworkBehaviour
{
    public TextMeshProUGUI playerNameText;

    [HideInInspector] public LobbyManagerOnline lobbyManager;
    [HideInInspector] public Transform leftCharacterPanel;
    [HideInInspector] public Transform rightCharacterPanel;

    public PlayerSelectionCharacter selectionData;

    private LobbyInputActions inputActions;

    [Networked] public string PlayerName { get; set; }
    [Networked] public int SelectedCharacter { get; set; } = 1;
    [Networked] public bool IsReady { get; set; }

    private void Awake()
    {
        Debug.Log("[LobbyPlayerController] Awake");
        Debug.Log(Environment.StackTrace);
    }


    public override void Spawned()
    {
        inputActions = new LobbyInputActions();
        Debug.Log($"[LobbyPlayerController] Spawned. InputAuthority: {HasInputAuthority}");

        lobbyManager = FindObjectOfType<LobbyManagerOnline>();
        if (lobbyManager == null) return;

        leftCharacterPanel = lobbyManager.leftCharacterPanel;
        rightCharacterPanel = lobbyManager.rightCharacterPanel;

        if (HasInputAuthority)
        {
            selectionData.selectedCharacter = SelectedCharacter;

            inputActions.Enable();
            inputActions.LobbyActions.MoveLeft.performed += ctx => OnMoveLeft();
            inputActions.LobbyActions.MoveRight.performed += ctx => OnMoveRight();
            inputActions.LobbyActions.Ready.performed += ctx => OnReady();

            RPC_SetPlayerName($"Player {Runner.LocalPlayer.PlayerId}");
        }

        UpdateUI();
    }

    private void OnDisable()
    {
        if (inputActions != null)
            inputActions.Disable();
        inputActions.LobbyActions.MoveLeft.performed -= ctx => OnMoveLeft();
        inputActions.LobbyActions.MoveRight.performed -= ctx => OnMoveRight();
        inputActions.LobbyActions.Ready.performed -= ctx => OnReady();
    }

    private void OnDestroy()
    {
        if (inputActions != null)
            inputActions.Disable();
        inputActions.LobbyActions.MoveLeft.performed -= ctx => OnMoveLeft();
        inputActions.LobbyActions.MoveRight.performed -= ctx => OnMoveRight();
        inputActions.LobbyActions.Ready.performed -= ctx => OnReady();
    }


    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetPlayerName(string name)
    {
        PlayerName = name;
    }

    private void OnMoveLeft()
    {
        if (!IsReady)
            RPC_SetCharacter(1);
    }

    private void OnMoveRight()
    {
        if (!IsReady)
            RPC_SetCharacter(2);
    }

    private void OnReady()
    {
        bool newReadyState = !IsReady;
        RPC_UpdateReadyUI(newReadyState);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_UpdateReadyUI(bool ready)
    {
        IsReady = ready;
        UpdateUI();
        // Store local selection
        if (HasInputAuthority && selectionData != null)
        {
            selectionData.selectedCharacter = SelectedCharacter;
        }

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
        SelectedCharacter = character;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (playerNameText != null)
        {
            playerNameText.text = PlayerName;
            playerNameText.color = (Object.InputAuthority.PlayerId == 1) ? Color.red : Color.green;
            playerNameText.fontStyle = IsReady ? (FontStyles.Bold | FontStyles.Underline) : FontStyles.Normal;
        }

        Transform targetPanel = (SelectedCharacter == 1) ? leftCharacterPanel : rightCharacterPanel;
        if (targetPanel != null && transform.parent != targetPanel)
            transform.SetParent(targetPanel, false);

        RectTransform rect = GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = (Object.InputAuthority.PlayerId == 2)
                ? new Vector2(rect.anchoredPosition.x, -200f)
                : new Vector2(rect.anchoredPosition.x, 0f);
        }
    }
}
