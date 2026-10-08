using UnityEngine;

public class PlayMapMusicOnStart : MonoBehaviour
{
    [Tooltip("0 = Map 1, 1 = Map 2, 2 = Map 3")]
    [SerializeField] private int mapIndex;

    private void Start()
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayMapMusic(mapIndex);
        }
    }
}