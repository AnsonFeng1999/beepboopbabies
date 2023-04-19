using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    // Start is called before the first frame update
    public static bool GameIsPaused;

    public GameObject pauseMenuUI;
    public GameObject initialButton;
    

    public void TogglePause()
    {
        if (ScoreManager.Instance.IsGameOver) return;
        
        if (GameIsPaused)
            Resume();
        else
            Pause();
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
    }
    public void LoadGame()
    {
        Time.timeScale = 1f;
        LevelsManager.Instance.LoadLevelScene();
    }

    public void NextLevel()
    {
        LevelsManager.Instance.Level++;
        if (LevelsManager.Instance.Level == 1)
        {
            // set level based on player count
            switch (LevelsManager.Instance.numberOfPlayers)
            {
                case 2:
                    LevelsManager.Instance.Level = 3;
                    break;
                case 3:    
                    LevelsManager.Instance.Level = 4;
                    break;
                case 4:
                    LevelsManager.Instance.Level = 4;
                    break;
            }
            LevelsManager.Instance.CurrentMap = LevelsManager.Map.Classic;
        }
        else
        {
            LevelsManager.Instance.CurrentMap = LevelsManager.Instance.CurrentMap == LevelsManager.Map.Classic
                ? LevelsManager.Map.Spinning
                : LevelsManager.Map.Classic;
        }
        LoadGame();
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        initialButton.GetComponent<Button>().Select();
        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    public void QuitGame()
    {
        Debug.Log("quit");
        Application.Quit();
    }
}