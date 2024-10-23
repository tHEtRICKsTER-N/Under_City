using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class MatchManager :MonoBehaviourPunCallbacks,IOnEventCallback
{
    #region STATIC VAR
    public static MatchManager obj;
    private void Awake()
    {
        obj = this;
    }
    #endregion

    public enum EventCodes : byte
    {
        NewPlayers,
        ListPlayers,
        UpdateStat,
        TimerSync
    }

    public List<PlayerInfo> allPlayers = new List<PlayerInfo>();
    private int _index;

    private List<LeaderBoard> lBoardPlayers = new List<LeaderBoard>();

    public enum GameState
    {
        Waiting,
        Playing,
        Ending
    }

    public int killsToWin;
    public Transform mapCamPoint;
    public GameState state = GameState.Waiting;
    public float waitAfterEnding;
    public float matchLength;
    private float _currentMatchTime;
    private float _sendTimer;

    void Start() 
    {
        UIManager.obj.mvpText.SetActive(false);
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            NewPlayersSend(PhotonNetwork.NickName);
            state = GameState.Playing;
            SetupTimer();

            if (!PhotonNetwork.IsMasterClient)
            {
                UIManager.obj.timeText.gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Tab) && state!=GameState.Ending)
        {
            ShowLeaderBoard();
        }
        else if(Input.GetKeyUp(KeyCode.Tab) && state!=GameState.Ending)
        {
            UIManager.obj.leaderBoard.SetActive(false);
        }

        if (PhotonNetwork.IsMasterClient)
        {
            if (_currentMatchTime > 0f && state == GameState.Playing)
            {
                _currentMatchTime -= Time.deltaTime;

                if (_currentMatchTime <= 0f)
                {
                    _currentMatchTime = 0f;
                    state = GameState.Ending;

                    ListPlayersSend();
                    StateCheck();
                }

                UpdateTimerDisplay();

                _sendTimer -= Time.deltaTime;
                if (_sendTimer <= 0)
                {
                    _sendTimer += 1f;
                    TimerSend();
                }
            }
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code < 200)
        {
            EventCodes theEvent = (EventCodes)photonEvent.Code;
            object[] data = (object[])photonEvent.CustomData;

            Debug.Log("Received Event : " + theEvent);

            switch (theEvent)
            {
                case EventCodes.NewPlayers:
                    NewPlayersReceive(data);
                    break;
                case EventCodes.ListPlayers:
                    ListPlayersReceive(data);
                    break;
                case EventCodes.UpdateStat:
                    UpdateStatReceive(data);
                    break;
                case EventCodes.TimerSync:
                    TimerReceive(data);
                    break;
            }
        }
    }

    public override void OnEnable()     //when the matchManager script component attached to the gameobject gets enabled , this will be called 
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()    //same , but when disabled
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void NewPlayersSend(string userName)
    {
        object[] package = new object[4];
        package[0] = userName;
        package[1] = PhotonNetwork.LocalPlayer.ActorNumber;
        package[2] = 0;
        package[3] = 0;

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.NewPlayers,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true }
            );
    }

    public void NewPlayersReceive(object[] dataReceived) 
    {
        PlayerInfo player = new PlayerInfo((string)dataReceived[0],(int)dataReceived[1],(int)dataReceived[2],(int)dataReceived[3]);

        allPlayers.Add(player);
        ListPlayersSend();
    }

    public void ListPlayersSend()
    {
        object[] package = new object[allPlayers.Count+1];

        package[0] = state;

        for(int i = 0; i < allPlayers.Count; i++)
        {
            object[] piece = new object[4];
            piece[0] = allPlayers[i].name;
            piece[1] = allPlayers[i].actor;
            piece[2] = allPlayers[i].kills;
            piece[3] = allPlayers[i].deaths;

            package[i+1] = piece;
        }

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.ListPlayers,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
            );

    }

    public void ListPlayersReceive(object[] dataReceived)
    {
        allPlayers.Clear();

        state = (GameState)dataReceived[0];

        for(int i = 1; i < dataReceived.Length; i++)
        {
            object[] piece = (object[])dataReceived[i];
            PlayerInfo player = new PlayerInfo(
                (string)piece[0],
                (int)piece[1],
                (int)piece[2],
                (int)piece[3]
                );
            allPlayers.Add(player);

            if (PhotonNetwork.LocalPlayer.ActorNumber == player.actor)
            {
                _index = i-1;
            }
        }
        StateCheck();
    }
    
    public void UpdateStatSend(int actorSending,int statToUpdate,int amountToChange)
    {
        object[] package = new object[] { actorSending, statToUpdate, amountToChange };

        PhotonNetwork.RaiseEvent(
          (byte)EventCodes.UpdateStat,
          package,
          new RaiseEventOptions { Receivers = ReceiverGroup.All },
          new SendOptions { Reliability = true }
          );
    }

    public void UpdateStatReceive(object[] dataReceived)
    {
        int actor = (int)dataReceived[0];
        int statType = (int)dataReceived[1];
        int amount = (int)dataReceived[2];

        for(int i = 0; i < allPlayers.Count; i++)
        {
            if (allPlayers[i].actor == actor)
            {
                switch (statType)
                {
                    case 0:             //this is for kills
                        allPlayers[i].kills += amount;
                        Debug.Log("Player " + allPlayers[i].name + " : kills " + allPlayers[i].kills);
                        break;
                    case 1:             //this is for deaths
                        allPlayers[i].deaths += amount;
                        Debug.Log("Player " + allPlayers[i].name + " : deaths " + allPlayers[i].deaths);
                        break;
                }

                if (i == _index)
                {
                    UpdateStatsDisplay();
                }

                if (UIManager.obj.leaderBoard.activeInHierarchy)
                {
                    ShowLeaderBoard();
                }

                break;
            }
        }
        ScoreCheck();
    }

    public void UpdateStatsDisplay()
    {
        if (allPlayers.Count > _index)
        {
            UIManager.obj.KDtext.text = "K/D : " + allPlayers[_index].kills + "/" + allPlayers[_index].deaths;
        }
        else
        {
            UIManager.obj.KDtext.text = "K/D : 0/0";
        }
    }

    void ShowLeaderBoard()
    {
        UIManager.obj.leaderBoard.SetActive(true);
        foreach(LeaderBoard lp in lBoardPlayers)
        {
            Destroy(lp.gameObject);
        }
        lBoardPlayers.Clear();

        UIManager.obj.leaderBoardPlayerDisplay.gameObject.SetActive(false);

        List<PlayerInfo> sorted = SortPlayers(allPlayers);

        foreach(PlayerInfo player in sorted)
        {
            LeaderBoard newPlayerDisplay = Instantiate(UIManager.obj.leaderBoardPlayerDisplay, UIManager.obj.leaderBoardPlayerDisplay.transform.parent);

            newPlayerDisplay.SetDetails(player.name, player.kills, player.deaths);

            newPlayerDisplay.gameObject.SetActive(true);
            lBoardPlayers.Add(newPlayerDisplay);
        }
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        SceneManager.LoadScene("MainMenu");
    }

    void ScoreCheck()
    {
        bool winnerFound = false;
        foreach(PlayerInfo player in allPlayers)
        {
            if (player.kills >= killsToWin && killsToWin > 0)
            {
                winnerFound = true;
                break;
            }
        }

        if (winnerFound)
        {
            if(PhotonNetwork.IsMasterClient && state != GameState.Ending)
            {
                state = GameState.Ending;
                ListPlayersSend();
            }
        }

    }

    void StateCheck()
    {
        if (state == GameState.Ending)
        {
            EndGame();
        }
    }

    void EndGame()          //change =  moved the line showleaderboard to line 374 from 383
    {
        UIManager.obj.deathScreen.SetActive(false);
        ReplaceKillsAndDeaths(allPlayers);
        state = GameState.Ending;

        Debug.Log("Kills and deaths replaced");

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.DestroyAll();
        }

        UIManager.obj.endScreenImage.SetActive(true);
        ShowLeaderBoard();
        UIManager.obj.mvpText.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Camera.main.transform.position = mapCamPoint.position;
        Camera.main.transform.rotation = mapCamPoint.rotation;


        StartCoroutine(EndCo());
    }

    public void SetupTimer()
    {
        if (matchLength > 0)
        {
            _currentMatchTime = matchLength;
            UpdateTimerDisplay();
        }
    }

    public void UpdateTimerDisplay()
    {
        var TimeToDisplay = System.TimeSpan.FromSeconds(_currentMatchTime);

        UIManager.obj.timeText.text = TimeToDisplay.Minutes.ToString("00") + ":" + TimeToDisplay.Seconds.ToString("00");
    }

    public void TimerSend()
    {
        object[] package = new object[] {(int) _currentMatchTime, state };

        PhotonNetwork.RaiseEvent(
           (byte)EventCodes.TimerSync,
           package,
           new RaiseEventOptions { Receivers = ReceiverGroup.All },
           new SendOptions { Reliability = true }
           );
    }

    public void TimerReceive(object[] dataReceived)
    {
        _currentMatchTime = (int)dataReceived[0];
        state = (GameState)dataReceived[1];
        UpdateTimerDisplay();

        UIManager.obj.timeText.gameObject.SetActive(true);
    }

    private IEnumerator EndCo()
    {
        yield return new WaitForSeconds(waitAfterEnding);
        
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
    }

    private List<PlayerInfo> SortPlayers(List<PlayerInfo> players)
    {
        List<PlayerInfo> sorted = new List<PlayerInfo>();

        while (sorted.Count < players.Count)
        {
            int highest = -1;
            PlayerInfo selectedPlayer = players[0];

            foreach(PlayerInfo player in players)
            {
                if (!sorted.Contains(player))
                {
                    if (player.kills > highest)
                    {
                        selectedPlayer = player;
                        highest = player.kills;
                    }
                }
            }

            sorted.Add(selectedPlayer);

        }

        return sorted;
    }

    //void WhoIsMVP(List<PlayerInfo> players)
   // {
   //     string mvpName="";
    //    foreach(PlayerInfo player in players)
    //    {
    //        if (player.kills == killsToWin)
   //         {
   //             mvpName = player.name;
   //             break;
   //         }
    //    }
    //    UIManager.obj.mvpNameText.text = mvpName;
   // }

    void ReplaceKillsAndDeaths(List<PlayerInfo> players)
    {
        // I HAD TO MAKE THIS FUNCTION BECAUSE SOMEHOW , AFTER THE MATCH ENDS, EACH PLAYER'S DEATHS AND KILLS ARE GETTING REPLACED

        foreach(PlayerInfo player in players)
        {
            player.kills = player.kills + player.deaths;
            player.deaths = player.kills - player.deaths;
            player.kills = player.kills - player.deaths;
        }
    }   //made this coz of error
}


[System.Serializable]
public class PlayerInfo
{
    public string name;
    public int actor, kills, deaths;
    public PlayerInfo(string _name,int _actor,int _deaths,int _kills)
    {
        name = _name;
        kills = _kills;
        deaths = _deaths;
        actor = _actor;
    }
}
