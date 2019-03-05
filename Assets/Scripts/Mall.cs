using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mall : MonoBehaviour
{

    private static Mall s_instance = null;

    [SerializeField][Tooltip("The patron spawner")]
    private PatronSpawner m_patronSpawner;

    private Store[] m_stores;
    private RestArea[] m_restAreas;
    private FoodVendor[] m_foodVendors;

    [SerializeField][Tooltip("The location where patrons go to leave")]
    private GameObject m_exitLocation;

    [SerializeField]
    private int m_population;

    //maybe this should be the maximum patrons that spawn?
    [SerializeField]
    private int m_maxPopulation;

    public static Mall Instance
    {
        set
        {
            if (s_instance != null)
            {
                Destroy(value);
                return;
            }

            s_instance = value;
        }
        get
        {
            if (!s_instance)
            {
                new GameObject(Constants.kMall, typeof(Mall));
            }
            return s_instance;
        }
    }

    private void Awake()
    {
        s_instance = this;

        m_stores = FindObjectsOfType<Store>();
        m_restAreas = FindObjectsOfType<RestArea>();
        m_foodVendors = FindObjectsOfType<FoodVendor>();
    }

    public int Population
    {
        get { return m_population; }
        set { m_population = value; }
    }

    public int MaxPopulation
    {
        get { return m_maxPopulation; }
        set { m_maxPopulation = value; }
    }

    public Store[] Stores
    {
        get { return m_stores; }
    }

    public RestArea[] RestAreas
    {
        get { return m_restAreas; }
    }

    public FoodVendor[] FoodVendors
    {
        get { return m_foodVendors; }
    }

    public GameObject ExitLocation
    {
        get { return m_exitLocation; }
    }
}
