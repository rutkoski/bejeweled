using System;
using UnityEngine;
using UnityEngine.UI;

public class EndState : BaseCanvasState
{
    [SerializeField] private Button m_restartButton;
    [SerializeField] private Text m_scoreText;

    public override void Enter()
    {
        base.Enter();

        m_restartButton.onClick.AddListener(RestartButton_OnClick);

        SetScore(GameController.Instance.Score);
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

    private void SetScore(int score)
    {
        m_scoreText.text = $"Score: {score}";
    }
}