using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCanvasState : State
{

    public GameObject canvas;

    public BaseCanvasStateTransition transition;

    [HideInInspector]
    public CanvasGroup canvasGroup;
    
    protected virtual void Awake()
    {
        if (canvas)
        {
            canvasGroup = canvas.GetComponent<CanvasGroup>();
        }

        if (transition)
        {
            transition.Init(this);
        }
        else if (canvas)
        {
            canvas.SetActive(false);
        }
    }

    public override void Enter()
    {
        base.Enter();

        if (transition)
        {
            transition.Enter(this);
        }
        else if (canvas)
        {
            canvas.SetActive(true);
        }
    }

    public override void Exit()
    {
        base.Exit();

        if (transition)
        {
            transition.Exit(this);
        }
        else if (canvas)
        {
            canvas.SetActive(false);
        }
    }
}
