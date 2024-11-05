using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
    [SerializeField] private float _moveSpeedTransition = 2f;
    [SerializeField] private float _normalHeight = 2f;
    [SerializeField] private float _crouchHeight = 1f;
    [SerializeField] private PlayerInput _playerInput;

    private CharacterController _characterController;

    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _jumpAction;
    private InputAction _sprintAction;
    private InputAction _crouchAction;

    private Vector3 _move;
    public float speed;
    public bool crouching = false;
    private Vector3 _verticalVelocity = Vector3.zero;

    private Vector3 _mouseDelta;
    private float cameraPitch = 0f;

    [SerializeField] private float _coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    private bool moving = false;
    private float bobOffset;
    [SerializeField] private float _bobFrequency;
    [SerializeField] private float _bobAmplitude;
    private float timer = 0f;
    private float originalCamY;

    private InputAction _shootAction;
    private Vector3 originalPosition;
    private Vector3 shakeOffset;
    [SerializeField] private Vector3 _shakeStrength;
    private float shakeDuration = 0.1f;
    private float shakeTimer = 0f;

    [SerializeField] private GameObject Weapon;
    public Weapon _currentWeapon;
    public static System.Action<int, int> OnAmmoChanged;

    private InputAction _pickupAction;
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
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        speed = _moveSpeed;
        _characterController.height = _normalHeight;
        originalCamY = Camera.main.transform.position.y;
        originalPosition = Camera.main.transform.position;
        _currentWeapon = Weapon.GetComponent<Weapon>();
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

        _move = transform.right * input.x + transform.forward * input.y;
        _verticalVelocity.y += _gravityScale * -9.81f * Time.deltaTime;

        if(_sprintAction.IsPressed() && !crouching) _currentSpeedMult = _sprintSpeedMult;
        else _currentSpeedMult = 1f;

        speed = Mathf.Lerp(speed, _moveSpeed * _currentSpeedMult, _moveSpeedTransition * Time.deltaTime);
        if (_move != Vector3.zero && _characterController.isGrounded) moving = true;
        else moving = false;

        Vector3 finalMove = _verticalVelocity + (_move * speed);

        _characterController.Move(finalMove * Time.deltaTime);
    }

    private void LateUpdate()
    {
        HandleCameraPitch();
        BobHandler();
        HandleCameraShake();

        Camera.main.transform.localPosition = originalPosition + shakeOffset + new Vector3(0f, bobOffset, 0f);
    }

    void HandleCameraPitch()
    {
        float mouseY = _mouseDelta.y * _verticalSensitivity * Time.deltaTime;

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);
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

        Transform camtransform = Camera.main.transform;
        camtransform.localPosition = new Vector3(camtransform.localPosition.x, originalCamY + bobOffset, camtransform.localPosition.z);
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
}
        else
        {
            _characterController.height = _normalHeight;
            _characterController.center = new Vector3(0, 0, 0);
            crouching = false;
        }
    }
    private void Shoot()
    {
        if (_shootAction.IsPressed())
        {
            shakeTimer = shakeDuration;
            _currentWeapon.Shoot();
            InvokeAmmoChanged();
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
                Destroy(hitinfo.collider.gameObject);
            }
        }
    }

    public void InvokeAmmoChanged()
    {
        OnAmmoChanged?.Invoke(_currentWeapon.currentAmmo, _currentWeapon.maxAmmo);
    }
}
