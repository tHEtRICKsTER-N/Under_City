using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class MainMenu : MonoBehaviourPunCallbacks
{
    #region static var
    public static MainMenu obj;
    private void Awake()
    {
        obj = this;
    }
    #endregion


    #region variables
    public Text makerText;
    public Text _roomNameText;
    public Text _errorText;
    public Text playerNameText;

    public TMP_InputField _createRoomInputField;
    public TMP_InputField nameInputField;

    public GameObject _createRoomPanel;
    public GameObject _roomsPanel;
    public GameObject _errorPanel;
    public GameObject roomBrowserPanel;
    public GameObject nameInputPanel;
    public GameObject startGameButton;
    public GameObject roomTestButton;
    public GameObject loadingPanel;
    
    public Button _menuCreateRoomButton;
    public Button _panelCreateRoomButton;
   
    public RoomButton theRoomButton;

    public string[] allmaps;

    public bool changeMapBetweenMatch = true;
    public static bool hasSetNickName;


    private List<Text> allPlayerNames = new List<Text>();
    private List<RoomButton> allRoomButtons = new List<RoomButton>();
    #endregion

    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        loadingPanel.SetActive(false);
        _createRoomPanel.SetActive(false);
        _roomsPanel.SetActive(false);
        _errorPanel.SetActive(false);
        roomBrowserPanel.SetActive(false);
        nameInputPanel.SetActive(false);
        roomTestButton.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        #if UNITY_EDITOR
        roomTestButton.SetActive(true);
        #endif
    }

    #region  functions

    void ListAllPlayers()
    {
        foreach(Text player in allPlayerNames)
        {
            Destroy(player.gameObject);
        }
        allPlayerNames.Clear();

        Player[] players = PhotonNetwork.PlayerList;
        for(int i = 0; i < players.Length; i++)
        {
            Text newPlayerLabel = Instantiate(playerNameText, playerNameText.transform.parent);
            newPlayerLabel.text = players[i].NickName;
            newPlayerLabel.gameObject.SetActive(true);
            allPlayerNames.Add(newPlayerLabel);
        }
    }

    public void QuickJoin()
    {
        RoomOptions rOptions = new RoomOptions();
        rOptions.MaxPlayers = 10;
        PhotonNetwork.CreateRoom("TestRoom",rOptions);
        CloseEveryPanel();
    }

    public void StartGame()
    {
        //PhotonNetwork.LoadLevel(2);
        PhotonNetwork.LoadLevel(allmaps[Random.Range(0, allmaps.Length)]);
        CloseEveryPanel();
        loadingPanel.SetActive(true);
    }

    public void SetNickName()
    {
        if (!string.IsNullOrEmpty(nameInputField.text))
        {
            PhotonNetwork.NickName = nameInputField.text;

            PlayerPrefs.SetString("playerName", nameInputField.text);   //this will store the name of the player in hard drive so that we dont have to change the name everytime we enter the game


            CloseEveryPanel();

            hasSetNickName = true;
        }
    }

    public void CloseEveryPanel() 
    {
        _createRoomPanel.SetActive(false);
        _roomsPanel.SetActive(false);
        _errorPanel.SetActive(false);
        roomBrowserPanel.SetActive(false);
        nameInputPanel.SetActive(false);
        loadingPanel.SetActive(false);
    }

    public void ActivateCreateRoomPanel()
    {
        _createRoomPanel.SetActive(true);
    }

    public void DeactivateCreateRoomPanel()
    {
        _createRoomPanel.SetActive(false);

    }

    public void CreateRoom()
    {
        if (!string.IsNullOrEmpty(_createRoomInputField.text))
        {
            RoomOptions rOptions = new RoomOptions();
            rOptions.MaxPlayers = 10;
            PhotonNetwork.CreateRoom(_createRoomInputField.text, rOptions);
            Debug.Log("ROOM CREATED !!");
        }
    }

    public void LeaveRoom()         //i have used the error panel's text to dispaly that we are leaving
    {
        PhotonNetwork.LeaveRoom();
        CloseEveryPanel();
        _errorPanel.SetActive(true);
        _errorText.text = "Leaving...";
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void OpenRoomBrowser()
    {
        CloseEveryPanel();
        roomBrowserPanel.SetActive(true);
    }

    public void JoinRoom(RoomInfo inputInfo)
    {
        PhotonNetwork.JoinRoom(inputInfo.Name);
        CloseEveryPanel();
        _errorPanel.SetActive(true);
        _errorText.text = "Joining Room...";
        CloseEveryPanel();
        Debug.Log("Room Joined");
    }

#endregion


    #region overriden functions

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Text newPlayerLabel = Instantiate(playerNameText, playerNameText.transform.parent);
        newPlayerLabel.text = newPlayer.NickName;
        newPlayerLabel.gameObject.SetActive(true);
        allPlayerNames.Add(newPlayerLabel);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ListAllPlayers();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            startGameButton.SetActive(true);
        }
        else
        {
            startGameButton.SetActive(false);
        }
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;    //this will sync the scene in all players
    }

    public override void OnJoinedLobby()
    {
        makerText.color = Color.green;
        Debug.Log("CONNECTED !!!");

        PhotonNetwork.NickName = Random.Range(0, 1000).ToString();

        if (!hasSetNickName)
        {
            CloseEveryPanel();
            nameInputPanel.SetActive(true);

            if (PlayerPrefs.HasKey("playerName"))
            {
                nameInputField.text = PlayerPrefs.GetString("playerName");
            }
        }
        else
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("playerName");
        }
    }

    public override void OnLeftRoom()
    {
        CloseEveryPanel();
        Debug.Log("Left The Room");
    }

    public override void OnJoinedRoom()
    {
        CloseEveryPanel();
        _roomsPanel.SetActive(true);
        _roomNameText.text = PhotonNetwork.CurrentRoom.Name;
        ListAllPlayers();

        if (PhotonNetwork.IsMasterClient)
        {
            startGameButton.SetActive(true);
        }
        else
        {
            startGameButton.SetActive(false);
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        _createRoomPanel.SetActive(false);
        _errorPanel.SetActive(true);
        _errorText.text = "Cannot Create Room : " + message;
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomButton rb in allRoomButtons)
        {
            Destroy(rb.gameObject);
        }
        allRoomButtons.Clear();

        theRoomButton.gameObject.SetActive(false);

        for(int i = 0; i < roomList.Count; i++)
        {
            if(roomList[i].PlayerCount!=roomList[i].MaxPlayers)
            {
                RoomButton newButton = Instantiate(theRoomButton, theRoomButton.transform.parent);
                newButton.SetButtonDetails(roomList[i]);
                newButton.gameObject.SetActive(true);
                allRoomButtons.Add(newButton);
            }
        }
    }
    #endregion
}

