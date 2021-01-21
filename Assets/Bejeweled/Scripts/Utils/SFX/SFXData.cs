using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SFXSettings")]
public class SFXData : ScriptableObject
{

    public enum Type
    {
        ButtonDefault = 0,
        Positive = 100,
        Negative = 200,
    }

    public AudioClip buttonDefaultSound;
    public AudioClip positiveSound;
    public AudioClip negativeSound;

    public AudioClip GetClip(Type type)
    {
        switch (type)
        {
            case Type.ButtonDefault:
                return buttonDefaultSound;
            case Type.Positive:
                return positiveSound;
            case Type.Negative:
                return negativeSound;
        }

        return null;
    }
}
