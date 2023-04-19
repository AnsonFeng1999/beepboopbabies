using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu2 : MonoBehaviour
{
    [SerializeField]
    private Slider SFX, MUSIC;
    [SerializeField]
    private Dropdown levelDropdown;
    [SerializeField]
    private Dropdown mapDropdown;

    private void Start()
    {
        SFX.value = PlayerPrefs.GetFloat("sfx", 0.5f);
        MUSIC.value = PlayerPrefs.GetFloat("music", 0.5f);
        mapDropdown.options = Enum.GetValues(typeof(LevelsManager.Map)).Cast<LevelsManager.Map>()
            .Select(map => new Dropdown.OptionData(map.ToString()))
            .ToList();
        mapDropdown.value = (int) LevelsManager.Instance.CurrentMap;
    }

    private void Update()
    {
        LevelsManager.Instance.Level = levelDropdown.value;
        LevelsManager.Instance.CurrentMap = (LevelsManager.Map) mapDropdown.value;
        
        PlayerPrefs.SetFloat("sfx", SFX.value);
        PlayerPrefs.SetFloat("music", MUSIC.value);
        var quitKey = KeyCode.Escape;
        var nextKey = KeyCode.Space;

        if (Input.GetKeyDown(quitKey))
        {
            Application.Quit();
        }
        
        if (Input.GetKeyDown(nextKey))
        {
            SceneManager.LoadScene("Select");
        }
    }

    public void PlayGame()
    {
        LevelsManager.Instance.CurrentMap = LevelsManager.Map.Classic;
        LevelsManager.Instance.ContinueGame = false;
        LevelsManager.Instance.Level = 0;
        SceneManager.LoadScene("Select");
    }
    public void ContineGame()
    {
        LevelsManager.Instance.Level = levelDropdown.value;
        LevelsManager.Instance.ContinueGame = true;
        LevelsManager.Instance.CurrentMap = (LevelsManager.Map) mapDropdown.value;
        SceneManager.LoadScene("Select");
    }


    public void QuitGame()
    {
        Application.Quit();
    }
}