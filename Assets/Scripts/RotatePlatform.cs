using UnityEngine;

public class RotatePlatform : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 30f;
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    
    void Update()
    {
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        // other spin interactable or player components
        if (other.GetComponent<Interactable>() || other.GetComponent<PlayerController>())
        {
            var interactable = other.GetComponent<PickUpInteractable>();
            if (interactable == null || !interactable.isPickedUp)
            {
                other.gameObject.transform.parent = transform;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Interactable>() || other.GetComponent<PlayerController>())
        {
            var interactable = other.GetComponent<PickUpInteractable>();
            if (interactable == null || !interactable.isPickedUp)
            {
                other.gameObject.transform.parent = null;
            }
        }
    }
}