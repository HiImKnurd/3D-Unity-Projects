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
        _audioSource = GetComponent<AudioSource>();
    }

    public override bool Shoot()
    {
        if (Time.time >= nextFireTime)
        {
            // Add projectile shooting codes
            var projectile = Instantiate(_weaponData.projectile, transform.position + transform.forward * 1.5f, Quaternion.identity);
            projectile.transform.forward = transform.forward;

            PlayShootSound();

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

    public override void RefillAmmo()
    {
        currentAmmo += maxAmmo - currentAmmo;
    }
}
