using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damagable : MonoBehaviour
{
    public float health = 50f;

    //Hit effect
    private Color _originalColor;
    public Color _damageColor = Color.red;
    private float _damageDuration = 0.3f;
    private Renderer _renderer;

    private void Start()
    {
        _renderer = GetComponent<Renderer>();
        _originalColor = _renderer.material.color;
    }

    private IEnumerator DamageEffect()
    {
        _renderer.material.color = _damageColor;

        float elapseTime = 0.0f;

        while (elapseTime <= _damageDuration)
        {
            _renderer.material.color = Color.Lerp(_damageColor, _originalColor, elapseTime/_damageDuration);
            elapseTime += Time.deltaTime;
            yield return null;
        }

        _renderer.material.color = _originalColor;
    }
    public void TakeDamage(float damage)
    {
        health -= damage;
        Debug.Log("Health: " + health);
        if(_renderer != null)
        {
            StartCoroutine(DamageEffect());
        }

        if(health <= 0) Destroy(gameObject);
    }
}
