using UnityEngine;

public class PickUpInteractable : Interactable
{
    public delegate void PickedUpEvent();

    public event PickedUpEvent HandlePickedUp;
        
    public bool isPickedUp;
    private Rigidbody rb;
    public void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
    }
    
    public override void Interact(GameObject other)
    {
        var state = other.GetComponent<AgentState>();
        if (state == null) return;

        if (!isPickedUp)
            PickUp(state);
        else
            Drop(state);
    }

    public void PickUp(AgentState state)
    {
        // already has a picked up an object so do nothing
        if (state.pickedUpObject != null) return;
        transform.parent = state.pickUpPoint.transform;
        transform.localPosition = Vector3.zero;
        state.pickedUpObject = this;
        rb.isKinematic = true;
        // clear the interactable slot so the agent can interact with other objects
        state.interactable = null;
        // set the agent to non interactable so Triggers won't collide with it
        gameObject.layer = LayerMask.NameToLayer("NonInteractable");
        outline.OutlineMode = Outline.Mode.OutlineHidden;
        isPickedUp = true;
        HandlePickedUp?.Invoke();
    }

    public void Drop(AgentState state)
    {
        state.pickedUpObject = null;
        rb.isKinematic = false;
        transform.parent = null;
        // reset to default so object can be interactable again
        gameObject.layer = LayerMask.NameToLayer("Default");
        isPickedUp = false;
    }
}