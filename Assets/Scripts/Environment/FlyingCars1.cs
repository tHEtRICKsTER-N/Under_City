using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingCars1 : MonoBehaviour
{
    private float _speed=5f;

    private void Update()
    {
      transform.Translate(Vector3.forward * Time.deltaTime * _speed);
      if(transform.position.z <- 70f)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 110f);
        }
    }
}
