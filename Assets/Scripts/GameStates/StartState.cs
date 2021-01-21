using UnityEngine;
using UnityEngine.UI;

public class StartState : BaseCanvasState
{
    [SerializeField] private Button m_startButton;

    public override void Enter()
    {
        base.Enter();

        m_startButton.onClick.AddListener(RestartButton_OnClick);

        GameController.Instance.Container.gameObject.SetActive(false);
    }

    public override void Exit()
    {
        m_startButton.onClick.RemoveListener(RestartButton_OnClick);

        base.Exit();
    }

    private void RestartButton_OnClick()
    {
        GameController.Instance.RestartGame();
    }
}
