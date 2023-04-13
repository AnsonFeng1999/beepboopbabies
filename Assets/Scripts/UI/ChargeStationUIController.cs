using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChargeStationUIController : MonoBehaviour
{
    [SerializeField] private Image overChargedWarning;
    [SerializeField] private Transform stationLocation;
    [SerializeField] private float height = 7f;
    [SerializeField] private float width = -0.25f;
    private bool overchargeActive = false;
    private bool needsWarningHealth = false;
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
            trans.position = stationLocation.position + height * Vector3.up + width * Vector3.right;
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

    public void SetAlwaysActive(bool overCharged, bool needsWarning)
    {
        overchargeActive = overCharged;
        needsWarningHealth = needsWarning;
        SetActive();
        if (needsWarningHealth && overchargeActive)
        {
            StartCoroutine(IconFlash(overChargedWarning));
        }
        else
        {
            StopCoroutine(nameof(IconFlash));
        }
    }

    private IEnumerator IconFlash(Image overChargedWarning)
    {
        overChargedWarning.enabled = true;
        Color color = overChargedWarning.color;

        while (needsWarningHealth)
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
