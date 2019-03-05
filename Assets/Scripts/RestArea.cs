using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestArea : Activity
{
    [SerializeField]
    [Tooltip("Wait time that a patron spends at a rest area")]
    private float m_waitTime = 5.0f;

    [SerializeField]
    [Tooltip("Amount of time energy gain tick happens")]
    private float m_waitTimeForEnergy = 1.0f;

    [SerializeField]
    [Tooltip("How much energy the patron gains per tick")]
    private int m_energyGainedOverTime = 10;

    private Coroutine m_energyCoroutine;


    override public void DoAction(Patron patron)
    {
        m_patron = patron;
        if (m_patron != null)
        {
            StartCoroutine(IncreaseEnergy(m_patron));
        }
        else
        {
            Debug.LogWarning("RestArea patron is null. Not running Action");
        }
    }

    private IEnumerator AddEnergyPerTick()
    {
        while (true)
        {
            m_patron.MotiveDictionary[Motive.Type.RestMotive].AddMotive(m_energyGainedOverTime);
            yield return new WaitForSeconds(m_waitTimeForEnergy);
        }
    }


    private IEnumerator IncreaseEnergy(Patron patron)
    {
        patron.ActivityState = Patron.State.kLocked;
        m_energyCoroutine = StartCoroutine(AddEnergyPerTick());

        yield return new WaitForSeconds(m_waitTime);
        StopCoroutine(m_energyCoroutine);
        RemotePatron();
    }

}
