using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "ScriptableObject/WeaponData", order = 1)]

public class WeaponData : ScriptableObject
{
    public string WeaponName;
    public float Range; // if hitscan weapon
    public float Damage;
    public float fireRate;
    public int maxAmmo;
    public float recoil;
    public float reloadTime;

    public GameObject projectile; // if projectile weapon
}
