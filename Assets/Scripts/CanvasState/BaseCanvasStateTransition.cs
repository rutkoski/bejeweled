using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseCanvasStateTransition : ScriptableObject
{

    public virtual void Init(BaseCanvasState state)
    {
        //
    }

    public virtual void Enter(BaseCanvasState state)
    {
        //
    }

    public virtual void Exit(BaseCanvasState state)
    {
        //
    }
}
