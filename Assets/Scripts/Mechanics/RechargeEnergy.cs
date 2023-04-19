using System;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(StationInteractable))]
[RequireComponent(typeof(BabyStationAudioPlayer))]
public class RechargeEnergy : MonoBehaviour
{
    // how long it will take to recharge to full
    public float incrementAmount = 5f;
    public float decreaseHealthOverCharge = 10f;
    public GameObject overchargeEffect;
    public GameObject overchargeEffectInner;
    // Start is called before the first frame update
    private StationInteractable station;
    private static readonly int Recharge = Animator.StringToHash("Recharge");
    public ChargeStationUIController uiController;

    private void Awake()
    {
        station = GetComponent<StationInteractable>();
        
        var audioPlayer = GetComponent<BabyStationAudioPlayer>();
        station.HandlePlaceEvent += audioPlayer.HandleBabyPlaced;
        audioPlayer.shouldStartAudio = baby => baby.currentEnergy < baby.energy - 1;
        audioPlayer.shouldEndAudio = baby => baby.currentEnergy >= baby.energy - 1;
    }

    private void OnEnable()
    {
        station.HandlePlaceEvent += onPlaceEvent;
    }

    private void OnDisable()
    {
        station.HandlePlaceEvent -= onPlaceEvent;
    }

    private void onPlaceEvent(bool placeInStation)
    {
        if (placeInStation)
        {
            station.Baby.uiController.SetAlwaysActive(energy: true);
            station.Baby.anim.SetBool(Recharge, true);
        }
        else
        {
            station.Baby.uiController.SetAlwaysActive(energy: false);
            station.Baby.anim.SetBool(Recharge, false);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (station.Baby)
        {
            if (station.Baby.uiController.getHealthBar() <= 0.75f)
            {
                uiController.SetAlwaysActive(station.Baby.GetOverCharged(), station.Baby.uiController.getHealthBar() <= 0.5f);
            }
            if (station.Baby.GetOverCharged()) 
            {
                station.Baby.DecreaseHealth(decreaseHealthOverCharge * Time.deltaTime);
                station.Baby.uiController.SetAlwaysActive(health: true);
                if (overchargeEffectInner == null)
                {
                    overchargeEffectInner = Instantiate(overchargeEffect, transform.position + Vector3.up, Quaternion.identity);
                }
            }
            else
            {
                station.Baby.uiController.SetAlwaysActive(health: false);
                station.Baby.IncreaseEnergy(incrementAmount * Time.deltaTime);
                if (overchargeEffectInner != null)
                {
                    Destroy(overchargeEffectInner);
                    overchargeEffectInner = null;
                }
                
            }
        }
        else
        {
            if (overchargeEffectInner != null)
            {
                Destroy(overchargeEffectInner);
                overchargeEffectInner = null;
            }
            uiController.SetAlwaysActive(false, false);
        }
    }
}