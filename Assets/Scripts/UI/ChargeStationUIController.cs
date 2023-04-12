using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChargeStationUIController : MonoBehaviour
{
    [SerializeField] private Image overChargedWarning;
    [SerializeField] private Transform stationLocation;
    private bool overchargeActive = false;
    private Camera _cam;

    public float fadeDuration = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        _cam = Camera.main;
        overChargedWarning.gameObject.SetActive(false);
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
    public void SetActive()
    {
        overChargedWarning.gameObject.SetActive(overchargeActive);
    }

    public void setAlwaysActive(bool overCharged, bool needsWarning)
    {
        overchargeActive = overCharged;
        SetActive();
        if (needsWarning && overchargeActive)
        {
            StartCoroutine(IconFlash(overChargedWarning));
        }
        else
        {
            StopCoroutine("IconFlash");
        }
    }

    private IEnumerator IconFlash(Image overChargedWarning)
    {
        overChargedWarning.enabled = true;
        Color color = overChargedWarning.color;

        while (true)
        {
            for (float t = 0f; t < fadeDuration; t += Time.deltaTime)
            {
                color.a = Mathf.Lerp(1, 0, t / fadeDuration);
                overChargedWarning.color = color;
                yield return null;
            }

            for (float t = 0f; t < fadeDuration; t += Time.deltaTime)
            {
                color.a = Mathf.Lerp(0, 1, t / fadeDuration);
                overChargedWarning.color = color;
                yield return null;
            }
        }
    }
}
