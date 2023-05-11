using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    /*
    public AudioClip withBeats;
    public AudioClip noBeats;
    public AudioClip onlyBeats;
    */

    public AudioClip backing;
    public AudioClip beats;

    public float maxVolume = 0.5f;
    public float fadeTime = 1.0f;

    private PlayerStateManager playerStateManager;

    private AudioSource[] musicSources;
    private AudioSource backingSource;
    private AudioSource beatsSource;

    // Start is called before the first frame update
    void Start()
    {
        playerStateManager = GameObject.Find("PlayerStateManager").GetComponent<PlayerStateManager>();
        musicSources = GetComponents<AudioSource>();
        backingSource = musicSources[0];
        beatsSource = musicSources[1];
        /*
        musicSources[0].clip = withBeats;
        musicSources[0].volume = 0;

        musicSources[1].clip = noBeats;
        musicSources[1].volume = maxVolume;

        musicSources[0].Play();
        musicSources[1].Play();
        */

        backingSource.clip = backing;
        backingSource.volume = maxVolume;

        beatsSource.clip = beats;
        beatsSource.volume = 0;

        backingSource.Play();
        beatsSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerStateManager.modeIs3D)
        {
            // beatsSource.volume = Fade(beatsSource.volume, 0);
            beatsSource.volume = 0;
        }
        else
        {
            // beatsSource.volume = Fade(beatsSource.volume, maxVolume);
            beatsSource.volume = maxVolume;
        }
    }

    /*
    private float Fade(float currentVolume, float endVolume)
    {
        if (currentVolume < endVolume)
        {
            return currentVolume + (currentVolume * Time.deltaTime / fadeTime);
        }
        else if (currentVolume > endVolume)
        {
            return currentVolume - (currentVolume * Time.deltaTime / fadeTime);
        }
        else return currentVolume;
    }
    */
}
