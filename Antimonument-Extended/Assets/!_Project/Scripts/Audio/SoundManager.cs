using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("Sound Effects")]
    [SerializeField] private AudioClip[] grabSounds;
    [SerializeField] private AudioClip[] grabStatueSounds;
    [SerializeField] private AudioClip[] puttingDownPedestalSounds;
    [SerializeField] private AudioClip[] statueDebrisImpactSounds;
    [SerializeField] private AudioClip[] stickerSlapSounds;
    [SerializeField] private AudioClip[] swordCutSounds;
    [SerializeField] private AudioClip[] typewriterSounds;
    [SerializeField] private AudioClip[] uiSelectSounds;
    [SerializeField] private AudioClip spraySound;
    [SerializeField] private AudioClip posterStabSound;

    [Header("Background Sound")]
    [SerializeField] private AudioClip lionBackground;
    [SerializeField] private AudioClip junglingBackground;
    [SerializeField] private AudioClip meerschneckeBackground;
    [SerializeField] private AudioClip lionExplain;
    [SerializeField] private AudioClip junglingExplain;
    [SerializeField] private AudioClip meerschneckeExplain;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sprayAudioSource;
    [SerializeField] private AudioSource stickerAudioSource;
    [SerializeField] private AudioSource swordCutAudioSource;
    [SerializeField] private AudioSource uiSelectAudioSource;
    [SerializeField] private AudioSource grabAudioSource;
    [SerializeField] private AudioSource backgroundAudioSource;
    [SerializeField] private AudioSource explainAudioSource;
    

    int stickerCount, grabStatueCount, putStatueDownCount, statueDebrisImpactCount, swordCutCount, typewriterCount, uiSelectCount, grabCount;

    public void PlayGrabSound()
    {
        grabAudioSource.clip = grabSounds[grabCount];
        grabAudioSource.Play();
        grabCount++;
        if (grabCount > grabCount.ToString().Length - 1)
            grabCount = 0;
    }

    public void PlaySpraySound(bool on)
    {
        if(on && !sprayAudioSource.isPlaying)
            sprayAudioSource.Play();
        else
            sprayAudioSource.Stop();
    }

    public void PlayPosterSound(AudioSource audioSource)
    {
        audioSource.clip = posterStabSound;
        audioSource.Play();
    }

    public void PlayStickerSound()
    {
        stickerAudioSource.clip = stickerSlapSounds[stickerCount];
        stickerAudioSource.Play();
        stickerCount++;
        if (stickerCount > stickerSlapSounds.Length - 1)
            stickerCount = 0;
    }

    public void PlayGrabStatueSound(AudioSource audioSource)
    {
        audioSource.clip = grabStatueSounds[grabStatueCount];
        audioSource.Play();
        grabStatueCount++;
        if (grabStatueCount > grabStatueSounds.Length - 1)
            grabStatueCount = 0;
    }

    public void PlayPutStatueDownSound(AudioSource audioSource)
    {
        audioSource.clip = puttingDownPedestalSounds[putStatueDownCount];
        audioSource.Play();
        putStatueDownCount++;
        if (putStatueDownCount > puttingDownPedestalSounds.Length - 1)
            putStatueDownCount = 0;
    }

    public void PlayStatueDebrisImpactSound(AudioSource audioSource)
    {
        audioSource.clip = statueDebrisImpactSounds[statueDebrisImpactCount];
        audioSource.Play();
        statueDebrisImpactCount++;
        if (statueDebrisImpactCount > statueDebrisImpactSounds.Length - 1)
            statueDebrisImpactCount = 0;
    }

    public void PlaySwordCutSound(bool on)
    {
        swordCutAudioSource.clip = swordCutSounds[swordCutCount];
        if (on)
        {
            swordCutAudioSource.Play();
            swordCutCount++;
            if (swordCutCount > swordCutSounds.Length - 1)
                swordCutCount = 0;
        }
        else
            swordCutAudioSource.Stop();
    }

    public void PlayTypewriterSound(AudioSource audioSource)
    {
        audioSource.clip = typewriterSounds[typewriterCount];
        audioSource.Play();
        typewriterCount++;
        if (typewriterCount > typewriterSounds.Length - 1)
            typewriterCount = 0;
    }

    public void PlayUISelectSound()
    {
        uiSelectAudioSource.clip = uiSelectSounds[uiSelectCount];
        uiSelectAudioSource.Play();
        uiSelectCount++;
        if (uiSelectCount > uiSelectSounds.Length - 1)
            uiSelectCount = 0;
    }

    public void PlayLionBackground()
    {
        backgroundAudioSource.clip = lionBackground;
        backgroundAudioSource.Play();
        explainAudioSource.clip = lionExplain;
        explainAudioSource.Play();
    }

    public void PlayJunglingBackground()
    {
        backgroundAudioSource.clip = junglingBackground;
        backgroundAudioSource.Play();
        explainAudioSource.clip = junglingExplain;
        explainAudioSource.Play();
    }

    public void PlayMeerschneckeBackground()
    {
        backgroundAudioSource.clip = meerschneckeBackground;
        backgroundAudioSource.Play();
        explainAudioSource.clip = meerschneckeExplain;
        explainAudioSource.Play();
    }

    public void PlayStatueSound(AudioClip backgroundClip, AudioClip explanationClip)
    {
        if(backgroundClip == null)
            Debug.LogError("BackgroundClip is currently null");
        backgroundAudioSource.clip = backgroundClip;
        backgroundAudioSource.Play();
        
        if(explanationClip == null)
            Debug.LogError("BackgroundClip is currently null");
        explainAudioSource.clip = explanationClip;
        explainAudioSource.Play();
    }

    public void StopSound()
    {
        backgroundAudioSource.Stop();
        explainAudioSource.Stop();
    }
}
