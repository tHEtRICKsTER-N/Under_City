using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTheImage : MonoBehaviour
{
    [SerializeField]
    private char _rotationAxis;

    void Update()
    {
        if(_rotationAxis=='x')
        transform.Rotate(1, 0, 0);
        else
        transform.Rotate(0, 0, 1);

    }
}
