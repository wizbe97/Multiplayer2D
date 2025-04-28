// using TMPro;
// using UnityEngine;

// public class LobbyUI : MonoBehaviour
// {
//     [SerializeField] private Transform leftPanel;
//     [SerializeField] private Transform rightPanel;
//     [SerializeField] private GameObject playerNamePrefab;

//     public void RefreshUI()
//     {
//         foreach (Transform child in leftPanel) Destroy(child.gameObject);
//         foreach (Transform child in rightPanel) Destroy(child.gameObject);

//         var lobbyManager = FindObjectOfType<LobbyManagerOnline>();

//         for (int i = 0; i < lobbyManager.Players.Count; i++)
//         {
//             var player = lobbyManager.Players[i];

//             Transform parent = player.selectedCharacter == 0 ? leftPanel : rightPanel;

//             var textObj = Instantiate(playerNamePrefab, parent);
//             var textMesh = textObj.GetComponent<TextMeshProUGUI>();
//             textMesh.text = player.playerName.ToString();
//             textMesh.color = Color.white;
//         }
//     }
// }
