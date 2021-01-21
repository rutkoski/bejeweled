using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    [SerializeField] private StateTransition m_StateTransition;
    public StateTransition StateTransition
    {
        get
        {
            if (!m_StateTransition)
            {
                m_StateTransition = new StateTransition();
            }
            return m_StateTransition;
        }
        set
        {
            m_StateTransition = value;
        }
    }

    public virtual State CurrentState
    {
        get
        {
            return _currentState;
        }
        set
        {
            Transition(value);
        }
    }

    protected State _currentState;

    protected bool _inTransition;

    public virtual T GetState<T>() where T : State
    {
        T target = GetComponent<T>();

        if (target == null)
        {
            target = gameObject.AddComponent<T>();
        }

        return target;
    }

    public virtual void ChangeState<T>() where T : State
    {
        CurrentState = GetState<T>();
    }

    protected virtual void Transition(State value)
    {
        if (_currentState == value || _inTransition)
            return;

        _inTransition = true;

        StateTransition.Transition(_currentState, value, () =>
        {
            _currentState = value;
            _inTransition = false;
        });

        //if (_currentState != null)
        //{
        //    _currentState.Exit();
        //}

        //_currentState = value;

        //if (_currentState != null)
        //{
        //    _currentState.Enter();
        //}

        //_inTransition = false;
    }
}
