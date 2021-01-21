using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXButtonAction : ButtonAction
{

    public SFXData.Type type;

    public override void Execute()
    {
        base.Execute();

        SFX.Instance.PlayOneShot(type);
    }
}
