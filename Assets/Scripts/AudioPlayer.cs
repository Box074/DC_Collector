using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    public AudioClip dash_release;
    public AudioClip die;
    public AudioClip energy_charge;
    public AudioClip energy_release;
    public AudioClip spin_charge;
    public AudioClip spin_release;
    public AudioClip laser_charge;
    public AudioClip laser_release;
    public AudioClip fire_release;
    public AudioClip potion_drink_loop;
    public AudioClip potion_grab;
    public AudioSource audioSource;

    public void Play(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.loop = false;
        audioSource.Play();
    }
    public void PlayLoop(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();
    }
    public void Stop()
    {
        audioSource.Stop();
    }
}
