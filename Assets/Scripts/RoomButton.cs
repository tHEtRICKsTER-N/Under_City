using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;

public class RoomButton : MonoBehaviour
{
    public Text buttonText;
    private RoomInfo info;

    public void SetButtonDetails(RoomInfo inputInfo)
    {
        info = inputInfo;
        buttonText.text = info.Name;
    }

    public void OpenRoom()
    {
        MainMenu.obj.JoinRoom(info);
    }

}
