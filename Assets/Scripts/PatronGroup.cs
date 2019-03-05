using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PatronGroup : ScriptableObject
    // TODO
    // monobehavior not scriptableObject
    // make each group have a parent object that is the center point for the patrons in a group
    // this would make it easier to have the patrons all go to one food area and easier to create positions for each of them
{
    private List<Patron> m_patrons = new List<Patron>();

    private float m_groupSpeed;

    private int m_groupSize;

    //ticking inididual group when one patron reaches destination

    [SerializeField]
    [Tooltip("The minimum movement speed that Groups can have")]
    private float m_groupMinMovementSpeed = 0.8f;

    [SerializeField]
    [Tooltip("The maximum movement speed that Groups can have")]
    private float m_groupMaxMovementSpeed = 1.75f;

    private GameObject m_movementDestination;

    // average the motives from each patron???
    // or just pick one patron motive and do that
    // does group have dog????


    public void GenerateGroupSize(int howManyCanSpawn)
    {
        if (howManyCanSpawn > 4)
        {
            m_groupSize = Random.Range(1, 4);
        }
        else if (howManyCanSpawn > 0)
        {
            m_groupSize = Random.Range(1, howManyCanSpawn);
        }
        else
        {
            howManyCanSpawn = 0;
        }
    }

    public int GroupSize
    {
        get { return m_groupSize; }
    }

    public void AddPatronToGroup(Patron patron)
    {
        m_patrons.Add(patron);
    }

    public void SyncMovementSpeed()
    {
        float randomMovementSpeed = Random.Range(m_groupMinMovementSpeed, m_groupMaxMovementSpeed);
        m_groupSpeed = randomMovementSpeed;

        foreach(Patron patron in m_patrons)
        {
            patron.SetMovementSpeed(m_groupSpeed);
        }
    }



    private void SetDestination()
    {
        foreach (Patron patron in m_patrons)
        {
            if (m_movementDestination == null)
            {
                m_movementDestination = patron.MovementTarget;
                continue;
            }
            patron.SetMovementTarget(m_movementDestination);
        }
    }

}
