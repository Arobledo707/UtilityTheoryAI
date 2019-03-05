using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Parent class of activites that patrons can go to
public class Activity : MonoBehaviour
{
    protected Patron m_patron;

    public Patron Patron
    {
        get { return m_patron; }
        set { m_patron = value; }
    }


    virtual public void DoAction(Patron patron)
    {

    }

    protected void RemotePatron()
    {
        m_patron.ActivityState = Patron.State.kFree;
        m_patron.Moving = false;
        m_patron.HasTask = false;
        m_patron = null;
    }
}
