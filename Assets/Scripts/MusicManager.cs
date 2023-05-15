using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioClip backing;
    public AudioClip beats;

    public float maxVolume = 0.5f;
    public float musicFadeTime = 0.5f;

    private PlayerStateManager playerStateManager;

    private AudioSource[] musicSources;
    private AudioSource backingSource;
    private AudioSource beatsSource;

    // Start is called before the first frame update
    void Start()
    {
        playerStateManager = GameObject.Find("PlayerStateManager").GetComponent<PlayerStateManager>();

        // Get audio sources for background music
        musicSources = GetComponents<AudioSource>();
        backingSource = musicSources[0];
        beatsSource = musicSources[1];

        // Set backround music
        backingSource.clip = backing;
        beatsSource.clip = beats;

        // Set volumes for music backing and beats
        backingSource.volume = maxVolume;
        beatsSource.volume = 0;

        // Play both backing and beats at the same time
        backingSource.Play();
        beatsSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerStateManager.modeIs3D)
        {
            // Have the beats fade out while the player is in 3D mode
            beatsSource.volume = Fade(beatsSource.volume, 0, musicFadeTime);
        }
        else
        {
            // Have the beats fade in while the player is in 2D mode
            beatsSource.volume = Fade(beatsSource.volume, maxVolume, musicFadeTime);
        }
    }

    // Have one value slowly approach another over a certain amount of time
    private float Fade(float currentValue, float endValue, float fadeTime)
    {
        if (currentValue < endValue)
        {
            if (currentValue + (maxVolume * Time.deltaTime / fadeTime) < endValue)
            {
                return currentValue + (maxVolume * Time.deltaTime / fadeTime);
            }
            else
            {
                return endValue;
            }
        }
        else if (currentValue > endValue)
        {
            if (currentValue - (maxVolume * Time.deltaTime / fadeTime) > endValue)
            {
                return currentValue - (maxVolume * Time.deltaTime / fadeTime);
            }
            else
            {
                return endValue;
            }
        }
        else return currentValue;
    }
}
