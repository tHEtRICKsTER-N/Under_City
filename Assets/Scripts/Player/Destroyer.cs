using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyer : MonoBehaviour
{
    [SerializeField]
    private int _lifeTime;

    void Start()
    {
        Destroy(this.gameObject, _lifeTime);
    }

}
