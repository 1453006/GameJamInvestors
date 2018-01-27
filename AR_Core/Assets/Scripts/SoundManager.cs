using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {

    public static SoundManager instance;
    public AudioSource audioSourceSfx;
    public AudioSource audioSourceBgm;

    public float bgmVolume
    {
        get { return audioSourceBgm.volume; }
        set { audioSourceBgm.volume = value; }
    }
    public float sfxVolume
    {
        get { return audioSourceSfx.volume; }
        set { audioSourceSfx.volume = value; }
    }

    private bool _isEnabled = true;
    public bool isEnabled
    {
        get { return _isEnabled; }
        set
        {
            _isEnabled = value;
            if (!_isEnabled)
            {
                stopBgm();
                stopSfx();
            }
        }
    }

    Dictionary<string, AudioClip> dicAudioClips;
    public List<AudioClip> audioClips;

    void Awake()
    {
        instance = this;

        dicAudioClips = new Dictionary<string, AudioClip>();
        for (int i = 0; i < audioClips.Count; i++)
            dicAudioClips.Add(audioClips[i].name, audioClips[i]);
    }
 
    public void playSfx(string audio_clip_name)
    {
        if (isEnabled)
            audioSourceSfx.PlayOneShot(dicAudioClips[audio_clip_name], sfxVolume);
    }
  
    public void playBgm(string audio_clip_name, bool loop)
    {
        if (isEnabled)
        {
            audioSourceBgm.clip = dicAudioClips[audio_clip_name];
            audioSourceBgm.Play();
            audioSourceBgm.loop = loop;
        }
    }

    public void stopSfx()
    {
        audioSourceSfx.Stop();
    }
 
    public void stopBgm()
    {
        audioSourceBgm.Stop();
    }
}
