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
    [Networked] public int SelectedCharacter { get; set; } = 1; // 1 = left panel default
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
            inputActions.LobbyActions.MoveLeft.performed += ctx => { Debug.Log("[INPUT] MoveLeft pressed"); OnMoveLeft(); };
            inputActions.LobbyActions.MoveRight.performed += ctx => { Debug.Log("[INPUT] MoveRight pressed"); OnMoveRight(); };
            inputActions.LobbyActions.Ready.performed += ctx => { Debug.Log("[INPUT] Ready pressed"); OnReady(); };

            StartCoroutine(WaitAndSetName());
        }

        UpdateUI();
    }

    private System.Collections.IEnumerator WaitAndSetName()
    {
        yield return new WaitUntil(() => Runner != null && Runner.LocalPlayer == Object.InputAuthority);

        Debug.Log($"[LobbyPlayerController] Sending PlayerName for {Runner.LocalPlayer}");
        RPC_SetPlayerName($"Player {Runner.LocalPlayer.PlayerId}");
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetPlayerName(string name)
    {
        Debug.Log($"[LobbyPlayerController] RPC_SetPlayerName called with {name}");
        PlayerName = name;
        UpdateUI();
    }

    private void OnMoveLeft()
    {
        if (!IsReady)
        {
            Debug.Log("[INPUT] Move Left Pressed");
            RPC_SetCharacter(1);
        }
    }

    private void OnMoveRight()
    {
        if (!IsReady)
        {
            Debug.Log("[INPUT] Move Right Pressed");
            RPC_SetCharacter(2);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_SetCharacter(int character)
    {
        Debug.Log($"[RPC] Set Character: {character}");
        SelectedCharacter = character;
        UpdateUI();
    }

    private void OnReady()
    {
        Debug.Log("[LobbyPlayerController] Ready input detected");
        IsReady = !IsReady;
    }

    public void UpdateUI()
    {
        if (playerNameText != null)
        {
            playerNameText.text = PlayerName;
            Debug.Log($"[LobbyPlayerController] UI updated - PlayerName = {PlayerName}");
        }

        if (SelectedCharacter == 1 && leftCharacterPanel != null)
        {
            if (transform.parent != leftCharacterPanel)
            {
                Debug.Log("[LobbyPlayerController] Moving to Left Panel");
                transform.SetParent(leftCharacterPanel, false);
            }
        }
        else if (SelectedCharacter == 2 && rightCharacterPanel != null)
        {
            if (transform.parent != rightCharacterPanel)
            {
                Debug.Log("[LobbyPlayerController] Moving to Right Panel");
                transform.SetParent(rightCharacterPanel, false);
            }
        }
    }
}
