using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FPSController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private float _jumpHeight = 10f;
    [SerializeField] private float _sensitivity = 10f;
    [SerializeField] private float _gravityScale = -9.81f;
    [SerializeField] private PlayerInput _playerInput;

    private CharacterController _characterController;

    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _jumpAction;
    private Vector3 _move;
    private Vector3 _verticalVelocity = Vector3.zero;
    private Vector3 _mouseDelta;

    // Start is called before the first frame update
    void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _moveAction = _playerInput.actions["Move"];
        _lookAction = _playerInput.actions["Look"];
        _jumpAction = _playerInput.actions["Jump"];
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 input = _moveAction.ReadValue<Vector2>();
        
        Look();
        Jump();

        _move = transform.right * input.x + transform.forward * input.y;

        _verticalVelocity.y += _gravityScale * Time.deltaTime;
        Vector3 finalMove = _verticalVelocity + (_move * _moveSpeed);

        _characterController.Move(finalMove * Time.deltaTime);
    }

    void Look()
    {
        _mouseDelta = _lookAction.ReadValue<Vector2>();

        float mouseX = _mouseDelta.x * _sensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);
    }

    void Jump()
    {
        if(_jumpAction.IsPressed() && _characterController.isGrounded)
        {
            _verticalVelocity.y = Mathf.Sqrt(-2 * _gravityScale * _jumpHeight);
        }
        
        if(_characterController.isGrounded && _verticalVelocity.y < 0)
        {
            _verticalVelocity.y = -1;
        }
    }
}
