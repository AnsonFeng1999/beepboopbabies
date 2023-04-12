using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChargeStationUIController : MonoBehaviour
{
    [SerializeField] private Image overChargedWarning;
    [SerializeField] private Transform stationLocation;
    private bool? overchargeActive = false;
    private Camera _cam;

    //public float depletionTime = 4f;
    public float depletionAmount = 0.1f;
    public float depletionInterval = 1f;

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
    public void SetActive ()
    {
        overChargedWarning.gameObject.SetActive(overchargeActive.GetValueOrDefault(false));
    }

    public void setAlwaysActive(bool overCharged, bool needsWarning)
    {
        overchargeActive = overCharged;
        SetActive();
        if (needsWarning)
        {
            StartCoroutine(IconFlash(overChargedWarning));
        }
    }

    private IEnumerator IconFlash(Image overChargedWarning)
    {
        overChargedWarning.enabled = true;
        Color color = overChargedWarning.color;
        float alpha = color.a;

        while (alpha > 0.2f)
        {
            alpha -= depletionAmount;
            color.a = alpha;
            overChargedWarning.color = color;
            yield return new WaitForSeconds(1f);
        }

        yield return new WaitForSeconds(5f);

        while (alpha < 1f)
        {
            alpha += depletionAmount;
            color.a = alpha;
            overChargedWarning.color = color;
            yield return new WaitForSeconds(1f);
        }

        color.a = 1.0f;
        overChargedWarning.color = color;
    }
}
