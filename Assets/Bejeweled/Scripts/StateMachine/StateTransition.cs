using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StateTransition")]
public class StateTransition : ScriptableObject
{
    public virtual void Transition(State from, State to, Action transitionDone)
    {
        if (from != null)
        {
            from.Exit();
        }

        if (to != null)
        {
            to.Enter();
        }

        transitionDone.Invoke();
    }
}
