#region Namespace
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using PlayerFunctionality;
using Photon.Realtime;
using TMPro;
#endregion

/// <summary>
/// Handles local player settings like volume and snesitivity
/// </summary>
public class LocalPlayerSettings : MonoBehaviour
{
    #region Attributes
    [Header("References")]
    [Tooltip("Slider reference for Master Volume.")]
    public Slider masterVolumeSlider;
    [Tooltip("Slider reference for Music Volume.")]
    public Slider musicVolumeSlider;
    [Tooltip("Slider reference for SFX Volume.")]
    public Slider sfxVolumeSlider;
    [Tooltip("Slider reference for Sensitivity.")]
    public Slider sensitivitySlider;

    [Header("Max Settings")]
    [Tooltip("Max Master Volume Value")]
    public float maxMasterVolume = 100f;
    [Tooltip("Max Msuic Volume Value")]
    public float maxMusicVolume = 100f;
    [Tooltip("Max SFX Volume Value")]
    public float maxSfxVolume = 100f;
    [Tooltip("Max Sensitivity Value")]
    public float maxSensitivity = 2.0f;

    [Header("Default Settings")]
    [Tooltip("Default Master Volume Value")]
    public float defaultMasterVolume = 100f;
    [Tooltip("Default Msuic Volume Value")]
    public float defaultMusicVolume = 100f;
    [Tooltip("Default SFX Volume Value")]
    public float defaultSfxVolume = 100f;
    [Tooltip("Default Sensitivity Value")]
    public float defaultSensitivity = 1.0f;

    [Header("Current Settings")]
    [Tooltip("Master Volume Value")]
    public float masterVolume = 100f;
    [Tooltip("Msuic Volume Value")]
    public float musicVolume = 100f;
    [Tooltip("SFX Volume Value")]
    public float sfxVolume = 100f;
    [Tooltip("Sensitivity Value")]
    public float sensitivity = 1.0f;


    #endregion

    #region Methods

    private void Start()
    {
        masterVolumeSlider.maxValue = maxMasterVolume;
        musicVolumeSlider.maxValue = maxMusicVolume;
        sfxVolumeSlider.maxValue = maxSfxVolume;
        sensitivitySlider.maxValue = maxSensitivity;
        SetSavedSettings();
    }


    /// <summary>
    /// Sets the volume setting for the specific volume type
    /// </summary>
    /// <param name="whichVolume"></param> the volume setting to adjust
    public void SetVolume(string whichVolume)
    {
        switch (whichVolume)
        {
            case "Master":
                masterVolume = masterVolumeSlider.value;
                PlayerPrefs.SetFloat("Master Volume", masterVolume);
                if (masterVolume == 0)
                    PlayerPrefs.SetInt("Master Zero", 1);
                else
                    PlayerPrefs.SetInt("Master Zero", 0);
                AkSoundEngine.SetRTPCValue("MasterVolume", masterVolume);
                masterVolumeSlider.gameObject.transform.GetChild(masterVolumeSlider.gameObject.transform.childCount - 1).gameObject.GetComponent<TMP_Text>().text = Mathf.RoundToInt(masterVolumeSlider.value).ToString();
                break;
            case "Music":
                musicVolume = musicVolumeSlider.value;
                PlayerPrefs.SetFloat("Music Volume", musicVolume);
                if (musicVolume == 0)
                    PlayerPrefs.SetInt("Music Zero", 1);
                else
                    PlayerPrefs.SetInt("Music Zero", 0);
                AkSoundEngine.SetRTPCValue("MusicVolume", musicVolume);
                musicVolumeSlider.gameObject.transform.GetChild(musicVolumeSlider.gameObject.transform.childCount - 1).gameObject.GetComponent<TMP_Text>().text = Mathf.RoundToInt(musicVolumeSlider.value).ToString();
                break;
            case "SFX":
                sfxVolume = sfxVolumeSlider.value;
                PlayerPrefs.SetFloat("SFX Volume", sfxVolume);
                if (sfxVolume == 0)
                    PlayerPrefs.SetInt("SFX Zero", 1);
                else
                    PlayerPrefs.SetInt("SFX Zero", 0);
                AkSoundEngine.SetRTPCValue("SfxVolume", sfxVolume);
                sfxVolumeSlider.gameObject.transform.GetChild(sfxVolumeSlider.gameObject.transform.childCount - 1).gameObject.GetComponent<TMP_Text>().text = Mathf.RoundToInt(sfxVolumeSlider.value).ToString();
                break;
            default:
                Debug.Log("Invalid volume type.");
                break;
        }
    }

