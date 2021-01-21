using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    private StateMachine m_stateMachine;

    private void Awake()
    {
        m_stateMachine = gameObject.AddComponent<StateMachine>();
    }

    private void Start()
    {
        ShowStart();
    }

    public void ShowStart()
    {
        m_stateMachine.ChangeState<StartState>();
    }

    public void ShowGame()
    {
        m_stateMachine.ChangeState<GameState>();
    }

    public void ShowEnd()
    {
        m_stateMachine.ChangeState<EndState>();
    }
}
