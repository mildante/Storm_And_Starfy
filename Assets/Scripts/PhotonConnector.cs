using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonConnector : MonoBehaviour
{
    void Start()
    {
        // Подключаемся к серверам Photon
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Успешное подключение к серверу!");
        // Входим в лобби
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Мы в лобби! Можно искать комнату или создавать свою.");
        // Присоединяемся к случайной комнате или создаем новую
        PhotonNetwork.JoinRandomOrCreateRoom();
    }
}
