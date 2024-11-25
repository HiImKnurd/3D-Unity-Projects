using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class FPSController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private float _jumpHeight = 10f;
    [SerializeField] private float _verticalSensitivity = 10f;
    [SerializeField] private float _horizontalSensitivity = 20f;
    [SerializeField] private float _gravityScale = 1f;
    [SerializeField] private float _currentSpeedMult = 1f;
    [SerializeField] private float _sprintSpeedMult;
    [SerializeField] private float _moveSpeedTransition = 4f;
    [SerializeField] private float _normalHeight = 2f;
    [SerializeField] private float _crouchHeight = 1f;
    [SerializeField] private PlayerInput _playerInput;

    private CharacterController _characterController;

    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _jumpAction;
    private InputAction _sprintAction;
    private InputAction _crouchAction;
    private InputAction _aimAction;
    private InputAction _switchWeaponAction;
    private InputAction _reloadAction;

    private Vector3 _move;
    public float speed;
    public bool crouching = false;
    public bool sliding = false;
    private Vector3 _verticalVelocity = Vector3.zero;
    public float slideSpeed;
    private Vector3 slideDirection;
    [SerializeField] private float slideDeceleration = 1f;

    private Vector3 _mouseDelta;
    private float cameraPitch = 0f;
    public float targetcameraPitch;
    private float rotationY = 0.0f;

    [SerializeField] private float _coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    [Header("Camera Settings")]
    private bool moving = false;
    public float bobOffset;
    [SerializeField] private float _bobFrequency;
    [SerializeField] private float _bobAmplitude;
    private float timer = 0f;
    private float originalCamY;

    private InputAction _shootAction;
    private Vector3 originalPosition;
    public Vector3 shakeOffset;
    [SerializeField] private Vector3 _shakeStrength;
    private float shakeDuration = 0.1f;
    private float shakeTimer = 0f;

    [SerializeField] private Camera _weaponCamera;
    [SerializeField] private float _FOVtransition = 7f;
    [SerializeField] private float _sprintFOVmult = 0.8f;
    [SerializeField] private float _aimFOVmult = 1.3f;
    private float sprintFOV;
    private float aimFOV;
    public float normalFOV;
    public float currentFOV;
    public bool isSpeeding = false;
    public bool isAiming = false;
    private Vector3 aimingSpeed = Vector3.zero;
    private float movementforward;

    [SerializeField] private float _recoilOffsetY = 0.3f;
    public float _currentRecoilY;

    [Header("Weapon List")]
    public List<Weapon> weapons = new List<Weapon>();
    //[SerializeField] private GameObject Weapon;
    private int _currentWeaponIndex = 0;
    public Weapon _currentWeapon;
    public static System.Action<int, int> OnAmmoChanged;
    public GameObject _weaponHolder;
    private bool reloading;
    [SerializeField] private TMP_Text _reloadingText;

    private InputAction _pickupAction;
    [SerializeField] private hitEffectSpawner _hitEffectSpawner;
    [SerializeField] LayerMask _itemLayer;

    // Start is called before the first frame update
    void Start()
    {
        _characterController = GetComponent<CharacterController>();

        _moveAction = _playerInput.actions["Move"];
        _lookAction = _playerInput.actions["Look"];
        _jumpAction = _playerInput.actions["Jump"];
        _sprintAction = _playerInput.actions["Sprint"];
        _crouchAction = _playerInput.actions["Crouch"];
        _shootAction = _playerInput.actions["Shoot"];
        _pickupAction = _playerInput.actions["Pick Up"];
        _aimAction = _playerInput.actions["Aim"];
        _switchWeaponAction = _playerInput.actions["Switch Weapon"];
        _reloadAction = _playerInput.actions["Reload"];

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        speed = _moveSpeed;
        _characterController.height = _normalHeight;

        targetcameraPitch = cameraPitch;
        originalCamY = Camera.main.transform.localPosition.y;
        originalPosition = Camera.main.transform.localPosition;
        currentFOV = normalFOV = Camera.main.fieldOfView;
        sprintFOV = normalFOV * _sprintFOVmult;
        aimFOV = normalFOV * _aimFOVmult;

        _currentWeapon = weapons[_currentWeaponIndex].GetComponent<Weapon>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 input = _moveAction.ReadValue<Vector2>();
        
        Look();
        Jump();
        Crouch();
        Shoot();
        PickUp();
        SwitchWeapon();
        
        if(_reloadAction.IsPressed() && !reloading && _currentWeapon.currentAmmo < _currentWeapon.maxAmmo)
        {
            StartCoroutine(Reload());
        }

        _move = transform.right * input.x + transform.forward * input.y;
        _verticalVelocity.y += _gravityScale * -9.81f * Time.deltaTime;

        if (_sprintAction.IsPressed() && !crouching && !isAiming)
        {
            _currentSpeedMult = _sprintSpeedMult;
        }
        else _currentSpeedMult = 1f;

        if (sliding)
        {
            slideSpeed = Mathf.Lerp(slideSpeed, 0f, slideDeceleration * Time.deltaTime);
        }
        else speed = Mathf.Lerp(speed, _moveSpeed * _currentSpeedMult, _moveSpeedTransition * Time.deltaTime);

        movementforward = Vector3.Magnitude(transform.forward - _move);
        if (movementforward < 1f && speed > _moveSpeed * 1.1f) isSpeeding = true;
        else isSpeeding = false;

        if (_aimAction.IsPressed()) isAiming = true;
        else isAiming = false;

        if (_move != Vector3.zero && _characterController.isGrounded && !sliding) moving = true;
        else moving = false;

        Vector3 finalMove;
        if (sliding)
        {
            finalMove = _verticalVelocity + (slideSpeed * slideDirection);
        }
        else finalMove = _verticalVelocity + (_move * speed);

        _characterController.Move(finalMove * Time.deltaTime);
    }

    private void LateUpdate()
    {
        HandleCameraPitch();
        BobHandler();
        HandleCameraShake();
        HandleCameraFOV();

        Camera.main.transform.localPosition = originalPosition + shakeOffset + new Vector3(0f, bobOffset, 0f);
    }
    void HandleCameraPitch()
    {
        float mouseY = _mouseDelta.y * _verticalSensitivity * Time.deltaTime;

        targetcameraPitch -= mouseY;
        targetcameraPitch = Mathf.Clamp(targetcameraPitch, -90f, 90f);

        cameraPitch = Mathf.SmoothDamp(cameraPitch, targetcameraPitch, ref rotationY, 0.01f);

        Camera.main.transform.localRotation = Quaternion.Euler(cameraPitch, 0, 0);
    }
    void BobHandler()
    {
        if(moving)
        {
            timer += _bobFrequency * _currentSpeedMult * Time.deltaTime;
            bobOffset = Mathf.Sin(timer) * _bobAmplitude;
        }
        else
        {
            bobOffset = Mathf.Lerp(bobOffset, 0, Time.deltaTime);
        }

        //Transform camtransform = Camera.main.transform;
        //camtransform.localPosition = new Vector3(camtransform.localPosition.x, originalCamY + bobOffset, camtransform.localPosition.z);
        //Camera.main.transform.localPosition = camtransform.localPosition;
    }
    void HandleCameraShake()
    {
        if(shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
            shakeOffset = Random.insideUnitCircle * _shakeStrength;
        }
        else
        {
            shakeOffset = Vector3.zero;
        }
    }
    void HandleCameraFOV()
    {
        if (isAiming)
        {
            currentFOV = Mathf.Lerp(currentFOV, aimFOV, _FOVtransition * Time.deltaTime);
            _currentWeapon.transform.localPosition = Vector3.SmoothDamp(_currentWeapon.transform.localPosition, 
                new Vector3(_currentWeapon.aimPosition.x, _currentWeapon.aimPosition.y, _currentWeapon.aimPosition.z), ref aimingSpeed, 0.02f);
        }
        else
        {
            _currentWeapon.transform.localPosition = Vector3.SmoothDamp(_currentWeapon.transform.localPosition, 
                new Vector3(_currentWeapon.hipPosition.x, _currentWeapon.hipPosition.y, _currentWeapon.hipPosition.z), ref aimingSpeed, 0.02f);
            if (isSpeeding || crouching)
            {
                currentFOV = Mathf.Lerp(currentFOV, sprintFOV, _FOVtransition * Time.deltaTime);
            }
            else
            {
                currentFOV = Mathf.Lerp(currentFOV, normalFOV, _FOVtransition * Time.deltaTime);
            }
        }

        Camera.main.fieldOfView = currentFOV;
        _weaponCamera.fieldOfView = currentFOV;
    }
    void HandleCameraRecoil()
    {  
        _currentRecoilY = Random.Range(0.75f, 1.0f) * _currentWeapon.recoil;
        if (isAiming) _currentRecoilY *= 0.6f;
        targetcameraPitch -= Mathf.Abs(_currentRecoilY);
    }
    void Look()
    {
        _mouseDelta = _lookAction.ReadValue<Vector2>();

        float mouseX = _mouseDelta.x * _horizontalSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);
    }

    void Jump()
    {
        if (_characterController.isGrounded) coyoteTimeCounter = _coyoteTime;
        else coyoteTimeCounter -= Time.deltaTime;

        if (_jumpAction.IsPressed() && coyoteTimeCounter > 0f)
        {
            _verticalVelocity.y = Mathf.Sqrt(-2 * -9.81f * _gravityScale * _jumpHeight);
        }
        
        if(_characterController.isGrounded && _verticalVelocity.y < 0)
        {
            _verticalVelocity.y = -1;
        }
    }
    private void Crouch()
    {
        if (_crouchAction.IsPressed())
        {
            _characterController.height = _crouchHeight;
            _characterController.center = new Vector3(0, _crouchHeight / 2, 0);
            crouching = true;
            if (isSpeeding && !sliding)
            {
                slideSpeed = speed;
                slideDirection = _move;
                sliding = true;
            }
}
        else
        {
            _characterController.height = _normalHeight;
            _characterController.center = new Vector3(0, 0, 0);
            crouching = false;
            if (sliding)
            {
                sliding = false;
                speed = slideSpeed;
            }
        }
    }
    private void Shoot()
    {
        if (_shootAction.IsPressed() && !reloading)
        {
            if (_currentWeapon.Shoot())
            {
                shakeTimer = shakeDuration;
                InvokeAmmoChanged();
                HandleCameraRecoil();
                if (_currentWeapon.currentAmmo <= 0) StartCoroutine(Reload());
            }
        }
    }

    private void PickUp()
    {
        if (_pickupAction.IsPressed()) 
        {
            Debug.Log("Pick up called");
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hitinfo, 5, _itemLayer))
            {
                Debug.Log("Pick up hit");
                Debug.Log("pickup object: " + hitinfo.collider.gameObject.name);
                Item item = hitinfo.collider.GetComponent<Item>();
                item.Use(this);
                if (hitinfo.collider.gameObject.TryGetComponent(out AmmoItem ammoitem))
                {
                    Destroy(hitinfo.collider.gameObject);
                }
                else if(hitinfo.collider.gameObject.TryGetComponent(out Weapon weapon))
                {
                    weapon._effectSpawner = _hitEffectSpawner;
                }
            }
        }
    }
    private void SwitchWeapon()
    {
        Vector2 scroll = _switchWeaponAction.ReadValue<Vector2>();
        if (scroll.y < 0)
        {
            reloading = false;
            _currentWeapon.StopSounds();
            _currentWeapon.gameObject.SetActive(false);
            _currentWeaponIndex++;
            _currentWeaponIndex %= weapons.Count;
            _currentWeapon = weapons[_currentWeaponIndex];
            _currentWeapon.gameObject.SetActive(true);
            _currentWeapon.StopSounds();
            InvokeAmmoChanged();
        }
        else if(scroll.y > 0)
        {
            reloading = false;
            _currentWeapon.StopSounds();
            _currentWeapon.gameObject.SetActive(false);
            _currentWeaponIndex--;
            _currentWeaponIndex %= weapons.Count;
            if (_currentWeaponIndex < 0) _currentWeaponIndex = weapons.Count - 1;
            _currentWeapon = weapons[_currentWeaponIndex];
            _currentWeapon.gameObject.SetActive(true);
            _currentWeapon.StopSounds();
            InvokeAmmoChanged();
        }
    }
    private IEnumerator Reload()
    {
        reloading = true;
        _reloadingText.gameObject.SetActive(true);
        _currentWeapon.PlayReloadSound();

        yield return new WaitForSeconds(_currentWeapon.reloadTime);

        _currentWeapon.currentAmmo = _currentWeapon.maxAmmo;
        reloading = false;
        _reloadingText.gameObject.SetActive(false);
        OnAmmoChanged?.Invoke(_currentWeapon.currentAmmo, _currentWeapon.maxAmmo);
    }

    public void InvokeAmmoChanged()
    {
        OnAmmoChanged?.Invoke(_currentWeapon.currentAmmo, _currentWeapon.maxAmmo);
    }
}
