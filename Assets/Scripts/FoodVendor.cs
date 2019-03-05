using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodVendor : Activity
{
    [SerializeField][Tooltip("The time patrons spend eating")]
    private float m_eatTime = 5.0f;

    [Tooltip("Enter a value from 0 to 1")]
    [SerializeField][Range(0.0f, 1.0f)]
    private float m_foodEaten = 1.0f;

    private void Awake()
    {
        m_foodEaten = Mathf.Clamp(m_foodEaten, 0.0f, 1.0f);
    }

   override public void DoAction(Patron patron)
    {
        m_patron = patron;

        if (m_patron != null)
        {
            StartCoroutine(EatFood(m_patron));
        }
    }

    IEnumerator EatFood(Patron patron)
    {
        patron.ActivityState = Patron.State.kLocked;
        yield return new WaitForSeconds(m_eatTime);
        m_patron.MotiveDictionary[Motive.Type.EatMotive].AddMotive(m_foodEaten);
        RemotePatron();
    }

}
