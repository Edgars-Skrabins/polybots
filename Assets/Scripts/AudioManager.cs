using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;

    private float volume;
    [SerializeField] private GameObject sound;

    [SerializeField] private Slider slider;

    private bool hasUpdated;

    private void Start()
    {
        if (sound != null || !PlayerMenuManager.I.isGamePaused || hasUpdated) return;
        Initialize();
    }

    private void Initialize()
    {
        audioMixer.GetFloat("SFXVolume", out volume);
        slider.value = volume;
        hasUpdated = true;
    }

    private void Update()
    {
        if (slider.value <= 0.001)
        {
            audioMixer.SetFloat("SFXVolume", -80);
        }
        else
        {
            volume = slider.value;
        }
    }

    public void SetVolume()
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
    }

    public void TestVolume()
    {
        Instantiate(sound, transform.position, Quaternion.identity);
    }
}
