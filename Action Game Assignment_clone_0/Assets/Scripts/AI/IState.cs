using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.TextCore.Text;
using UnityEngine.Windows;

public interface IState
{
    public void OnEnter();
    public void OnExit();
    public void OnUpdate();
}
public class IdleState : IState
{
    private InputController _inputs;
    private PlayerController _character;
    private PlayerController _target;
    public IdleState(InputController inputs, PlayerController character, PlayerController target) 
    {
        _inputs = inputs;
        _character = character;
        _target = target;
    }
    public void OnEnter()
    {
        _inputs.ResetInputs();
    }
    public void OnExit()
    {

    }
    public void OnUpdate()
    {

    }
}
public class NeutralState : IState
{
    private InputController _inputs;
    private PlayerController _character;
    private PlayerController _target;

    private float _timer = 0f;
    private float _walktimer = 0f;
    private float _maxTime = 5f; // max duration of the neutral state, will run to player until it can change states
    private float _maxDist = 5f; // max distance between self and target

    public NeutralState(InputController inputs, PlayerController character, PlayerController target, float maxTime = 5f, float maxDist = 5f)
    {
        _inputs = inputs;
        _character = character;
        _target = target;
        _maxTime = maxTime;
        _maxDist = maxDist;
    }
    public void OnEnter()
    {
        _timer = 0f;
        _walktimer = 0f;
        _inputs.ResetInputs();
    }
    public void OnExit()
    {
        _inputs.ResetInputs();
    }
    public void OnUpdate()
    {
        //return;
        _timer += Time.deltaTime;
        _walktimer -= Time.deltaTime;
        Vector3 direction = _target.gameObject.transform.position - _character.transform.position;
        direction.y = 0f;
        if (direction.magnitude > _maxDist || _timer > _maxTime)
        {
            // Runs toward target
            _inputs.move = direction.normalized;
            _walktimer = 0.5f;
            _inputs.run = true;
        }
        // Walk in random direction for 1 second at a time
        if (_walktimer <= 0f)
        {
            _inputs.move = new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10)).normalized;
            _walktimer = 1f;
            _inputs.run = false;
        }
    }
}
public class ApproachState : IState
{
    private InputController _inputs;
    private PlayerController _character;
    private PlayerController _target;
    private EnemyAI _enemyAI;

    private int decision;

    public ApproachState(InputController inputs, PlayerController character, PlayerController target, EnemyAI enemyAI)
    {
        _inputs = inputs;
        _character = character;
        _target = target;
        _enemyAI = enemyAI;
    }
    public void OnEnter()
    {
        decision = -1;
        _inputs.ResetInputs();

        // 50/50 between dashing or jumping at player
        Vector3 direction = _target.gameObject.transform.position - _character.transform.position;
        direction.y = 0f;
        _character.moveDirection = direction.normalized;
        _inputs.move = direction.normalized;
        decision = Random.Range(0, 2);
        if (decision == 0)
        {
            _inputs.jump = true;
        }
        else
        {
            _inputs.dash = true;
        }
    }
    public void OnExit()
    {
        //_inputs.ResetInputs();
    }
    public void OnUpdate()
    {

        //_inputs.ResetInputs();
        Vector3 direction = _target.gameObject.transform.position - _character.transform.position;
        //direction.y = 0f;
        if (decision == 0)
        {
            //_inputs.medAttack = true;
            if (Mathf.Abs(direction.y) < 1.5f && _character._verticalVelocity.y < -2f) // if falling and near target's height
            {
                _inputs.medAttack = true;
                _enemyAI.returntoNeutral = true;
            }
            if (_character._isGrounded && !_character._inJumpSquat && _enemyAI.stateTimer > 0.5f)
            {
                _inputs.medAttack = false;
                _character.ResetCombo();
                _enemyAI.returntoNeutral = true;
            }
            direction.y = 0f;
            _inputs.move = direction.normalized;
        }
        else
        {
            if (!_character._isDashing)
            {
                _inputs.medAttack = true;
                _enemyAI.returntoNeutral = true;
            }
        }
        direction.y = 0f;
        _inputs.move = direction.normalized;
    }
}
public class BlockState : IState
{
    private InputController _inputs;
    private PlayerController _character;
    private PlayerController _target;

    public BlockState(InputController inputs, PlayerController character, PlayerController target)
    {
        _inputs = inputs;
        _character = character;
        _target = target;
    }
    public void OnEnter() 
    { 
        _inputs.ResetInputs();
        
    }
    public void OnExit()
    {
        _inputs.block = false;
    }
    public void OnUpdate()
    {
        _inputs.block = true;
    }
}
public class CounterState : IState
{
    private InputController _inputs;
    private PlayerController _character;
    private PlayerController _target;
    private EnemyAI _enemyAI;

    private int decision;
    public CounterState(InputController inputs, PlayerController character, PlayerController target, EnemyAI enemyAI)
    {
        _inputs = inputs;
        _character = character;
        _target = target;
        _enemyAI = enemyAI;
    }
    public void OnEnter()
    {
        _inputs.ResetInputs();
        decision = -1;

        // 50/50 between retreating and blocking
        Vector3 direction = _target.gameObject.transform.position - _character.transform.position;
        decision = Random.Range(0, 2);
        if (decision == 0 || !_character._isGrounded)
        {
            direction.y = 0f;
            direction *= -1;
            _character.moveDirection = direction.normalized;
            _inputs.move = direction.normalized;
            _inputs.dash = true;
        }
        else
        {
            _inputs.block = true;
        }
    }
    public void OnExit()
    {

    }
    public void OnUpdate()
    {
        if (!_character._isDashing)
            _enemyAI.returntoNeutral = true;
        
    }
}
public class GroundComboState : IState
{
    private InputController _inputs;
    private PlayerController _character;
    private PlayerController _target;

    private int attackType;
    public GroundComboState(InputController inputs, PlayerController character, PlayerController target)
    {
        _inputs = inputs;
        _character = character;
        _target = target;
    }
    public void OnEnter()
    {
        _inputs.ResetInputs();
        attackType = _character._attackType;
    }
    public void OnExit()
    {
        _inputs.ResetInputs();
    }
    public void OnUpdate()
    {
        if (attackType == 1)
        {
            _inputs.lightAttack = true;
        }
        else if (attackType == 2)
        {
            _inputs.medAttack = true;
        }
        
    }
}


