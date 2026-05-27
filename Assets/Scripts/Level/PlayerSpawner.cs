using Photon.Pun;
using System.Collections;
using UnityEngine;

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
            Debug.LogError("Selected character is missing.");
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
        else if (characterId == 2)
        {
            prefabName = "Starfy";
            spawnPosition = starfySpawn.position;
        }
        else
        {
            Debug.LogError("Selected character is invalid.");
            return;
        }

        GameObject player = PhotonNetwork.Instantiate(
            prefabName,
            spawnPosition,
            Quaternion.identity);

        StartCoroutine(AttachLocalPlayer(player.transform));
    }

    private IEnumerator AttachLocalPlayer(Transform playerTransform)
    {
        const int maxAttempts = 30;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.SetPlayer(playerTransform);

                if (LevelManager.Instance.cameraMove != null)
                {
                    LevelManager.Instance.cameraMove.SetTarget(playerTransform, true);
                    yield break;
                }
            }

            if (Camera.main != null &&
                Camera.main.TryGetComponent(out CameraMove cameraMove))
            {
                cameraMove.SetTarget(playerTransform, true);
                yield break;
            }

            yield return null;
        }

        Debug.LogWarning("CameraMove was not found for local player.");
    }
}
