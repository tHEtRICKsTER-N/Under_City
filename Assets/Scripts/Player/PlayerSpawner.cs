using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviour
{
    #region STATIC OBJ
    public static PlayerSpawner obj;
    private void Awake()
    {
        obj = this;
    }
    #endregion

    public GameObject playerPrefab;
    private GameObject _player;
    public GameObject deathEffect;
    public float respawnTime;

    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            SpawnPlayer();
        }   
    }

    public void SpawnPlayer()
    {
        Transform spawnPoint = SpawnManager.obj.GetSpawnPoints();
        _player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
        UIManager.obj.healthText.color = Color.cyan;
    }

    public void Die(string whokilledMe)
    {
        PhotonNetwork.Instantiate(deathEffect.name, _player.transform.position, Quaternion.identity);
        UIManager.obj.deathText.text = "You Got Killed By " + whokilledMe;

        // PhotonNetwork.Destroy(_player);
        //SpawnPlayer();

        MatchManager.obj.UpdateStatSend(PhotonNetwork.LocalPlayer.ActorNumber,1,1);

        if (_player != null)
        {
            StartCoroutine(DieCoroutine());
        }

        IEnumerator DieCoroutine()
        {
            PhotonNetwork.Instantiate(deathEffect.name, _player.transform.position, Quaternion.identity);
            PhotonNetwork.Destroy(_player);

            _player = null;

            UIManager.obj.deathScreen.SetActive(true);
            yield return new WaitForSeconds(respawnTime);
            UIManager.obj.deathScreen.SetActive(false);

            if (MatchManager.obj.state == MatchManager.GameState.Playing && _player==null)
            {
                SpawnPlayer();
            }
        }
    }
}
