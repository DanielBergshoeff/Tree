using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundtrackManager : MonoBehaviour
{
    public static SoundtrackManager Instance;

    public AudioClip IntroSong;
    public AudioClip NormalSong;

    private AudioSource myAudioSource;

    private AudioClip currentSong;

    private bool fadeOut = false;
    private bool fadeIn = false;
    private float fadingTime = 5f;

    private float startFadeTime = 0f;
    private float maxVolume;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        myAudioSource = GetComponent<AudioSource>();
        maxVolume = myAudioSource.volume;
        StartNewSong(IntroSong);
        Invoke("StartNormalSong", 22f);
    }

    private void StartNormalSong() {
        StartNewSong(NormalSong);
    }

    // Update is called once per frame
    void Update() {
        if (fadeOut) {
            float percent = 1f - (Time.time - startFadeTime) / fadingTime;
            if (percent > 0f) {
                myAudioSource.volume = percent * maxVolume;
            }
            else {
                FadeInMusic();
            }
        }

        if (fadeIn) {
            float percent = (Time.time - startFadeTime) / fadingTime;
            if(percent < 1f) {
                myAudioSource.volume = percent * maxVolume;
            }
            else {
                myAudioSource.volume = maxVolume;
                fadeIn = false;
            }
        }
    }

    private void FadeInMusic() {
        if (currentSong != null) {
            myAudioSource.volume = 0f;
            myAudioSource.clip = currentSong;
            fadeOut = false;
            fadeIn = true;
            startFadeTime = Time.time;
            myAudioSource.Play();
            currentSong = null;
        }
    }

    public void StartNewSong(AudioClip clip) {
        currentSong = clip;
        if (myAudioSource.clip != null)
            FadeOutMusic();
        else
            FadeInMusic();
    }

    private void FadeOutMusic() {
        startFadeTime = Time.time;
        fadeOut = true;
    }
}
