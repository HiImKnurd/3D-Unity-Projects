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
    [SerializeField] private LightSource _lightSource;

    private CharacterController _characterController;

    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _jumpAction;

    private Vector3 _move;
    public float speed;
    private Vector3 _verticalVelocity = Vector3.zero;

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

    // Start is called before the first frame update
    void Start()
    {
        _characterController = GetComponent<CharacterController>();

        _moveAction = _playerInput.actions["Move"];
        _lookAction = _playerInput.actions["Look"];
        _jumpAction = _playerInput.actions["Jump"];

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        speed = _moveSpeed;
        _characterController.height = _normalHeight;

        targetcameraPitch = cameraPitch;
        originalCamY = Camera.main.transform.localPosition.y;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 input = _moveAction.ReadValue<Vector2>();
        
        Look();
        Jump();

        _move = transform.right * input.x + transform.forward * input.y;
        _verticalVelocity.y += _gravityScale * -9.81f * Time.deltaTime;

       
        speed = Mathf.Lerp(speed, _moveSpeed * _currentSpeedMult, _moveSpeedTransition * Time.deltaTime);

        Vector3 finalMove;
        finalMove = _verticalVelocity + (_move * speed);

        _characterController.Move(finalMove * Time.deltaTime);
    }

    private void LateUpdate()
    {
        HandleCameraPitch();
    }
    void HandleCameraPitch()
    {
        float mouseY = _mouseDelta.y * _verticalSensitivity * Time.deltaTime;

        targetcameraPitch -= mouseY;
        targetcameraPitch = Mathf.Clamp(targetcameraPitch, -90f, 90f);

        cameraPitch = Mathf.SmoothDamp(cameraPitch, targetcameraPitch, ref rotationY, 0.01f);

        Camera.main.transform.localRotation = Quaternion.Euler(cameraPitch, 0, 0);
        _lightSource.transform.localRotation = Quaternion.Euler(cameraPitch - 90f, 0, 0);
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
}
