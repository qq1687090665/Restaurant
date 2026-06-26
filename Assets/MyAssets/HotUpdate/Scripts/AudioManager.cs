using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioClip[] audioClips;
    public AudioSource audioSource, bgAudioSource;

    /// <summary> 当前是否静音 </summary>
    public bool IsMuted { get; private set; }

    public void Play(string name)
    {
        AudioClip clip = Array.Find(audioClips, sound => sound.name == name);

        if (audioSource.clip != clip)
            audioSource.clip = clip;

        audioSource.Play();
    }

    private void Awake()
    {
        Instance = this;

        if (PlayerPrefs.HasKey("Sound"))
            SetMute(true);
    }

    /// <summary> 切换静音状态，纯音频逻辑，不管 UI </summary>
    public void ToggleMute()
    {
        SetMute(!IsMuted);
    }

    private void SetMute(bool muted)
    {
        IsMuted = muted;
        audioSource.mute = muted;
        bgAudioSource.mute = muted;

        if (muted)
            PlayerPrefs.SetString("Sound", "");
        else
            PlayerPrefs.DeleteKey("Sound");
    }
}
