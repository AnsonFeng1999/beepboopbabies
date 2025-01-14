using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

[RequireComponent(typeof(BabyState))]
[RequireComponent(typeof(BabyPickUpInteractable))]
public class BabyController : MonoBehaviour
{
    public AudioSource explodeSound;
    public AudioSource leakSound;
    // Start is called before the first frame update
    public BabyUIController uiController;
    public GameObject Floor;
    public GameObject explosionEffect;
    public GameObject oilLeakEffect;
    private BabyPickUpInteractable interactable;
    public UnityEngine.AI.NavMeshAgent agent;
    public GameObject basebaby;
    // variable used to track consequences if any of the stats fell to zero
    // all colliders that are activated when using ragdoll
    public Collider capsuleCollider;
    public Collider[] allCollider;
    // all the rigidbodies used by ragdoll
    public List<Rigidbody> ragdollRigidBodies;
    // animator used to controll different animation state of the character
    public Animator anim;
    // colliders that needs to be enabled when not using ragdoll
    public Rigidbody rb;
    // Object to spawn when leaking oil
    public GameObject oilPuddle;
    // every one 1 second decrement by 2 units
    [SerializeField] private float decrementAmountEnergy = 2f;
    [SerializeField] private float decrementAmountDiaper = 2f;
    [SerializeField] private float oilDrinkAmountOil = 2f;
    [SerializeField] private float decrementAmountOil = 2f;
    [SerializeField] private float funDecreasePerSecondIdle = 2f;
    [SerializeField] private float funIncreasePerSecondFlying = 25f;
    [SerializeField] private List<GameObject> bodyParts;
    [SerializeField] private List<GameObject> bodyPartsToHide;
    private bool isLocked;
    private bool healthZero;
    private bool energyZero;
    private bool oilZero;
    private bool funZero;
    private bool ragdoll;
    // if we are missing either legs the baby will ragdoll
    private bool rightLegMissing;
    private bool leftLegMissing;
    private BabyState state;
    private float healthDecreasePerDrop;
    private float decreaseHealthPerKick = 10f;
    private float decreaseHealthPerSec = 1f; // This is used when out of oil, health will slowly decrease
    private float decreaseHealthOverCharge = 10f; // used when diaper is
    private static readonly int Walking = Animator.StringToHash("Walking");
    private static readonly int InStation = Animator.StringToHash("InStation");
    private static readonly int BabyShaking = Animator.StringToHash("BabyShake");
    private GameObject overchargeEffectInner;

    private bool oilLeaked = false;
    private bool overCharged = false;       // OverCharge flag, there is an getter for this.
    
    public delegate void BodyPartDetachedHandler();

    public event BodyPartDetachedHandler HandleBodyPartDetached;
    
    public void Awake()
    {
        state = GetComponent<BabyState>();
        healthDecreasePerDrop = state.health / bodyParts.Count;
        interactable = GetComponent<BabyPickUpInteractable>();
        allCollider = GetComponentsInChildren<Collider>().Skip(1).ToArray();
        ragdollRigidBodies = new List<Rigidbody>();
        capsuleCollider = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        foreach (var collider in allCollider)
        {
            if (collider.transform != transform) // if this is not parent transform
            {
                var rag_rb = collider.GetComponent<Rigidbody>(); // get attached rigidbody
                if (rag_rb)
                {
                    ragdollRigidBodies.Add(rag_rb); // add to list
                }
            }
        }
    }

    public void Start()
    {
        EnableRagdoll(false);
        uiController.SetName(state.name);
    }
    public void OnEnable()
    {
        interactable.HandlePickedUp += HandlePickedUp;
        interactable.HandleKicked += HandleKicked;
    }

    public void OnDisable()
    {
        interactable.HandlePickedUp -= HandlePickedUp;
        interactable.HandleKicked -= HandleKicked;
    }



    private void Update()
    {
        basebaby.transform.localPosition = Vector3.zero;
        HandleAnimation();
        HandleFun();
        HandleOil();
        DecreaseDiaper();
        DecreaseEnergy();
        SetAlwaysActive();
        HandleConsequence();
        //Increased water sound
        if (transform.GetChild(2).childCount != 0)
        {
            if (GetComponent<AudioSource>().isPlaying == false)
            {
                CharacterUpdate.Instance.StopPlay();
                GetComponent<AudioSource>().Play();
            }
        }
        else
        {
            if (GetComponent<AudioSource>().isPlaying)
                GetComponent<AudioSource>().Stop();
        }
    }

