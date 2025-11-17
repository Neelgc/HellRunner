using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    Resolution[] resolutions;

    public TMP_Dropdown resolutionDropdown;

    public Slider soundSlider;

    private void Start()
    {
        // Récupère toutes les résolutions supportées
        resolutions = Screen.resolutions;

        // Crée une liste sans doublons (width + height uniques)
        List<string> options = new List<string>();
        List<Resolution> uniqueResolutions = new List<Resolution>();

        foreach (Resolution res in resolutions)
        {
            string option = res.width + " x " + res.height;

            if (!options.Contains(option))
            {
                options.Add(option);
                uniqueResolutions.Add(res);
            }
        }

        // On remplace la liste originale par la version filtrée
        resolutions = uniqueResolutions.ToArray();

        // Vide et recharge le dropdown
        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);

        // Sélectionne la résolution actuelle
        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
                break;
            }
        }

        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        // Volume initial
        soundSlider.value = AudioManager.Instance.GloablSFXVolume;
    }


    public void SetVolume(float volume)
    {
        AudioManager.Instance.GloablSFXVolume = volume;
    }

    public void SetFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
}
