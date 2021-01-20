using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] private Button m_restartButton;
    [SerializeField] private Text m_scoreText;

    private void Awake()
    {
        SetScore(0);
    }

    private void OnEnable()
    {
        m_restartButton.onClick.AddListener(RestartButton_OnClick);

        GameController.OnScoreUpdated += Instance_OnScoreUpdated;
    }

    private void OnDisable()
    {
        m_restartButton.onClick.RemoveListener(RestartButton_OnClick);

        GameController.OnScoreUpdated -= Instance_OnScoreUpdated;
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
