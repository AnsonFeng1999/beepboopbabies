using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelsManager
{
    public static LevelsManager Instance = new();
    public int numberOfPlayers = 1;
    public bool ContinueGame = false;
    
    public enum Map
    {
        Classic = 0,
        Spinning
    }

    public Map CurrentMap
    {
        get => (Map) PlayerPrefs.GetInt("CurrentMap", 0);
        set => PlayerPrefs.SetInt("CurrentMap", (int)value);
        
    }

    private readonly Dictionary<Map, string> mapToScene = new()
    {
        { Map.Classic, "Scene1" },
        { Map.Spinning, "Scene2" }
    };

    public int Level { get; set; }

    public int UnlockedLevel
    {
        get => PlayerPrefs.GetInt("UnlockedLevel", 0);
        set => PlayerPrefs.SetInt("UnlockedLevel", value);
    }

    public bool IsTutorial => Level == 0;

    private LevelsManager()
    {
        Level = UnlockedLevel;
    }
    
    public void LoadLevelScene()
    {
        if (!ContinueGame)
        {
            SceneManager.LoadScene("Tutorial");
        }
        else
        {
            SceneManager.LoadScene(mapToScene[CurrentMap]);
        }
    }
}
