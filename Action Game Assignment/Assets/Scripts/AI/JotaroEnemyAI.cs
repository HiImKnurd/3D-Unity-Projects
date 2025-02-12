using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class JotaroEnemyAI : EnemyAI
{
    [SerializeField] PlayerController character;
    [SerializeField] PlayerController target;
    [SerializeField] InputController inputs;

    public float maxNeutralTime = 5f;
    public float maxNeutralDistance = 10f;
    public float approachDistance = 5f;
    public float lockoutTime = 3f;

    private StateMachine statemachine;
    private IdleState idleState;
    private NeutralState neutralState;
    private ApproachState approachState;
    private CounterState counterState;
    private BlockState blockState;
    private GroundComboState groundComboState;

    // Start is called before the first frame update
    void Start()
    {
        character._isPlayer = false;
        stateTimer = 0f;
        returntoNeutral = false;
        character._target = target.gameObject.transform;

        statemachine = new StateMachine();

        idleState = new IdleState(inputs, character, target);
        neutralState = new NeutralState(inputs, character, target, maxNeutralTime, maxNeutralDistance);
        approachState = new ApproachState(inputs, character, target, this);
        counterState = new CounterState(inputs, character, target, this);
        blockState = new BlockState(inputs, character, target);
        groundComboState = new GroundComboState(inputs, character, target);

        statemachine.AddState(idleState);
        statemachine.AddState(neutralState);
        statemachine.AddState(approachState);
        statemachine.AddState(counterState);
        statemachine.AddState(blockState);
        statemachine.AddState(groundComboState);

        statemachine.Initialize(neutralState);
    }

    // Update is called once per frame
    void Update()
    {
        stateTimer += Time.deltaTime;
        Vector3 direction = target.gameObject.transform.position - character.transform.position;

        if (character._isInHitstun)
        {
            stateTimer = 0f;
            if(statemachine.CurrentState != idleState)
            {
                statemachine.ChangeState(idleState);
            }
            return;
        }
        else if(statemachine.CurrentState == idleState)
        {
            statemachine.ChangeState(counterState);
            stateTimer = 0f;
        }

        if(target._isInHitstun && target._knockback.magnitude < 1f && target._isGrounded && character._attackType > 0 && character._attackType < 3)
        {
            stateTimer = 0f;
            if (statemachine.CurrentState != groundComboState)
            {
                statemachine.ChangeState(groundComboState);
            } 
        }
        if(statemachine.CurrentState == groundComboState && character._attackStep >= 3)
        {
            statemachine.ChangeState(counterState);
            stateTimer = 0f;
        }

        if(statemachine.CurrentState == counterState)
        {
            if(inputs.block == true)
            {
                statemachine.ChangeState(blockState);
                stateTimer = 0f;
            }
        }

        if(statemachine.CurrentState == blockState && stateTimer > lockoutTime)
        {
            if (direction.magnitude <= approachDistance)
            {
                statemachine.ChangeState(approachState);
                stateTimer = 0f;
            }
            else returntoNeutral = true;
        }

        if (statemachine.CurrentState == neutralState)
        {
            if(direction.magnitude <= approachDistance && stateTimer > lockoutTime)
            {
                statemachine.ChangeState(approachState);
                stateTimer = 0f;
            }
        }

        if (returntoNeutral)
        {
            statemachine.ChangeState(neutralState);
            stateTimer = 0f;
            returntoNeutral = false;
        }

        statemachine.Update();
    }
}
