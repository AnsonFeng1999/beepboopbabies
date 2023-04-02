using System;
using UnityEngine;
using Random = System.Random;

public class OilPuddle : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("enter!" + other.name);
        var baby = other.GetComponent<BabyPickUpInteractable>();
        if (baby && !baby.isPickedUp)
        {
            var velo = baby.transform.forward;
            velo.y = 0;
            velo.Normalize();
            velo.y = 1;
            
            baby.Kick(velo * 3);
            baby.GetComponent<Rigidbody>().angularVelocity = velo * 10;
            return;
        }

        var player = other.GetComponent<PlayerController>();
        if (player)
        {
            player.moveSpeedMultiplier = 0.5f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var player = other.GetComponent<PlayerController>();
        if (player)
        {
            player.moveSpeedMultiplier = 1;
        }
    }
}