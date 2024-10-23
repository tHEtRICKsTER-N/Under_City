using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingCars : MonoBehaviour
{
    [SerializeField]
    private float _speed;

    private void Update()
    {
      transform.Translate(Vector3.forward * Time.deltaTime * _speed);
      if(transform.position.z >= 100f)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, -55f);
        }
    }
}
