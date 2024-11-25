using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Pool;

public class hitEffectSpawner : MonoBehaviour
{
    private ObjectPool<GameObject> hitEffectPool;
    [SerializeField] private GameObject hitEffectprefab;
    [SerializeField] private AudioClip hitEffectSound;

    private void Start()
    {
        hitEffectPool = new ObjectPool<GameObject>(
            CreateHitEffect,
            OnTakeFromPool,
            OnReturnToPool,
            OnDestroyEffect,
            true,
            10,
            20
        );
    }

    private GameObject CreateHitEffect()
    {
        GameObject hitEffect = Instantiate(hitEffectprefab);
        hitEffect.SetActive(false);

        return hitEffect;
    }

    private void OnTakeFromPool(GameObject effect)
    {
        effect.SetActive(true);
    }
    public void SpawnHitEffect(Vector3 position, Vector3 normal)
    {
        GameObject effect = hitEffectPool.Get();
        effect.transform.position = position;
        effect.transform.rotation = Quaternion.LookRotation(normal);
        AudioSource.PlayClipAtPoint(hitEffectSound, position);
        StartCoroutine(ReleaseHitEffect(effect, 3));
    }
    private void OnReturnToPool(GameObject effect)
    {
        effect.SetActive(false);
    }
    private void OnDestroyEffect(GameObject effect)
    {
        Destroy(effect);
    }
    private IEnumerator ReleaseHitEffect(GameObject effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        hitEffectPool.Release(effect);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
