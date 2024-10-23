using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillBoardText : MonoBehaviour
{
    private Transform _camTransform;

    void Start()
    {
        _camTransform = Camera.main.transform;
    }

    private void LateUpdate()
    {
        transform.LookAt(transform.position + _camTransform.rotation * Vector3.forward,_camTransform.rotation*Vector3.up);
    }
}
