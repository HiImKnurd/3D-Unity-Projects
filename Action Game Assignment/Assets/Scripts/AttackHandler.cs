using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class AttackHandler : MonoBehaviour
{
    [SerializeField] 
    private SphereCollider[] _collider;
    [SerializeField]
    Animator _animator;
    [SerializeField]
    GameObject _trailPrefab;
    private List<GameObject> _trails = new List<GameObject>();
    public Attack _currentAttack = null;
    [SerializeField] PlayerController _playerController;
    [SerializeField] Collider selfHurtbox;
    [SerializeField] AudioSource _audioSource;
    [SerializeField] GameObject _hitEffect;
    private void Start()
    {
        foreach (Collider c in _collider)
        {
            GameObject _trail = Instantiate(_trailPrefab, c.transform);
            _trail.transform.position = c.transform.position;
            _trails.Add(_trail);
            _trail.gameObject.SetActive(false);
        }
        DisableAll();
    }
    public void EnableCollider(int i)
    {
        _collider[i].enabled = true;
        //_collider[i].GetComponent<MeshRenderer>().enabled = true;
    }
    public void DisableCollider(int i)
    {
        _collider[i].enabled = false;
        _currentAttack = null;
        _trails[i].gameObject.SetActive(false);
    }
    public void EnableFX(int i)
    {
        _trails[i].gameObject.SetActive(true);
    }
    public void DisableFX(int i)
    {
        _trails[i].gameObject.SetActive(false);
    }
    public void PlaySFX()
    {

        if (_audioSource.isPlaying) _audioSource.Stop();
        if(_currentAttack != null) if(_currentAttack.attackSFX != null) _audioSource.clip = _currentAttack.attackSFX;
        _audioSource.Play();
        
    }
    public void DisableAll()
    {
        int i = 0;
        foreach (Collider c in _collider)
        {
            c.enabled = false;
            //c.GetComponent<MeshRenderer>().enabled = false;
            if (i < _trails.Count)
            {
                _trails[i].gameObject.SetActive(false);
                i++;
            }
        }
        if (_audioSource.isPlaying) _audioSource.Stop();
        _currentAttack = null;
    }
    private void Update()
    {
        // Only detect collision with any object on Target layer
        LayerMask layer = LayerMask.GetMask("Target");
        foreach (SphereCollider collider in _collider)
        {
            if (collider.enabled)
            {
                Collider[] hitColliders =
                Physics.OverlapSphere(collider.transform.position,
                collider.radius, layer);
                for (int i = 0; i < hitColliders.Length; i++)
                {
                    if (hitColliders[i] == selfHurtbox) continue;
                    collider.enabled = false;
                    PlayerController target = hitColliders[i].gameObject.GetComponentInParent<PlayerController>();
                    if (target != null)
                    {
                        if (target._isDead) continue;
                        Debug.Log(target.gameObject.name);
                        _playerController.AttackHit(target);
                        var hiteffect = Instantiate(_hitEffect);
                        hiteffect.transform.position = collider.transform.position;
                    }
                }
            }
        }
    }
}