    public void HandleAnimation()
    {
        anim.SetBool(InStation, interactable.inStation);
        anim.SetBool(Walking, agent.velocity.magnitude > 0.1f);
        anim.SetBool(BabyShaking, oilZero || energyZero);
    }

    public void HandleConsequence()
    {
        var healthIsZero = Mathf.Approximately(state.currentHealth, 0.0f);
        funZero = Mathf.Approximately(state.currentFun, 0.0f);
        energyZero = Mathf.Approximately(state.currentEnergy, 0.0f);
        oilZero = Mathf.Approximately(state.currentOil, 0.0f);
        // Consequence for health (baby will explode if health is 0)
        if (healthIsZero && !healthZero)
        {
            var station = GetComponentInParent<StationInteractable>();
            if (station)
            {
                Debug.Log("Baby Health bar zero dropping baby");
                station.pickedUpObject.Drop(station.GetComponent<AgentState>());
                station.pickedUpObject = null; //TODO: refactor later
                Debug.Log("set station pick up object to null");
            }
            healthZero = true;
            explodeSound.Play();
            // explodes when health is 0
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Collider[] colliders = Physics.OverlapSphere(transform.position, state.explosionRadius);
            foreach (Collider nearbyObject in colliders)
            {
                // kick every baby nearby
                var baby = nearbyObject.GetComponent<BabyPickUpInteractable>();
                if (baby != null)
                {
                    baby.KickExplosive(state.explosionForce, transform.position, state.explosionRadius);
                }
            }
            for (int i = 0; i < bodyParts.Count; i++)
            {
                DetachBodyPart(i);
            }
            EnableRagdoll(true);
        }
        else if (!healthIsZero)
        {
            healthZero = false;
        }
        // Consequence for fun (baby will start kicking other babies if fun is 0)
        state.isSad = funZero;
        if (isLocked && state.currentHealth > 0.0f && state.currentEnergy > 0.0f && state.currentOil > 0.0f && !ragdoll)
        {
            isLocked = false;
            interactable.UnlockBaby();
        }
        else if (!isLocked && (healthZero || energyZero))
        {
            isLocked = true;
            interactable.LockBaby();
        }
        // Consequence for diaper (leak oil)
        if (!oilLeaked && Mathf.Approximately(state.currentDiaper, 0.0f))
        {
            oilLeaked = true;
            // play water spill effect and
            leakSound.Play();
            Instantiate(oilLeakEffect, transform.position + Vector3.up, Quaternion.identity);
            Instantiate(oilPuddle, new Vector3(transform.position.x, 0.1f, transform.position.z), Quaternion.identity);
        }
        else if (state.currentDiaper > 0.9f)
        {
            oilLeaked = false;
        }
        // Consequence for oil is zero (baby will slowly decrease health until explode)
        if (oilZero)
        {
            DecreaseHealth(decreaseHealthPerSec * Time.deltaTime);
        }
    }

    private void SetAlwaysActive()
    {
        bool? healthAlwaysActive = null;
        bool? energyAlwaysActive = null;
        bool? diaperAlwaysActive = null;
        bool? funAlwaysActive = null;
        bool? oilAlwaysActive = null;
        if (state.currentHealth < state.healthWarnThreshold)
        {
            healthAlwaysActive = true;
        }
        if (state.currentEnergy < state.energyWarnThreshold)
        {
            energyAlwaysActive = true;
        }
        if (state.currentDiaper < state.diaperWarnThreshold)
        {
            diaperAlwaysActive = true;
        }
        if (state.currentFun < state.funWarnThreshold)
        {
            funAlwaysActive = true;
        }
        if (state.currentOil < state.oilWarnThreshold)
        {
            oilAlwaysActive = true;
        }
        uiController.SetAlwaysActive(healthAlwaysActive, energyAlwaysActive, diaperAlwaysActive, funAlwaysActive, oilAlwaysActive);
    }

