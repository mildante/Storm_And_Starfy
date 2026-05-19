using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ApiManager : MonoBehaviour
{
    public static ApiManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public IEnumerator GetRequest(string url, Action<string> onSuccess, Action<string> onError, bool useAuth = false)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.downloadHandler = new DownloadHandlerBuffer();

        if (useAuth)
        {
            request.SetRequestHeader("Authorization", "Bearer " + PlayerSession.Token);
        }

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            onSuccess?.Invoke(request.downloadHandler.text);
        }
        else
        {
            Debug.LogError(request.error);
            onError?.Invoke(request.downloadHandler.text);
        }
    }

    public IEnumerator PostRequest(string url, string jsonBody, Action<string> onSuccess, Action<string> onError, bool useAuth = false)
    {
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        if (useAuth)
        {
            request.SetRequestHeader("Authorization", "Bearer " + PlayerSession.Token);
        }

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            onSuccess?.Invoke(request.downloadHandler.text);
        }
        else
        {
            Debug.LogError(request.error);
            onError?.Invoke(request.downloadHandler.text);
        }
    }
}