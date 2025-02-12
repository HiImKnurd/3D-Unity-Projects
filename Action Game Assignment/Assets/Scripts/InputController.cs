using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    public Vector3 move;
    public bool lightAttack, medAttack, heavyAttack, jump, dash, run, crouch, block, parry;

    [SerializeField] private PlayerInput playerInput;
    private InputActionAsset _inputActions;
    public bool isPlayer = true;

    // Start is called before the first frame update
    void Start()
    {
        if(isPlayer && playerInput != null)
        {
            _inputActions = playerInput.actions;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(isPlayer)
        {
            lightAttack = _inputActions["Light Attack"].WasPressedThisFrame();
            medAttack = _inputActions["Medium Attack"].WasPressedThisFrame();
            heavyAttack = _inputActions["Heavy Attack"].WasPressedThisFrame();
            jump = _inputActions["Jump"].IsPressed();
            dash = _inputActions["Dash"].WasPressedThisFrame();
            run = _inputActions["Run"].IsPressed();
            crouch = _inputActions["Crouch"].IsPressed();
            block = _inputActions["Block"].IsPressed();
            parry = _inputActions["Parry"].WasPressedThisFrame();
            Vector2 input = _inputActions["Move"].ReadValue<Vector2>();
            move = new Vector3(input.x, 0, input.y);
        }
        
    }
    public void ResetInputs()
    {
        lightAttack = false; medAttack = false; heavyAttack = false; jump = false; dash = false; run = false; crouch = false; block = false; move = Vector3.zero;
    }
}
