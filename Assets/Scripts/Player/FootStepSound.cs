using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootStepSound : MonoBehaviour
{
    public AudioSource footStepSound;

    void FootStep()
    {
        footStepSound.Play();
    }
}
