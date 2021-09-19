﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    private static AudioPlayer _instance = null;
    public static AudioPlayer Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = FindObjectOfType<AudioPlayer>();
            }

            return _instance;
        }
    }

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private List<AudioClip> _audioClip;

    public void PlaySFX(string name)
    {
        AudioClip sfx = _audioClip.Find(s => s.name == name);
        if(sfx == null)
        {
            return;
        }

        _audioSource.PlayOneShot(sfx);
    }

    public void StopBGM(string name)
    {
        AudioClip bgm =_audioClip.Find(m => m.name == name);
        if(bgm == null)
        {
            return;
        }

        _audioSource.Stop();
    }

}//class
