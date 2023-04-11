using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChargeStationUIController : MonoBehaviour
{
    [SerializeField] private Image overchargeBarSprite;
    [SerializeField] private GameObject overchargeBar;
    [SerializeField] private Transform stationLocation;
    [SerializeField] private float height = 3f;
    private bool? overchargeActive = false;
    private Camera _cam;
    // Start is called before the first frame update
    void Start()
    {
        _cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        // make the UI face towards the camera
        if (_cam != null)
        {
            var trans = transform;
            trans.rotation = Quaternion.LookRotation(trans.position - _cam.transform.position);
            var eulerAngles = trans.eulerAngles;
            eulerAngles.y = 0;
            trans.eulerAngles = eulerAngles;
        }
    }
    public void SetActive ()
    {
        overchargeBar.SetActive(overchargeActive.GetValueOrDefault(false));
    }

    public void UpdateOverChargeBar (float amount)
    {
        float actualAmount = Mathf.Clamp(amount, 0, 1);
        overchargeBarSprite.fillAmount = 1 - actualAmount;
    }

    public void setAlwaysActive (bool? overCharged = null) 
    {
        overchargeActive = overCharged.GetValueOrDefault(overchargeActive.GetValueOrDefault(false));
        SetActive();
    }
}
