using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFX : Singleton<SFX>
{

    public AudioSource audioSource;

    public SFXData settings;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();

        settings = Resources.Load<SFXData>("SFXSettings");
    }

    public void PlayOneShot(SFXData.Type type)
    {
        PlayOneShot(settings.GetClip(type));
    }

    public void PlayOneShot(AudioClip audioClip)
    {
        if (!audioClip)
            return;

        audioSource.PlayOneShot(audioClip);
    }
}
