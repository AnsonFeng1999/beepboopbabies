using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AK.Wwise;

public class TimerClock : MonoBehaviour
{
    public float timeleft;
    public bool timeon = false;
    public Text TimerTxt;
    public AudioSource CD;//Audio for countdown
    public bool Noon;
    public bool EndTime;
    //wwise
    public AK.Wwise.Event playBeepBoopSpaceship;

    void Start()
    {
        playBeepBoopSpaceship.Post(gameObject);
        AkSoundEngine.SetState("Hurry", "Relaxed");
        timeon = true;
        //TimerTxt.text = "8:00";
    }

    // Update is called once per frame
    void Update()
    {
        if (timeon)
        {
            if (timeleft > 0)
            {
                timeleft += Time.deltaTime * 4;
                updateTimer(timeleft);
            }
            else
            {
                Debug.Log("time is up");
                timeleft = 0;
                timeon = false;
            }
        }
    }
    bool isPlay = false;
    void updateTimer(float CurrentTime)
    {
        CurrentTime += 1;
        float min = Mathf.FloorToInt(CurrentTime / 60);
        float sec = Mathf.FloorToInt(CurrentTime % 60);

        if (min + 6 < 12)
        {
            TimerTxt.text = string.Format("{0:00} : {1:00}", min + 6, sec) + " AM";
        }
        else
        {
            TimerTxt.text = string.Format("{0:00} : {1:00}", min + 6, sec) + " PM";
            //Play Camera Shake
            if (min + 6 - 12 >= 4 && isPlay == false)
            {
                Debug.Log("Play Camera Shake");
                isPlay = true;
                //EZCameraShake.CameraShaker.Instance.ShakeOnce(4f, 4f, 0.1f, 1f); //disabled camera shake at 2mins
            } 
            
            
            //turning 12:00
            if (!Noon && CurrentTime >= 360)
            {
                AkSoundEngine.SetRTPCValue("isTwelveOclock", 1);
                Noon = true;
            }
            //35 seconds left (left time for the trigger)
            if (!EndTime && CurrentTime >= 560)
            {
                AkSoundEngine.SetState("Hurry", "Hurry");
                EndTime = true;
            }
        }
        if (min + 6 - 12 >= 5 && sec == 40.0f)
        {
            Debug.Log("Play CD");
            if (CD.isPlaying == false)
                CD.Play();
        }
    }
}
