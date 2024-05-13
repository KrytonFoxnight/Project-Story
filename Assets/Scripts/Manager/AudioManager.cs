using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private GameObject Audio_Background, Audio_Effect;

    public void Set_BGM(AudioClip audio)
    {
        AudioSource src = Audio_Background.GetComponent<AudioSource>();
        src.clip = audio;
    }

    public void Play_BGM()
    {
        Audio_Background.GetComponent<AudioSource>().Play();
    }

    public void Play_Effect(AudioClip audio)
    {
        AudioSource src = Audio_Effect.GetComponent<AudioSource>();
        src.clip = audio;
        src.Play();
    }
}
