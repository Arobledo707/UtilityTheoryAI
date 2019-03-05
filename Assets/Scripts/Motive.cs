using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public abstract class Motive : ScriptableObject
{
    public enum Type
    {
        EatMotive,
        ShopMotive,
        LeaveMotive,
        RestMotive,
        InvalidMotive
    }

    public abstract Type MotiveType();
    public abstract float Score();
    public abstract float MotiveValue();
    public abstract string Name();
    public abstract void TickMotive();
    public abstract void AddMotive(float amount);
    public abstract bool Action(Patron patron);
}

[System.Serializable]
public class EatMotive : Motive
{
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float m_hunger = 1.0f;

    private float m_hungerDivisor = 0.2f;

    private float m_hungerExponent = -6.0f;

    [SerializeField]
    private float m_hungerConstant = 0.02f;

    [SerializeField]
    private float m_hungerDecay = 0.025f;


    public override bool Action(Patron patron)
    {
        foreach (FoodVendor vendor in Mall.Instance.FoodVendors)
        {
            if (vendor.Patron == null)
            {
                vendor.Patron = patron;
                patron.SetMovementTarget(vendor.gameObject);
                patron.CurrentAction = Constants.kEatAction;
                return true;
            }
        }
        return false;
    }

    public override void AddMotive(float amount)
    {
        m_hunger += amount;
        m_hunger = Mathf.Clamp(m_hunger, 0.0f, 1.0f);
    }

    public override float MotiveValue()
    {
        return m_hunger;
    }

    public override string Name()
    {
        return "Eat";
    }
    public override void TickMotive()
    {
        m_hunger -= m_hungerDecay;
        m_hunger = Mathf.Clamp(m_hunger, 0.0f, 1.0f);
    }

    public override float Score()
    {
        float score = Mathf.Pow((m_hunger / m_hungerDivisor), m_hungerExponent) - m_hungerConstant;
        score = Mathf.Clamp(score, 0.0f, 1.0f);
        return score;
    }

    public override Type MotiveType()
    {
        return Type.EatMotive;
    }
}

[System.Serializable]
public class ShopMotive : Motive
{
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float m_shopDesire = 0.6f;

    public override bool Action(Patron patron)
    {
        int count = Mall.Instance.Stores.Length;

        int random = Random.Range(0, Mall.Instance.Stores.Length);

        List<Store> stores = new List<Store>(Mall.Instance.Stores);

        if (patron.MovementTarget == (Mall.Instance.Stores[random].gameObject))
        {
            stores.RemoveAt(random);
            if (random >= stores.Count)
            {
                random -= 1;
            }
        }
        patron.CurrentAction = Constants.kShoppingAction;
        patron.SetMovementTarget(stores[random].gameObject);

        return true;
    }
    public override float Score()
    {
        return m_shopDesire;
    }

    public override void AddMotive(float amount)
    {
    }

    public override float MotiveValue()
    {
        return m_shopDesire;
    }

    public override void TickMotive()
    {
    }


    public override string Name()
    {
        return "Shop";
    }

    public override Type MotiveType()
    {
        return Type.ShopMotive;
    }
}

[System.Serializable]
public class LeaveMotive : Motive
{
    // U = Population^2.85  
    // was population to the power of 2/.7
    [SerializeField]
    private float m_populationExponent = 2.85f;

    private Motive m_EnergyScoreMotive;

    public void SetRestMotive(Motive motive)
    {
        if (motive.MotiveType() == Type.RestMotive)
        {
            m_EnergyScoreMotive = motive;
        }
        else
        {
            Debug.LogError("Rest Motive not assigned for LeaveMotive");
        }
    }

    public override string Name()
    {
        return "Population";
    }

    public override void TickMotive()
    {
    }

    public override bool Action(Patron patron)
    {
        patron.SetMovementTarget(Mall.Instance.ExitLocation);
        patron.CurrentAction = Constants.kLeaveAction;
        return true;
    }

    public override float MotiveValue()
    {
        //Debug.Log(Mathf.Clamp((float)Mall.Instance.Population / (float)Mall.Instance.MaxPopulation, 0.0f, 1.0f));
        return Mall.Instance.Population;
    }

    public override void AddMotive(float amount)
    {
    }

    public override float Score()
    {
        float energyScore = m_EnergyScoreMotive.Score();
        energyScore = Mathf.Clamp(energyScore, 0.0f, 1.0f);
        float populationScore = Mall.Instance.Population / (float)Mall.Instance.MaxPopulation;
        float score = (energyScore + populationScore) / 2.0f;
        score = Mathf.Clamp(score, 0.0f, 1.0f);
        return score;
    }

    public override Type MotiveType()
    {
        return Type.LeaveMotive;
    }
}

[System.Serializable]
public class RestMotive : Motive
{
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float m_energy = 1.0f;

    [SerializeField]
    private float m_energyExponent = -0.1f;

    [SerializeField]
    private float m_energyConstant = 0.5f;

    [SerializeField]
    private float m_energyDecay = 0.025f;

    public override bool Action(Patron patron)
    {
        //this could change
        //unless if score restAreas by distance
        //RestArea restArea = Mall.Instance.FindRestArea();

        foreach (RestArea restArea in Mall.Instance.RestAreas)
        {
            if (restArea.Patron == null)
            {
                restArea.Patron = patron;
                patron.SetMovementTarget(restArea.gameObject);
                patron.CurrentAction = Constants.kRestAction;
                return true;
            }
        }
        return false;
    }

    public override void AddMotive(float amount)
    {
        m_energy += amount;
        m_energy = Mathf.Clamp(m_energy, 0.0f, 1.0f);
    }

    public override string Name()
    {
        return "Rest";
    }

    public override void TickMotive()
    {
        m_energy -= m_energyDecay;
        m_energy = Mathf.Clamp(m_energy, 0.0f, 1.0f);
    }

    public override float MotiveValue()
    {
        return m_energy;
    }


    public override float Score()
    {
        float score = (Mathf.Pow(m_energy, m_energyExponent)) - m_energyConstant;
        score = Mathf.Clamp(score, 0.0f, 1.0f);
        return score;
    }

    public override Type MotiveType()
    {
        return Type.RestMotive;
    }
}