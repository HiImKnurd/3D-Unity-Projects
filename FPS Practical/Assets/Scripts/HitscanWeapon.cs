using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HitscanWeapon : Weapon
{
    private void Start()
    {
        maxAmmo = _weaponData.maxAmmo;
        currentAmmo = _weaponData.maxAmmo;
    }
    public override void Shoot()
    {
        if (Time.time >= nextFireTime)
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hitinfo, _weaponData.Range))
            {
                Debug.Log("Hit object: " + hitinfo.collider.gameObject.name);
                GameObject hitObject = hitinfo.collider.gameObject;

                if(hitObject.TryGetComponent(out Damagable damagable))
                {
                    damagable.TakeDamage(_weaponData.Damage);
                }
            }

            nextFireTime = Time.time + _weaponData.fireRate;
            currentAmmo--;

            if(currentAmmo <= 0)
            {
                // Add reload codes
                Debug.Log("Reload");
                currentAmmo = _weaponData.maxAmmo;
            }

        }
    }
}
