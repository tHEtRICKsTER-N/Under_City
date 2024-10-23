using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager obj;

    [SerializeField]
    private Transform[] _spawnPoints;

    private void Awake()
    {
        obj = this;
    }


    void Start()
    {
        foreach(Transform t in _spawnPoints)
        {
            t.gameObject.SetActive(false);
        }   
    }

    public Transform GetSpawnPoints()
    {
        return _spawnPoints[Random.Range(0, _spawnPoints.Length)];
    }
}
