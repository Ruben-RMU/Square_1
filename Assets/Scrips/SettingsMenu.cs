using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsMenu : MonoBehaviour
{
    public GameObject settingsPanel;
    public AudioMixer mainMixer;
    public Slider musicSlider;

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }
    
    public void SetMusicVolume(float sliderLevel)
    {
        mainMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Clamp(sliderLevel, 0.0001f, 1f)) * 20);
    }
}
