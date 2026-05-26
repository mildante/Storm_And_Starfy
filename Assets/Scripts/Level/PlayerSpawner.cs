using Photon.Pun;
using UnityEngine;
using ExitGames.Client.Photon;

public class NetworkPlayerSpawner : MonoBehaviour
{
    public Transform stormSpawn;
    public Transform starfySpawn;

    private const string CHAR_KEY = "SelectedChar";

    private void Start()
    {
        SpawnSelectedCharacter();
    }

    private void SpawnSelectedCharacter()
    {
        if (!PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(
            CHAR_KEY,
            out object value))
        {
            Debug.LogError("Персонаж не выбран!");
            return;
        }

        int characterId = (int)value;

        string prefabName;
        Vector3 spawnPosition;

        if (characterId == 1)
        {
            prefabName = "Storm";
            spawnPosition = stormSpawn.position;
        }
        else
        {
            prefabName = "Starfy";
            spawnPosition = starfySpawn.position;
        }

        GameObject player = PhotonNetwork.Instantiate(
            prefabName,
            spawnPosition,
            Quaternion.identity);

        Camera.main.GetComponent<CameraMove>()
            .SetTarget(player.transform);

        LevelManager.Instance.SetPlayer(player.transform);
    }
}