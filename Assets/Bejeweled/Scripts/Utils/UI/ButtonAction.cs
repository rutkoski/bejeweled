using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonAction : MonoBehaviour
{
    public Button button;

    public Action<ButtonAction> OnExecute;

    protected virtual void Awake()
    {
        if (OnExecute == null)
        {
            OnExecute = buttonAction => {
                this.Execute();
            };
        }
    }

    protected virtual void OnEnable()
    {
        button.onClick.AddListener(_Execute);
    }

    protected virtual void OnDisable()
    {
        if (button)
            button.onClick.RemoveListener(_Execute);
    }

    private void _Execute()
    {
        OnExecute.Invoke(this);
    }

    public virtual void Execute()
    {
        //
    }

#if UNITY_EDITOR
    protected virtual void Reset()
    {
        if (!button)
        {
            button = GetComponent<Button>();
        }
    }
#endif
}