    /// <summary>
    /// Function to set the sensitivity of the game
    /// </summary>
    public void SetSensitivity()
    {
        sensitivity = sensitivitySlider.value;
        PlayerPrefs.SetFloat("Sensitivity", sensitivity);
        sensitivitySlider.gameObject.transform.GetChild(sensitivitySlider.gameObject.transform.childCount - 1).gameObject.GetComponent<TMP_Text>().text = sensitivitySlider.value.ToString("F2");
    }

    /// <summary>
    /// Retrieves saved player pref settings and sets them, or sets default if not found
    /// </summary>
    private void SetSavedSettings()
    {
        // Master Volume
        if (PlayerPrefs.GetInt("Master Zero") == 1)
            masterVolumeSlider.value = PlayerPrefs.GetFloat("Master Volume");
        else if (PlayerPrefs.GetFloat("Master Volume") == 0f)
            masterVolumeSlider.value = defaultMasterVolume;
        else
            masterVolumeSlider.value = PlayerPrefs.GetFloat("Master Volume");
        masterVolumeSlider.gameObject.transform.GetChild(masterVolumeSlider.gameObject.transform.childCount - 1).gameObject.GetComponent<TMP_Text>().text = Mathf.RoundToInt(masterVolumeSlider.value).ToString();

        // Music Volume
        if (PlayerPrefs.GetInt("Music Zero") == 1)
            musicVolumeSlider.value = PlayerPrefs.GetFloat("Music Volume");
        else if (PlayerPrefs.GetFloat("Music Volume") == 0f)
            musicVolumeSlider.value = defaultMusicVolume;
        else
            musicVolumeSlider.value = PlayerPrefs.GetFloat("Music Volume");
        musicVolumeSlider.gameObject.transform.GetChild(musicVolumeSlider.gameObject.transform.childCount - 1).gameObject.GetComponent<TMP_Text>().text = Mathf.RoundToInt(musicVolumeSlider.value).ToString();

        // SFX  Volume
        if (PlayerPrefs.GetInt("SFX Zero") == 1)
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFX Volume");
        else if (PlayerPrefs.GetFloat("SFX Volume") == 0f)
            sfxVolumeSlider.value = defaultSfxVolume;
        else
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFX Volume");
        sfxVolumeSlider.gameObject.transform.GetChild(sfxVolumeSlider.gameObject.transform.childCount - 1).gameObject.GetComponent<TMP_Text>().text = Mathf.RoundToInt(sfxVolumeSlider.value).ToString();

        if (PlayerPrefs.GetFloat("Sensitivity") == 0f)
        {
            sensitivitySlider.value = defaultSensitivity;
            sensitivity = defaultSensitivity;
        }
        else 
        {
            sensitivitySlider.value = PlayerPrefs.GetFloat("Sensitivity");
            sensitivity = sensitivitySlider.value;
        }
        if (sensitivitySlider.value == 0)
        {
            sensitivitySlider.value = defaultSensitivity;
            sensitivity = defaultSensitivity;
        }
        PlayerPrefs.SetFloat("Sensitivity", sensitivity);
        sensitivitySlider.gameObject.transform.GetChild(sensitivitySlider.gameObject.transform.childCount - 1).gameObject.GetComponent<TMP_Text>().text = sensitivitySlider.value.ToString("F2");

    }

    /// <summary>
    /// Set the defult values for all settings
    /// </summary>
    public void SetDefaultSettings()
    {
        masterVolume = defaultMasterVolume;
        masterVolumeSlider.value = defaultMasterVolume;
        PlayerPrefs.SetFloat("Master Volume", defaultMasterVolume);

        musicVolume = defaultMusicVolume;
        musicVolumeSlider.value = defaultMusicVolume;
        PlayerPrefs.SetFloat("Music Volume", defaultMusicVolume);

        sfxVolume = defaultSfxVolume;
        sfxVolumeSlider.value = defaultSfxVolume;
        PlayerPrefs.SetFloat("SFX Volume", defaultSfxVolume);

        sensitivity = defaultSensitivity;
        sensitivitySlider.value = defaultSensitivity;
        PlayerPrefs.SetFloat("Sensitivity", defaultSensitivity);
    }

    #endregion
}
