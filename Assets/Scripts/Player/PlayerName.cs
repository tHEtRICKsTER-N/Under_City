using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class PlayerName : MonoBehaviourPun
{
    [SerializeField]
    private TMP_Text _playerName;

    void Start()
    {
        if (photonView.IsMine)
        {
            return;
        }

        SetName();
    }

    void SetName()
    {
        _playerName.text = photonView.Owner.NickName;
    }
}
