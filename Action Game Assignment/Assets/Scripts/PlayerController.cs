using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UIElements.Experimental;
using static UnityEngine.UIElements.UxmlAttributeDescription;
using UnityEngine.UI;
using JetBrains.Annotations;
using UnityEngine.EventSystems;
using Cinemachine;
using System;
using UnityEngine.InputSystem.HID;
using static Unity.VisualScripting.Member;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator _animator;
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] CinemachineVirtualCamera _camera;
    [SerializeField] CinemachineVirtualCamera _freecamera;
    [SerializeField] CinemachineImpulseSource _impulseSource;
    [SerializeField] public Transform _target;
    [SerializeField] AttackHandler _attackHandler;
    [SerializeField] PPManager _effects;
    private InputActionAsset _inputActions;
    private InputController _inputs;
    private CinemachineVirtualCamera _activeCamera;
    public bool _isPlayer = true;

    [Header("Movement")]
    public bool _isRunning = false;
    public Vector3 velocity;
    public Vector3 moveDirection;
    public Vector3 _verticalVelocity = Vector3.zero;
    [SerializeField] float _gravityScale = 1f;
    [SerializeField] float _jumpHeight = 10f;
    [SerializeField] float _airSpeed = 5f;
    public Vector3 _airVelocity;
    public bool _isGrounded = true;
    public bool _inJumpSquat = false;
    public bool _isDashing = false;
    [SerializeField] float _dashSpeed = 10f;
    public float _dashTimer = 0.5f;
    private Vector3 _dashDirection;
    public float _dashCD = 0.75f;

    [Header("Combat")]
    public static System.Action<float, float> OnHealthChanged;
    [SerializeField] Slider _enemyHealthbar;
    public float _health;
    [SerializeField] float _maxHealth = 100;
    public float _stun;
    private float _hitAnimCD;
    public Vector3 _knockback;
    public bool _isInHitstun = false;
    public bool _isKnockedDown = false;
    public bool _isDead = false;
    
    public List<IEnumerator> _attackQueue = new List<IEnumerator>();
    [Header("Combo")]
    public ComboHandler _comboHandler;
    private List<Combo> _combos;
    public int _attackType;
    public int _attackStep;
    public bool _isAttacking;
    public Attack _currentAttack;
    public bool buffered;
    public bool cancel = false;
    public bool _isBlocking;
    public bool _isParrying;

    [Header("Audio")]
    [SerializeField] AudioSource _audioSource;
    [SerializeField] AudioClip _blockSFX;
    [SerializeField] AudioClip _parrySFX;
    [SerializeField] AudioClip _dashSFX;
    [SerializeField] AudioClip _jumpSFX;

    [Header("Effects")]
    [SerializeField] GameObject _shield;
    [SerializeField] GameObject _dashTrail;
    [SerializeField] GameObject _parryEffect;
    private MeshRenderer _shieldRenderer;
    [SerializeField] Material _shieldMaterial;
    [SerializeField] Material _parryMaterial;

    // Start is called before the first frame update
    void Start()
    {
        _inputActions = _playerInput.actions;
        _combos = _comboHandler.combos;
        _inputs = GetComponent<InputController>();
        _impulseSource = GetComponent<CinemachineImpulseSource>();
        if (_isPlayer)
        {
            _camera.Priority = 0; _freecamera.Priority = 0;
            _activeCamera = _camera;
            _activeCamera.Priority = 10;
            _camera.LookAt = _target.transform;
        }
        _health = _maxHealth;

        if(_shield != null) _shieldRenderer = _shield.GetComponent<MeshRenderer>();
    }
    // Update is called once per frame
    void Update()
    {
        if (!_isGrounded && !_isDashing)
        {
            _verticalVelocity.y += _gravityScale * -9.81f * Time.deltaTime;
        }
        Debug.DrawRay(transform.position, Vector3.down * 3f, Color.red);
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 0.5f) && _verticalVelocity.y < 0)
        {
            if (hit.collider.gameObject.layer == 3)
            {
                _isGrounded = true;
                _animator.SetBool("IsJumping", false);
                _animator.SetBool("IsAirborne", false);
            }
        }
        else
        {
            if ((!_isAttacking || _currentAttack.name != "Heavy") && !_isDashing)
            {
                _isGrounded = false;
                if (!_animator.GetBool("IsJumping"))
                {
                    _animator.SetBool("IsAirborne", true);
                    _airVelocity = velocity.normalized * _airSpeed;
                }
            }
        }
        if (_isGrounded && _verticalVelocity.y < 0 && !_inJumpSquat)
        {
            _verticalVelocity.y = -1f;
            _airVelocity = Vector3.zero;
        }

        if (_animator.GetBool("IsDead")) return;
        if (_health < 0)
        {
            _animator.SetBool("IsDead", true);
            _isDead = true;
            if(_enemyHealthbar != null) _enemyHealthbar.gameObject.SetActive(false);
        }

            if (_stun > 0) HitstunResolution();
        else
        {
            HandleInputs();
            Movement();
            HandleAttackInputs();
        }
        if(_dashCD > 0) _dashCD -= Time.deltaTime;

        if(!_isBlocking && !_isParrying) { _shieldRenderer.enabled = false; }
        if(_dashCD <= 0 && !_isDashing) _dashTrail.SetActive(false);

    }
    private void HandleInputs()
    {
        if (_isPlayer)
        {
            if (_inputActions["Change Camera"].WasPressedThisFrame())
            {
                if (_activeCamera == _camera)
                {
                    _camera.Priority = 0;
                    _activeCamera = _freecamera;
                }
                else
                {
                    _freecamera.Priority = 0;
                    _activeCamera = _camera;
                }
                _activeCamera.Priority = 10;
            }

            //if (_inputActions["Change Target"].WasPressedThisFrame() && _activeCamera == _camera)
            //{
            //    // change lockon?
            //}
        }
        if (_inputs.crouch)
        {
            _animator.SetBool("IsCrouching", true);
        }
        else _animator.SetBool("IsCrouching", false);

        if (_inputs.run)
        {
            _isRunning = true;
            _inputs.run = false;
        }
        else _isRunning = false;

        if (_inputs.jump && _isGrounded)
        {
            if (_isAttacking)
            {
                if (IsCurrentAnimationReadyForNextStep(_currentAttack, true))
                {
                    Debug.Log("Cancel");
                    //ResetCombo();
                    cancel = true;
                    _attackHandler.DisableAll();
                    _animator.SetBool("IsJumping", true);
                    _airVelocity = Vector3.zero;
                    _inJumpSquat = true;
                    PlaySFX(_jumpSFX);
                }
            }
            else if (_isDashing)
            {
                if (_dashTimer < 0.1f)
                {
                    _isDashing = false;
                    _animator.SetBool("IsDashing", false);
                    _dashTimer = 0.25f;

                    _animator.SetBool("IsAirborne", true);
                    _airVelocity = _dashDirection * _dashSpeed * 0.65f;
                    Jump();
                    PlaySFX(_jumpSFX);
                    //_inJumpSquat = true;
                }
            }
            else
            {
                _animator.SetBool("IsJumping", true);
                if (_isRunning) _airVelocity = moveDirection * _airSpeed * 1.5f;
                else _airVelocity = moveDirection * _airSpeed;
                _inJumpSquat = true;
                PlaySFX(_jumpSFX);
            }

        }
        _inputs.jump = false;

        if (_inputs.dash && _inputs.move != Vector3.zero && _dashCD <= 0f && !_isDashing && !_animator.GetBool("IsAttacking"))
        {
            if (_isGrounded)
            {
                _dashTimer = 0.25f;
            }
            else _dashTimer = 0.185f;
            _inJumpSquat = false;
            _animator.SetBool("IsDashing", true);
            _isDashing = true;
            _dashDirection = moveDirection.normalized;
            _verticalVelocity.y = 0f;
            PlaySFX(_dashSFX);
            _dashTrail.SetActive(true);
        }
        _inputs.dash = false;

        if (_inputs.block && !_isAttacking && _isGrounded)
        {
            _animator.SetBool("IsBlocking", true);
            _isBlocking = true;
            // Rotate to face target
            Vector3 direction = _target.position - transform.position;
            direction.y = 0;
            Quaternion targetDirection = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetDirection, Time.deltaTime * 1000f);
            _shieldRenderer.enabled = true;
            _shieldRenderer.material = _shieldMaterial;
        }
        else { _animator.SetBool("IsBlocking", false); _isBlocking = false; }
        _inputs.block = false;

        if(_inputs.parry && !_isAttacking && !_isDashing && !_animator.GetBool("IsParrying"))
        {
            _animator.SetBool("IsParrying", true);
            // Rotate to face target
            Vector3 direction = _target.position - transform.position;
            direction.y = 0;
            Quaternion targetDirection = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetDirection, Time.deltaTime * 1000f);
        }
        _inputs.parry = false;
    }
    private void Movement()
    {
        //Vector2 input = _inputActions["Move"].ReadValue<Vector2>();
        //moveDirection = new Vector3(input.x, 0, input.y);
        moveDirection = _inputs.move;
        if (_isDashing)
        {
            if(_dashTimer > 0)
            {
                Quaternion targetDirection = Quaternion.LookRotation(_dashDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetDirection, Time.deltaTime * 1000f);
                _dashTimer -= Time.deltaTime;
                _characterController.Move(_dashDirection * _dashSpeed * Time.deltaTime);
                _verticalVelocity.y = -1f;
                return;
            }
            else
            {
                _isDashing = false;
                _animator.SetBool("IsDashing", false);
                _dashTimer = 0.25f;
                _verticalVelocity.y = -1f;
                _dashCD = 0.75f;
            }
        }
        _animator.SetBool("IsWalking", false);
        _animator.SetBool("IsRunning", false);
        if (moveDirection.magnitude > 0 && !_animator.GetBool("IsAttacking") && !_animator.GetBool("IsBlocking"))
        {
            if (_isRunning && !_animator.GetBool("IsCrouching")) {
                _animator.SetBool("IsRunning", true);
            }
            else _animator.SetBool("IsWalking", true);
            if (_isPlayer)
            {
                // Modify the move direction according to where the camera is facing
                moveDirection =
                Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y,
                Vector3.up) * moveDirection;
                // Rotate the character facing towards the movedirection
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation,
                Time.deltaTime * 500f);
            }
            else
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation,
                Time.deltaTime * 500f);
            }
        }
    }
    private void OnAnimatorMove()
    {
        if (_isDashing) return;
        if (_isInHitstun)
        {
            _characterController.Move(_knockback * 5f * Time.deltaTime);
            _knockback *= 0.95f;
        }
        if (_isGrounded)
        {
            velocity = _animator.deltaPosition;
            _characterController.Move(velocity + _verticalVelocity);
        }
        else
        {
            _characterController.Move(((transform.forward) + (_airVelocity * 0.75f) + _verticalVelocity) * Time.deltaTime);
        }
    }
    public void Jump()
    {
        _verticalVelocity.y = Mathf.Sqrt(-2 * -9.81f * _gravityScale * _jumpHeight);
        _isGrounded = false;
        _animator.SetBool("IsJumping", true);
        _animator.SetBool("IsAirborne", true);
        _inJumpSquat = false;
    }
    private void HandleAttackInputs()
    {
        if (_animator.GetBool("IsBlocking"))
        {
            return;
        }
        if (_isGrounded && !_animator.GetBool("IsJumping") && !_inJumpSquat)
        {
            if (_inputs.heavyAttack)
            {
                if (_attackQueue.Count == 0)
                {
                    _attackType = 3;
                    _attackQueue.Add(PerformAttack());
                    StartCombo();
                    _inputs.heavyAttack = false;
                    return;
                }
                else if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.3f && _currentAttack.attackTransitions.Contains(3) && !buffered)
                {
                    buffered = true;
                    _attackType = 3;
                    _attackQueue.Add(PerformAttack());
                    _inputs.heavyAttack = false;
                    return;
                }
            }
            if (_inputs.medAttack)
            {
                if (_attackQueue.Count == 0)
                {
                    _attackType = 2;
                    _attackQueue.Add(PerformAttack());
                    StartCombo();
                    _inputs.medAttack = false;
                    return;
                }
                else if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.3f && _currentAttack.attackTransitions.Contains(2) && !buffered)
                {
                    buffered = true;
                    _attackType = 2;
                    _attackQueue.Add(PerformAttack());
                    _inputs.medAttack = false;
                    return;
                }
            }
            if (_inputs.lightAttack)
            {
                if (_attackQueue.Count == 0)
                {
                    _attackType = 1;
                    _attackQueue.Add(PerformAttack());
                    StartCombo();
                    _inputs.lightAttack = false;
                    return;
                }
                else if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.3f && _currentAttack.attackTransitions.Contains(1) && !buffered)
                {
                    buffered = true;
                    _attackType = 1;
                    _attackQueue.Add(PerformAttack());
                    _inputs.lightAttack = false;
                    return;
                }
            }
        }
        if(!_isGrounded)
        {
            if (_inputs.heavyAttack ||
                _inputs.medAttack ||
                _inputs.lightAttack)
            {
                if(_attackQueue.Count == 0){
                    _attackStep = 0;
                    _attackType = 4;
                    _attackQueue.Add(PerformAttack());
                    StartCombo();
                    Debug.Log("air attack");
                    _inputs.lightAttack = false;
                    _inputs.medAttack = false;
                    _inputs.heavyAttack = false;
                    return;
                }
            }
        }
    }
    private void StartCombo()
    {
        _isAttacking = true;
        _animator.SetBool("IsAttacking", _isAttacking);
        _animator.SetInteger("AttackType", _attackType);
        StartCoroutine(_attackQueue[0]);
    }
    private IEnumerator PerformAttack()
    {
        _attackStep++;
        _animator.SetInteger("AttackStep", _attackStep);
        _animator.SetInteger("AttackType", _attackType);
        _currentAttack = _combos[_attackType - 1].attacks[_attackStep - 1];
        _attackHandler._currentAttack = _currentAttack;
        buffered = false;
        
        while (!IsCurrentAnimationReadyForNextStep(_currentAttack, true))
        {
            if (cancel)
            {
                ResetCombo();
                yield break;
            }
            if(_isGrounded && _attackType == 4)
            {
                ResetCombo();
                yield break;
            }
            if (!_isDashing)
            {
                // Rotate to face target
                Vector3 direction = _target.position - transform.position;
                direction.y = 0;
                Quaternion targetDirection = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetDirection, Time.deltaTime * 1000f);
            }
            yield return null;
        }
        while (!IsCurrentAnimationReadyForNextStep(_currentAttack, false))
        {
            if (cancel)
            {
                ResetCombo();
                yield break;
            }
            if (_isGrounded && _attackType == 4)
            {
                ResetCombo();
                yield break;
            }
            if (_attackStep < _attackQueue.Count)
            {
                Debug.Log("next attack");
                StartCoroutine(_attackQueue[_attackStep]);
                yield break;
            }
            else yield return null;
        }
        ResetCombo();
    }
    private bool IsCurrentAnimationReadyForNextStep(Attack attack, bool cancel)
    {
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        float endTime = 0.9f;
        if (cancel) endTime = attack.cancelTime;

        return stateInfo.normalizedTime >= endTime && stateInfo.IsName(attack.name); 
    }
    public void ResetCombo()
    {
        Debug.Log("clear combo");
        _isAttacking = false;
        _attackStep = 0;
        _attackType = 0;
        _animator.SetInteger("AttackStep", _attackStep);
        _animator.SetBool("IsAttacking", _isAttacking);
        _animator.SetInteger("AttackType", _attackType);
        _attackQueue.Clear();
        _attackHandler.DisableAll();
        //_currentAttack = null;
        //Vector2 input = _inputActions["Move"].ReadValue<Vector2>();
        //moveDirection = new Vector3(input.x, 0, input.y);
        moveDirection = _inputs.move;
        if (moveDirection.magnitude > 0)
        {
            _animator.SetBool("IsWalking", true);
        }
        cancel = false;
    }
    public void AttackHit(PlayerController target)
    {
        Vector3 direction = transform.forward;
        if (_currentAttack.upwardKnockback == 0) direction.y = 0;
        else direction.y = 1;
        if (_currentAttack.forwardKnockback == 0)
        {
            direction.x = 0;
            direction.z = 0;
        }
        target.OnHit(_currentAttack, direction.normalized, this);
    }
    public void OnHit(Attack attack, Vector3 knockbackDirection, PlayerController attacker)
    {
        if (_isParrying)
        {
            attacker.Parried();
            _animator.SetBool("IsParrying", false);
            _isParrying = false;
            _shieldRenderer.enabled = false;
            PlaySFX(_parrySFX);
            var parryEffect = Instantiate(_parryEffect, this.gameObject.transform);
            parryEffect.transform.position = _shield.transform.position;
            _effects.StartHitstop(0.2f, true);
            return;
        }
        _isInHitstun = true;
        if (_isBlocking)
        {
            _stun = attack.hitstun * 0.5f;
            _health -= attack.damage * 0.1f;
            PlaySFX(_blockSFX);
        }
        else
        {
            _stun = attack.hitstun;
            _health -= attack.damage; 
            _knockback = knockbackDirection * attack.forwardKnockback;
            _knockback.y = 0;
            if(attack.upwardKnockback > 0)
            {
                _verticalVelocity.y = Mathf.Sqrt(-2 * -9.81f * _gravityScale * attack.upwardKnockback);
                _isGrounded = false;
                _animator.SetBool("IsAirborne", true);
            }
            if (_knockback.magnitude > 5)
            {
                _impulseSource.GenerateImpulse(Camera.main.transform.forward);
                _effects.StartHitstop(0.2f, true);
            }
            _animator.SetBool("IsHitThisFrame", true);
            _hitAnimCD = 0.1f;
            _animator.SetBool("InHitstun", true);
            _isInHitstun = true;
            _isDashing = false;
            _animator.SetBool("IsDashing", false);
            ResetCombo();

            PlaySFX(attack.hitSFX);
            _shieldRenderer.enabled = false;
            _dashTrail.SetActive(false);
        }
        Vector3 direction = -knockbackDirection;
        direction.y = 0;
        Quaternion targetDirection = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetDirection, Time.deltaTime * 1000f);

        if(_isPlayer) OnHealthChanged?.Invoke(_health, _maxHealth);
        else _enemyHealthbar.value = _health / _maxHealth;
    }
    private void HitstunResolution()
    {
        _stun -= Time.deltaTime;
        _hitAnimCD -= Time.deltaTime;
        if (_hitAnimCD <= 0) _animator.SetBool("IsHitThisFrame", false);
        if(_stun <= 0) {
            _isInHitstun = false;
            _animator.SetBool("InHitstun", false);
            _knockback = Vector3.zero;
        }
    }
    public void StartParry()
    {
        _isParrying = true;
        _shieldRenderer.enabled = true;
        _shieldRenderer.material = _parryMaterial;
    }
    public void EndParry()
    {
        _isParrying = false;
        _animator.SetBool("IsParrying", false );
        _animator.SetBool("InHitstun", true);
        _shieldRenderer.enabled = false;
        _isInHitstun = true;
        _stun = 0.5f;
        _knockback = Vector3.zero;
    }
    public void Parried()
    {
        _animator.SetBool("IsHitThisFrame", true);
        _hitAnimCD = 0.1f;
        _animator.SetBool("InHitstun", true);
        _isInHitstun = true;
        _stun = 1f;
        _knockback = Vector3.zero;
        ResetCombo();
    }
    public void PlaySFX(AudioClip sfx)
    {
        if (_audioSource.isPlaying) _audioSource.Stop();
        _audioSource.clip = sfx;
        _audioSource.Play();
    }
}

