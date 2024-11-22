using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Missile : Projectile
{
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private float explosionRadius = 2f;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * travelSpeed;
    }

    private void Update()
    {
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            GameObject hitObject = hit.gameObject;
            if (hitObject.TryGetComponent(out Damagable damagable))
            {
                damagable.TakeDamage(damage);
            }
            else if(hitObject.TryGetComponent(out FPSController player))
            {
                Debug.Log("ouch");
                
            }
        }

        Instantiate(explosionEffect, transform.position, Quaternion.identity);
        Destroy(this.gameObject);
    }

    private void OnDrawGizmos()
    {
        //Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
