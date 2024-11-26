using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [SerializeField] protected WeaponData _weaponData;
    [SerializeField] protected TMP_Text _reloadingText;
    [SerializeField] protected ParticleSystem _muzzleFlash;
    [SerializeField] protected GameObject _hitEffect;
    [SerializeField] protected AudioClip _shootSound;
    [SerializeField] protected AudioClip _reloadSound;

    protected AudioSource _audioSource;
    public hitEffectSpawner _effectSpawner;

    public float nextFireTime;
    public int currentAmmo;
    public int maxAmmo;
    public float recoil;
    public Vector3 barrelPosition;
    public Vector3 aimPosition;
    public Vector3 hipPosition;
    public float reloadTime;
    public abstract bool Shoot();
    public abstract void PlayShootSound();
    public abstract void PlayReloadSound();
    public abstract void StopSounds();
    public abstract void RefillAmmo();
    private void OnDrawGizmos()
    {
        //Gizmos.DrawSphere(transform.position + barrelPosition, 0.2f);
    }
}
