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
        recoil = _weaponData.recoil;
        reloadTime = _weaponData.reloadTime;
        _muzzleFlash.transform.localPosition = barrelPosition;
    }
    public override bool Shoot()
    {
        if (Time.time >= nextFireTime)
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hitinfo, _weaponData.Range))
            {
                Debug.Log("Hit object: " + hitinfo.collider.gameObject.name);
                GameObject hitObject = hitinfo.collider.gameObject;

                _effectSpawner.SpawnHitEffect(hitinfo.point, hitinfo.normal);

                if (hitObject.TryGetComponent(out Damagable damagable))
                {
                    damagable.TakeDamage(_weaponData.Damage);
                }
            }

            _muzzleFlash.Play();

            nextFireTime = Time.time + _weaponData.fireRate;
            currentAmmo--;

            return true;
        }
        else return false;
    }

}
