using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioMixer m_audioMixer = null;
    [SerializeField] private AudioClip[] m_SE_lab_Clips;
    [SerializeField] private AudioClip[] m_SE_mao_Clips;
    [SerializeField] private AudioClip[] m_BGMClips;
    private AudioSource m_SE_lab_AudioSource = null;
    private AudioSource m_SE_mao_AudioSource = null;
    private AudioSource m_voice_AuidioSoure = null;
    private AudioSource m_BGMAudioSource = null;


    // Start is called before the first frame update
    void Start()
    {
        m_SE_lab_AudioSource = gameObject.AddComponent<AudioSource>();
        m_SE_mao_AudioSource = gameObject.AddComponent<AudioSource>();
        m_voice_AuidioSoure = gameObject.AddComponent<AudioSource>();
        m_BGMAudioSource = gameObject.AddComponent<AudioSource>();

        m_SE_lab_AudioSource.outputAudioMixerGroup = m_audioMixer.FindMatchingGroups("SE_ラボ")[0];
        m_SE_mao_AudioSource.outputAudioMixerGroup = m_audioMixer.FindMatchingGroups("SE_魔王")[0];
        m_voice_AuidioSoure.outputAudioMixerGroup = m_audioMixer.FindMatchingGroups("ボイス")[0];
        m_BGMAudioSource.outputAudioMixerGroup = m_audioMixer.FindMatchingGroups("BGM")[0];
    }

    public void PlaySE_lab(int index)
    {
        if (index >= m_SE_lab_Clips.Length)
        {
            return;
        }

        m_SE_lab_AudioSource.PlayOneShot(m_SE_lab_Clips[index]);
    }

    public void PlaySE_mao(int index)
    {
        if (index >= m_SE_lab_Clips.Length)
        {
            return;
        }

        m_SE_mao_AudioSource.PlayOneShot(m_SE_mao_Clips[index]);
    }

    public void PlayBGM(int index, bool isLoop)
    {
        if(index >= m_BGMClips.Length)
        {
            return;
        }

        m_BGMAudioSource.clip = m_BGMClips[index];
        m_BGMAudioSource.loop = isLoop;
        m_BGMAudioSource.Play();
    }
}
