using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [SerializeField] protected WeaponData _weaponData;
    [SerializeField] protected TMPro.TextMeshPro _ammoText;
    public float nextFireTime;
    public int currentAmmo;
    public int maxAmmo;
    public abstract void Shoot();
}
