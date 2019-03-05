using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Store : Activity
{
    [SerializeField]
    private float m_patronInStoreTime = 10.0f;

    override public void DoAction(Patron patron)
    {
        StartCoroutine(PatronInStore(patron));
    }

    IEnumerator PatronInStore(Patron patron)
    {
        patron.ActivityState = Patron.State.kLocked;
        patron.DisableSprite();
        yield return new WaitForSeconds(m_patronInStoreTime);

        patron.ActivityState = Patron.State.kFree;
        patron.EnableSprite();
        patron.Moving = false;
        patron.HasTask = false;

    }

}
