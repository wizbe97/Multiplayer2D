using UnityEngine;
using System.Threading.Tasks;

using Fusion;

public class GameManager : MonoBehaviour
{
    public PlayerSelectionCharacter selectionData;

    public GameObject characterAOnlinePrefab;
    public GameObject characterBOnlinePrefab;

    public Transform spawnPoint;

    private async void Start()
    {
        await Task.Delay(100); // wait for a short time after scene load

        var runner = FindObjectOfType<NetworkRunner>();
        int charId = selectionData.selectedCharacter;

        if (charId == -1)
        {
            Debug.LogError("[GameManager] No character selected!");
            return;
        }

        var prefab = (charId == 1) ? characterAOnlinePrefab : characterBOnlinePrefab;
        runner.Spawn(prefab, spawnPoint.position, Quaternion.identity, runner.LocalPlayer);

        Debug.Log($"[GameManager] Spawned local character {charId} for player {runner.LocalPlayer.PlayerId}");
    }

}
