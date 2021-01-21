using System;
using UnityEngine;
using UnityEngine.UI;

public class EndState : BaseCanvasState
{
    [SerializeField] private Button m_restartButton;

    public override void Enter()
    {
        base.Enter();

        m_restartButton.onClick.AddListener(RestartButton_OnClick);
    }

    public override void Exit()
    {
        m_restartButton.onClick.RemoveListener(RestartButton_OnClick);

        base.Exit();
    }

    private void RestartButton_OnClick()
    {
        GameController.Instance.RestartGame();
    }
}