using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float fireRate, heatPerShot;
    public bool isAutomatic;
    public GameObject muzzleFlash;
    public int weaponDamage;
    public AudioSource shotSound;
}
