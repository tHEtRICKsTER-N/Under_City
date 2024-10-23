using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderBoard : MonoBehaviour
{
    public Text playerNameText, killsText, deathsText;
    
    public void SetDetails(string name,int kills,int deaths)
    {
        playerNameText.text = name;
        killsText.text = kills.ToString();
        deathsText.text = deaths.ToString();
    }
}
