using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;

public class PhotonLobby : MonoBehaviourPunCallbacks
{
    public GameObject menuPanel;
    public GameObject waitingPanel;
    public TMP_InputField roomInputField;
    public TextMeshProUGUI roomCodeDisplay;
    public Button createRoomButton;
    public Button joinRoomButton;

    void Awake()
    {
        createRoomButton.interactable = false;
        joinRoomButton.interactable = false;
    }

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings(); // Connect to Photon
    }

    public void CreateRoom()
    {
        string roomCode = GenerateRoomCode();
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 2;

        PhotonNetwork.CreateRoom(roomCode, options);
    }

    public void JoinRoom()
    {
        string roomCode = roomInputField.text;
        PhotonNetwork.JoinRoom(roomCode);
    }

    public override void OnJoinedRoom()
    {
        menuPanel.SetActive(false);
        waitingPanel.SetActive(true);

        if (PhotonNetwork.IsMasterClient)
        {
            roomCodeDisplay.text = "Room Code: " + PhotonNetwork.CurrentRoom.Name;
        }

        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            // Load the game scene for both players
            PhotonNetwork.LoadLevel("TicTacToe");
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            PhotonNetwork.LoadLevel("TicTacToe");
        }
    }

    string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string code = "";
        for (int i = 0; i < 5; i++)
        {
            code += chars[Random.Range(0, chars.Length)];
        }
        return code;
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Join failed: " + message);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Create failed: " + message);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server");

        PhotonNetwork.JoinLobby(); // Optional but useful
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");

        createRoomButton.interactable = true;
        joinRoomButton.interactable = true;
    }
}