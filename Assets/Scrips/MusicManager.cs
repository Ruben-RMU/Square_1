using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource introSource;
    [SerializeField] private AudioSource loopSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip introClip;
    [SerializeField] private AudioClip mainLoopClip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        PlayMusicWithIntro();
    }

    private void PlayMusicWithIntro()
    {
        introSource.clip = introClip;
        loopSource.clip = mainLoopClip;
        loopSource.loop = true;
        double dspStartTime = AudioSettings.dspTime + 0.1; // Small buffer for scheduling
        double introDuration = (double)introClip.samples / introClip.frequency;
        introSource.PlayScheduled(dspStartTime);
        loopSource.PlayScheduled(dspStartTime + introDuration);
    }
}