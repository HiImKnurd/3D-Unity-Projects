using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class ProjectileWeapon : Weapon
{
    // Start is called before the first frame update
    void Start()
    {
        maxAmmo = _weaponData.maxAmmo;
        currentAmmo = _weaponData.maxAmmo;
        recoil = _weaponData.recoil;
        reloadTime = _weaponData.reloadTime;
    }

    public override bool Shoot()
    {
        if (Time.time >= nextFireTime)
        {
            // Add projectile shooting codes
            var projectile = Instantiate(_weaponData.projectile, transform.position + transform.forward, Quaternion.identity);
            projectile.transform.forward = transform.forward;

            nextFireTime = Time.time + _weaponData.fireRate;
            currentAmmo--;

            return true;
        }
        else return false;
    }

    
}