    public bool SetBodyPart(BodyPartInteractable.BodyPartType type, Material mat)
    {
        for (int i = 0; i < bodyParts.Count; i++)
        {
            // find body part with the same type
            var bodyPart = bodyParts[i].GetComponent<BodyPartInteractable>();
            if (bodyPart.bodyPartType == type)
            {
                if (type == BodyPartInteractable.BodyPartType.RightLeg)
                {
                    rightLegMissing = false;
                }
                else if (type == BodyPartInteractable.BodyPartType.LeftLeg)
                {
                    leftLegMissing = false;
                }

                if (bodyPartsToHide[i].activeInHierarchy)
                {
                    return false;
                }
                bodyPartsToHide[i].SetActive(true);
                bodyPartsToHide[i].GetComponent<Renderer>().material = mat;
                state.healthcap += healthDecreasePerDrop;
                return true;
            }
        }
        return false;
    }

    public bool GetOverCharged ()
    {
        return overCharged;
    }

    public void IncreaseEnergy(float incrementAmount)
    {
        state.currentEnergy += incrementAmount;
        // Overcharging, currently handle here
        overCharged = state.currentEnergy > state.energy;
        state.currentEnergy = Math.Clamp(state.currentEnergy, 0f, state.energy);
        uiController.UpdateEnergyBar(state.energy, state.currentEnergy);
    }

    public void IncreaseDiaper(float incrementAmount)
    {
        state.currentDiaper += incrementAmount;
        state.currentDiaper = Math.Clamp(state.currentDiaper, 0f, state.diaper);
        uiController.UpdateDiaperBar(state.diaper, state.currentDiaper);
    }

    public void IncreaseHealth(float incrementAmount)
    {
        state.currentHealth += incrementAmount;
        state.currentHealth = Math.Clamp(state.currentHealth, 0f, state.healthcap);
        uiController.UpdateHealthBar(state.health, state.currentHealth);
    }

    private void HandleFun()
    {
        if (ragdoll) return;
        if (state.isFlying)
        {
            state.currentFun = Mathf.Min(state.currentFun + funIncreasePerSecondFlying * Time.deltaTime, state.fun);
            uiController.SetAlwaysActive(fun: true);
        }
        else
        {
            state.currentFun = Mathf.Max(state.currentFun - funDecreasePerSecondIdle * Time.deltaTime, 0);
            uiController.SetAlwaysActive(fun: false);
        }
        uiController.UpdateFunBar(state.fun, state.currentFun);
    }

    private void HandleOil()
    {

        if (oilLeaked)
        {
            state.currentOil = 0;
        }
        
        if (state.pickedUpObject is BottleInteractable drinkBottle && state.currentOil <= state.oil)
        {
            drinkBottle.DecreaseAmount(oilDrinkAmountOil * Time.deltaTime);
            // if bottle is empty, drop it
            if (Mathf.Approximately(drinkBottle.currentAmount, 0))
            {
                drinkBottle.Drop(state);
                return;
            }
            state.currentOil = Mathf.Min(state.currentOil + oilDrinkAmountOil * Time.deltaTime, state.oil);
            uiController.UpdateOilBar(state.oil, state.currentOil);
        }
        // if we are fill to the max drop the bottle
        else if (state.pickedUpObject is BottleInteractable dropBottle && state.currentOil >= state.oil)
        {
            dropBottle.Drop(state);
        }
        else
        {
            state.currentOil = Mathf.Max(state.currentOil - decrementAmountOil * Time.deltaTime, 0);
            uiController.UpdateOilBar(state.oil, state.currentOil);
            uiController.SetAlwaysActive(oil: false);
        }
    }

    public void DecreaseEnergy()
    {
        if (state.currentEnergy >= 0)
        {
            state.currentEnergy = Mathf.Max(state.currentEnergy - decrementAmountEnergy * Time.deltaTime, 0);
            uiController.UpdateEnergyBar(state.energy, state.currentEnergy);
        }
    }

    public void DecreaseDiaper()
    {
        if (state.currentDiaper >= 0)
        {
            state.currentDiaper = Mathf.Max(state.currentDiaper - decrementAmountDiaper * Time.deltaTime, 0);
            uiController.UpdateDiaperBar(state.diaper, state.currentDiaper);
        }
    }

    public void DecreaseHealth(float decreaseAmount)
    { 
        if (state.currentHealth >= 0)
        {
            state.currentHealth = Mathf.Max(state.currentHealth - decreaseAmount, 0);
            state.currentHealth = Mathf.Min(state.currentHealth, state.healthcap);
            uiController.UpdateHealthBar(state.health, state.currentHealth);
            
        }
    }
    

