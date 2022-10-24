using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(StationInteractable))]
public class RepairHealth : MonoBehaviour
{
    private StationInteractable station;
    
    public float healthIncreasePerSecond = 5f;

    private void Awake()
    {
        station = GetComponent<StationInteractable>();
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
            station.Baby.uiController.SetAlwaysActive(health: true);
        }
        else
        {
            station.Baby.uiController.SetAlwaysActive(health: false);
        }
    }

    private void Update()
    {
        if (station.Baby)
        {
            station.Baby.IncreaseHealth(healthIncreasePerSecond * Time.deltaTime);
        }
    }
}
