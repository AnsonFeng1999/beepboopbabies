using System;
using System.Collections;
using UnityEngine;
using System.Linq;

public delegate void TimeUpdate(int time);
public delegate void GameOverHandler();

public class ScoreManager : MonoBehaviour
{
    public GameObject Image;//Clock UI
    public AudioClip audioClip;//Audio for Clock
    public AudioSource AudioSource;//Audio for countdown
    private float totalStars;
    private int currentParents;

    public static ScoreManager Instance
    {
        get; private set;
    }

    public float CurrentTime { get; private set; } = 150f; //set total game length
    private float ParentReturnTime = 40f; //set parent return time
    public float AllTime { get; private set; }

    public bool IsGameOver { get; private set; }

    // Final score, which is the average of "stars" of each parent
    public float FinalScore => RoundToHalf(totalStars / FindObjectOfType<ParentSpawnManager>().NumberOfParents);

    private void Awake()
    {
        Instance = this;
        AllTime = CurrentTime;
    }
    
    private void Update()
    {
        UpdateTime();
        bool lessthanParentReturnTime = CurrentTime < ParentReturnTime;
        Debug.Log("current time: " + lessthanParentReturnTime + " Current Time: " + CurrentTime + " Parent Return Time:" + ParentReturnTime);
        if (CurrentTime < ParentReturnTime)
        {
            // make each parent return to their kid
            foreach (var parent in ParentSpawnManager.Instance.parentStates)
            {
                parent.returnToKids = true;
            }
        }
        if (CurrentTime < 0 && !IsGameOver)
        {
            GameObject.Find("babis level music").GetComponent<AudioSource>().enabled = false;
            //PlayerPrefs.SetFloat("music", 0);
            StartCoroutine(EndGame());
        }
    }

    private IEnumerator EndGame()
    {
        Time.timeScale = 0f;
        Image.SetActive(true);
        Image.GetComponent<AudioSource>().PlayOneShot(audioClip);
        IsGameOver = true;

        yield return new WaitForSecondsRealtime(2);

        Image.gameObject.SetActive(false);
        transform.gameObject.SetActive(false);
        HandleGameOver?.Invoke();
    }

    public event TimeUpdate NotifyTimeUpdate;
    public event GameOverHandler HandleGameOver;

    private void UpdateTime()
    {
        CurrentTime -= Time.deltaTime;
        NotifyTimeUpdate?.Invoke((int)CurrentTime);
    }

    public void RegisterPickedUpBaby(BabyState babyState)
    {
        var stars = new[]
            {
                (babyState.currentEnergy, babyState.energy),
                (babyState.currentHealth, babyState.health),
                (babyState.currentDiaper, babyState.diaper),
                (babyState.currentFun, babyState.fun),
                (babyState.currentOil, babyState.oil),
            }
            .Select(tuple => tuple.Item1 / tuple.Item2)
            .Select(RoundToHalf)
            .Sum();
        var numBodyPartsMissing = (babyState.health - babyState.healthcap) / 20;
        stars = Mathf.Max(stars - numBodyPartsMissing, 0);
        
        StarsPanel.Instance.ShowStars(stars);
        totalStars += stars;
        currentParents++;

        if (currentParents == FindObjectOfType<ParentSpawnManager>().NumberOfParents)
        {
            StartCoroutine(EndGame());
        }
    }

    private float RoundToHalf(float n)
    {
        return Mathf.Round(n * 2 + 0.01f) / 2;
    }
}