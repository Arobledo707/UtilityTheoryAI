using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dog : Activity
{
    [SerializeField][Tooltip("Time that other patron spends petting the dog")]
    private float m_petTime = 5.0f;

    [SerializeField][Tooltip("The time that has to pass by before another patron is allowed to interact with dog")]
    private float m_patronInteractionCooldown = 10.0f;

    [SerializeField][Tooltip("The delay until patrons can interact with dogs")]
    private float m_startTimeDelay = 3.0f;

    [SerializeField]
    private Patron m_owner;

    private bool m_InteractionReady = false;

    [SerializeField][Tooltip("Speed that the owner of dog is set to when patron is going to pet dog")]
    private readonly float m_ownerMovementSpeedOnInteract = 0.0f;

    private void Awake()
    {
        m_owner = transform.parent.gameObject.GetComponent<Patron>();
        StartCoroutine(StartDelayCooldown());
    }

    public override void DoAction(Patron patron)
    {
        if (m_InteractionReady)
        {
            RaycastHit hit;
            if (Physics.Raycast(gameObject.transform.position, patron.gameObject.transform.position, out hit, 1.5f))
            {
                if (hit.transform.gameObject.CompareTag(Constants.kWall))
                {
                    return;
                }
            }
            StartCoroutine(RecievePets(patron));
        }
    }

    private IEnumerator StartCooldown()
    {
        yield return new WaitForSeconds(m_patronInteractionCooldown);
        m_InteractionReady = true;
    }

    private IEnumerator RecievePets(Patron patron)
    {
        GameObject previousDestination = null;
        string previousAction = patron.CurrentAction;

        string ownerPreviousAction = m_owner.CurrentAction;

        m_owner.CurrentAction = Constants.kDogBeingPet;

        m_InteractionReady = false;

        patron.CurrentAction = Constants.kPetDogAction;
        patron.EnableHeart();

        if (patron.MovementTarget != null && patron.MovementTarget.tag != Constants.kDog)
        {
            previousDestination = patron.MovementTarget;
        }
        float ownerMovementSpeed = m_owner.MovementSpeed;

        m_owner.SetMovementSpeed(m_ownerMovementSpeedOnInteract);

        patron.SetMovementTarget(gameObject);

        yield return new WaitForSeconds(m_petTime);

        m_owner.SetMovementSpeed(ownerMovementSpeed);

        if (previousDestination != null)
        {
            patron.SetMovementTarget(previousDestination);
        }
        patron.DisableHeart();

        if (patron.CurrentAction != null)
        {
            patron.CurrentAction = previousAction;
        }

        m_owner.CurrentAction = ownerPreviousAction;

        StartCoroutine(StartCooldown());
    }

    private IEnumerator StartDelayCooldown()
    {
        yield return new WaitForSeconds(m_startTimeDelay);
        m_InteractionReady = true;
    }
}
