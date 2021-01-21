using System;
using UnityEngine;
using UnityEngine.UI;

public class GameState : BaseCanvasState
{
    [SerializeField] private Button m_restartButton;
    [SerializeField] private Text m_scoreText;

    public override void Enter()
    {
        base.Enter();

        SetScore(0);

        m_restartButton.onClick.AddListener(RestartButton_OnClick);

        GameController.OnScoreUpdated += Instance_OnScoreUpdated;

        GameController.Instance.Container.gameObject.SetActive(true);
    }

    public override void Exit()
    {
        m_restartButton.onClick.RemoveListener(RestartButton_OnClick);

        GameController.OnScoreUpdated -= Instance_OnScoreUpdated;

        base.Exit();
    }

    private void RestartButton_OnClick()
    {
        GameController.Instance.RestartGame();
    }

    private void Instance_OnScoreUpdated(object sender, EventArgs args)
    {
        int score = GameController.Instance.Score;

        SetScore(score);
    }

    private void SetScore(int score)
    {
        m_scoreText.text = $"Score: {score}";
    }
}