    private void DetachBodyPart(int index)
    {
        var bodyPart = bodyPartsToHide[index];
        if (bodyPart.activeInHierarchy)
        {
            HandleBodyPartDetached?.Invoke();
            
            state.healthcap -= healthDecreasePerDrop;
            state.currentHealth = Mathf.Min(state.currentHealth, state.health);
            bodyPart.SetActive(false);
            var detachBodyPart = bodyParts[index];
            // instantiate the detached body part
            var detachedBodyPart = Instantiate(detachBodyPart, bodyPart.transform.position, bodyPart.transform.rotation);
            var part = detachedBodyPart.GetComponent<BodyPartInteractable>();
            var rb = detachedBodyPart.GetComponent<Rigidbody>();
            part.SetBodyPartName(state.name);
            // apply material to the detached body part
            detachedBodyPart.GetComponent<Renderer>().material = bodyPart.GetComponent<Renderer>().material;
            // add force to the detached body part
            // explosive kick body part
            rb.AddExplosionForce(state.explosionForce, transform.position, state.explosionRadius);
            if (part.bodyPartType == BodyPartInteractable.BodyPartType.LeftLeg)
            {
                leftLegMissing = true;
            }
            else if (part.bodyPartType == BodyPartInteractable.BodyPartType.RightLeg)
            {
                rightLegMissing = true;
            }
        }
    }

    public void EnableRagdoll(bool enableRagdoll)
    {
        ragdoll = enableRagdoll;
        anim.enabled = !enableRagdoll;
        rb.isKinematic = true;
        if (enableRagdoll)
        {
            interactable.DisableAI();
            interactable.LockBaby();
            capsuleCollider.isTrigger = true;
        }
        else
        {
            capsuleCollider.isTrigger = false;
            interactable.UnlockBaby();
        }
        foreach (Collider item in allCollider)
        {
            item.enabled = enableRagdoll; // enable all colliders  if ragdoll is set to enabled
        }
        foreach (var ragdollRigidBody in ragdollRigidBodies)
        {
            ragdollRigidBody.useGravity = enableRagdoll; // make rigidbody use gravity if ragdoll is active
            ragdollRigidBody.isKinematic = !enableRagdoll; // enable or disable kinematic accordig to enableRagdoll variable
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (ReferenceEquals(collision.gameObject, Floor))
        {
            if (state.isFlying)
            {
                state.isFlying = false;
                // interactable.EnableAI();
                // Sometimes, baby will lose bodypart
                // Guaranteed to lose body part in tutorial
                if (LevelsManager.Instance.IsTutorial || Random.Range(0, 100) < 25)
                {
                  // randomly choose a body part
                    DetachBodyPart(UnityEngine.Random.Range(0, bodyPartsToHide.Count));
                }
                DecreaseHealth(healthDecreasePerDrop);
                StartCoroutine(TemporaryAlert(health: true));
            }
            if (rightLegMissing || leftLegMissing)
            {
                EnableRagdoll(true);
            }
            else
            {
                interactable.EnableAI();
            }
        }
    }

    private IEnumerator TemporaryAlert(bool? health = null, bool? energy = null, bool? diaper = null, bool? fun = null, bool? oil = null)
    {
        uiController.SetAlwaysActive(health, energy, diaper, fun, oil);
        yield return new WaitForSeconds(1f);
        if (health.GetValueOrDefault(false)) uiController.SetAlwaysActive(health: false);
        if (energy.GetValueOrDefault(false)) uiController.SetAlwaysActive(energy: false);
        if (diaper.GetValueOrDefault(false)) uiController.SetAlwaysActive(diaper: false);
        if (fun.GetValueOrDefault(false)) uiController.SetAlwaysActive(fun: false);
        if (oil.GetValueOrDefault(false)) uiController.SetAlwaysActive(oil: false);
    }

    private void HandlePickedUp()
    {
        state.isFlying = false;
        EnableRagdoll(false);
    }

    private void HandleKicked()
    {
        DecreaseHealth(decreaseHealthPerKick);
        StartCoroutine(TemporaryAlert(health: true));
        state.isFlying = true;
        interactable.DisableAI();
    }
}