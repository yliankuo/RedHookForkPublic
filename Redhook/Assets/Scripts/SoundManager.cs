using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct AudioTrack
{
    public AudioClip clip;
    public bool persistent;
    public float currentTime;
    public float trackLength;
    public int sharedTrackId;

    public bool loopable;
    //Time in track where intro part transistions to loop part
    public float loopTime;

    public AudioTrack(AudioTrack oldTrack, float newCurrentTime)
    {
        clip = oldTrack.clip;
        persistent = oldTrack.persistent;
        trackLength = oldTrack.trackLength;
        loopable = oldTrack.loopable;
        loopTime = oldTrack.loopTime;
        sharedTrackId = oldTrack.sharedTrackId;

        currentTime = newCurrentTime;
    }

}

public class SoundManager : UnitySingleton<SoundManager>
{
    [SerializeField] AudioSource musicSource;
    [SerializeField] List<AudioTrack> Tracks;

    [SerializeField] private int queuedMusicID = -1;
    [SerializeField] private int currentMusicID = -1;

    [SerializeField] private float trackTimer = 0;

    private bool transitioning = false;
    private bool switchedTracks = false;
    private float fadeOutTime = 0;
    private float fadeInTime = 0;

    private float transitionTimer = 0;
    //Keeps track of current volume, to prevent jumps in volume if FadeInMusic is called during a transition
    private float volumeCurrentMusic = 0;
    //Keeps track of maximum volume of the last played clip to be used during interpolation
    private float previousMaxVolume = 0;
    //Keeps track of the target volume for the next clip
    private float queuedMusicVolume = 0;

    private bool ValidIndex(int i)
    {
        return 0 <= i && i < Tracks.Count;
    }

    public void pauseMusic(float FadeOutTime)
    {
        FadeInMusic(-1, FadeOutTime, 0);
    }
    public void FadeInMusic(int newMusicID, float FadeOutTime, float FadeInTime, float newMusicVolume)
    {
        FadeInMusic(-1, FadeOutTime, 0);
        FadeInMusic(newMusicID, FadeInTime, newMusicVolume);

    }

    public void FadeInMusic(int newMusicID, float transitionTime, float newMusicVolume)
    {


        transitioning = true;
        switchedTracks = false;
        queuedMusicID = newMusicID;
        queuedMusicVolume = newMusicVolume;

        //Finds the amount of time to be spent on each transition(ie fade out of old music fade in of new music)
        fadeOutTime = (volumeCurrentMusic/(volumeCurrentMusic + newMusicVolume)) * transitionTime;
        fadeInTime = (newMusicVolume / (volumeCurrentMusic + newMusicVolume)) * transitionTime;

        //Stores volume of music at the time FadeInMusic is called as previous max volume
        //This is done because we want the fade out from the current volume to 0, but we 
        //don't nessisarily know that the current volume is queuedMusicVolume in the case where FadeInMusic is called when transitioning is true; 

        previousMaxVolume = volumeCurrentMusic;
        transitionTimer = 0;
    }

    private void FadeOut()
    {
        volumeCurrentMusic = ((fadeOutTime - transitionTimer) / fadeOutTime) * previousMaxVolume;
        musicSource.volume = volumeCurrentMusic;

    }

    private void FadeIn()
    {
        volumeCurrentMusic = (transitionTimer - fadeOutTime) / (fadeInTime)* queuedMusicVolume;
        musicSource.volume = volumeCurrentMusic;
    }
      
    private void Transition()
    {
        transitionTimer += Time.deltaTime;
        if (transitionTimer < fadeOutTime)
        {
            FadeOut();
        }
        else if (transitionTimer < fadeOutTime + fadeInTime)// During FadeIn
        {
            if (ValidIndex(currentMusicID) && !switchedTracks)
            {
                AudioTrack currentTrack = Tracks[currentMusicID];
                if (currentTrack.persistent)
                {
                    Tracks[currentMusicID] = new AudioTrack(currentTrack, trackTimer);

                    // Update tracks with shared timers
                    if(currentTrack.sharedTrackId > 0)
                    {
                        for(int i = 0; i < Tracks.Count; i++)
                        {
                            if(Tracks[i].sharedTrackId == currentTrack.sharedTrackId)
                            {
                               Tracks[i] = new AudioTrack(Tracks[i], trackTimer);
                            }
                        }
                    }
                }
            }
            if (ValidIndex(queuedMusicID) )
            {
                switchedTracks = true;

                AudioTrack newTrack = Tracks[queuedMusicID];
                musicSource.clip = newTrack.clip;

                if (newTrack.persistent)
                {
                    musicSource.time = newTrack.currentTime;
                    trackTimer = newTrack.currentTime;
                }
                else
                {
                    musicSource.time = 0;
                    trackTimer = 0;
                }
                musicSource.Play();
                currentMusicID = queuedMusicID;
                queuedMusicID = -1;
            }else if (!switchedTracks)
            {
                currentMusicID = -1;
                switchedTracks = true;
            }
            FadeIn();
        }
        else
        {
            fadeInTime = 0;
            fadeOutTime = 0;
            transitioning = false;
        }

    }
    // Update is called once per frame
    void Update()
    {

        if (transitioning)
        {
            Transition();
        }

        if (ValidIndex(currentMusicID)) {
            trackTimer += Time.deltaTime;
            AudioTrack currentTrack = Tracks[currentMusicID];

            if (currentTrack.loopable && trackTimer >= currentTrack.trackLength)
            {
                trackTimer = currentTrack.loopTime;
                musicSource.time = currentTrack.loopTime;
            }

        }

    }
}
