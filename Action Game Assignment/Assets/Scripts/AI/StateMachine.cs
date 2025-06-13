using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class StateMachine
{
    public IState CurrentState { get; private set; }

    // event to notify other objects of the state change
    public event Action<IState> stateChanged;

    public List<IState> states;

    public StateMachine()
    {
        states = new List<IState>();
    }

    public void AddState(IState state)
    {
        states.Add(state);
    }

    public void Initialize(IState state)
    {
        CurrentState = state;
        CurrentState.OnEnter();
    }

    public void Update()
    {
        CurrentState.OnUpdate();
    }

    public void ChangeState(IState state)
    {
        CurrentState.OnExit();
        CurrentState = state;
        CurrentState.OnEnter();
        Debug.Log("Change state: " + state);
        stateChanged?.Invoke(CurrentState);
    }

}
