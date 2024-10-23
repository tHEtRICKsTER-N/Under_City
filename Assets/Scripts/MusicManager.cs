using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager obj;
    private void Awake()
    {
        obj = this;
    }


    [SerializeField]
    private AudioClip[] _songs;
    public AudioSource _audio;

    private void Start()
    {
        _audio.loop = false;
    }

    void Update()
    {
        if (!_audio.isPlaying)
        {
            _audio.clip = _songs[Random.Range(0, _songs.Length)];
            _audio.Play();
        } 
    }

    

}
