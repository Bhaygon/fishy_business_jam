using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StateMachine : MonoBehaviour
{
    public State startingState;
    private State _currentState;
    public Transform stateParent;
    public Player Player;
    public TMP_Text stateText;

    private void Start()
    {
        var states = new List<State>(); 
        for (var i = 0; i < stateParent.childCount; i++)
        {
            states.Add(stateParent.GetChild(i).GetComponent<State>());
        }

        foreach (var item in states)
        {
            item.StateInitialize(Player);
        }

        ChangeState(startingState);
    }

    private void Update() 
    {
        var newState = _currentState.StateUpdate();
        if (newState != null) ChangeState(newState);
    }

    private void FixedUpdate() 
    {
        var newState = _currentState.StateFixedUpdate();
        if (newState != null) ChangeState(newState);
    }

    private void ChangeState(State newState)
    {
        if (_currentState) _currentState.StateExit();
        _currentState = newState;
        _currentState.StateStart();
        if (stateText) stateText.text = _currentState.name;
    }
}