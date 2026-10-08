using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio; // Required for AudioMixerGroup
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }
    
    [SerializeField] private string mainMenuSceneName = "Main Menu";

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixerGroup musicMixerGroup;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource introSource;
    [SerializeField] private AudioSource loopSource;

    [Header("Main Menu Music")]
    [SerializeField] private AudioClip mainMenuIntroClip;
    [SerializeField] private AudioClip mainMenuLoopClip;

    [Header("Map Music Loops (3 Maps)")]
    [SerializeField] private List<AudioClip> mapLoopClips;

    private int currentTrackIndex = -2; // -1 for Main Menu, 0+ for Map index

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Automatically assign mixer group to both sources on Awake
        ApplyMixerGroup();

        PlayMainMenuMusic();
    }
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Switch back to Main Menu music automatically if we return to the Main Menu scene
        if (scene.name == mainMenuSceneName)
        {
            PlayMainMenuMusic();
        }
    }

    private void ApplyMixerGroup()
    {
        if (musicMixerGroup != null)
        {
            if (introSource != null) introSource.outputAudioMixerGroup = musicMixerGroup;
            if (loopSource != null) loopSource.outputAudioMixerGroup = musicMixerGroup;
        }
        else
        {
            Debug.LogWarning("MusicMixerGroup is not assigned in MusicManager!");
        }
    }

    /// <summary>
    /// Plays the main menu music track (Intro + Loop scheduled).
    /// </summary>
    public void PlayMainMenuMusic()
    {
        if (currentTrackIndex == -1 && (introSource.isPlaying || loopSource.isPlaying)) return;

        currentTrackIndex = -1;
        
        introSource.Stop();
        loopSource.Stop();

        introSource.clip = mainMenuIntroClip;
        loopSource.clip = mainMenuLoopClip;
        loopSource.loop = true;

        double dspStartTime = AudioSettings.dspTime + 0.1;
        double introDuration = (double)mainMenuIntroClip.samples / mainMenuIntroClip.frequency;

        introSource.PlayScheduled(dspStartTime);
        loopSource.PlayScheduled(dspStartTime + introDuration);
    }

    /// <summary>
    /// Plays the looping music for a specific map index (0, 1, or 2).
    /// </summary>
    public void PlayMapMusic(int mapIndex)
    {
        if (mapIndex < 0 || mapIndex >= mapLoopClips.Count)
        {
            Debug.LogWarning($"Map index {mapIndex} out of bounds!");
            return;
        }

        if (currentTrackIndex == mapIndex && loopSource.isPlaying) return;

        currentTrackIndex = mapIndex;

        introSource.Stop();
        loopSource.Stop();

        loopSource.clip = mapLoopClips[mapIndex];
        loopSource.loop = true;
        loopSource.Play();
    }

    public void StopMusic()
    {
        introSource.Stop();
        loopSource.Stop();
        currentTrackIndex = -2;
    }
}