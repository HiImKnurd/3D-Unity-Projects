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
        _audioSource = GetComponent<AudioSource>();
    }
    public override bool Shoot()
    {
        if (Time.time >= nextFireTime)
        {
            PlayShootSound();
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

    public override void PlayShootSound()
    {
        if (_audioSource.isPlaying) _audioSource.Stop();
        _audioSource.clip = _shootSound;
        _audioSource.Play();
    }

    public override void PlayReloadSound()
    {
        if (_audioSource.isPlaying) _audioSource.Stop();
        _audioSource.clip = _reloadSound;
        _audioSource.Play();
    }
    public override void StopSounds()
    {
        if (_audioSource.isPlaying) _audioSource.Stop();
    }
}
