using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class Patron : MonoBehaviour

{
    private delegate bool Action(Patron patron);

    public enum State
    {
        kFree,
        kLocked
    }

    private State m_activityState = State.kFree;


#if UNITY_EDITOR
    private List<KeyValuePair<Action, KeyValuePair<string, float>>> m_recentMotiveScores = new List<KeyValuePair<Action, KeyValuePair<string, float>>>();
    private List<string> m_lastDecisionsMade = new List<string>();
    private int m_storedActionsConsidered = 3;

#endif

    [SerializeField]
    private float m_movementSpeed = 1.0f;

    [SerializeField]
    private GameObject m_movementTarget;
    private bool m_moving = false;

    private NavMeshAgent m_aiAgent;

    [SerializeField]
    [Tooltip("The time it takes for the AI to tick")]
    private float m_aiTickTime = 0.25f;

    private Dictionary<Motive.Type, Motive> m_motiveDictionary = new Dictionary<Motive.Type, Motive>();

    [SerializeField]
    [Tooltip("The prefab for the dog that gets instantiated")]
    private GameObject m_dogPrefab;

    [SerializeField]
    [Range(0.0f, 100.0f)]
    [Tooltip("Chance that dogs can spawn.")]
    private float m_dogSpawnChance = 40.0f;
    private float m_dogSpawnXoffSet = 0.5f;
    private bool m_hasDog = false;

    private PatronGroup m_group;

    private int m_motivesToConsider = 3;

    private const int m_minimumPatronActions = 2;
    private int m_patronActionsDone = 0;

    private GameObject m_lastStoreVisited = null;

    [SerializeField]
    private GameObject m_heartSprite;


#if UNITY_EDITOR
    //Debug--------------------------------------------------------------------DEBUG
    private GameObject m_debugCanvas;

    private List<string> m_actionsConsidered = new List<string>();

    public List<string> ActionsConsidered
    {
        get { return m_actionsConsidered; }
    }

    public List<string> LastDecisionsMade
    {
        get { return m_lastDecisionsMade; }
    }

    public string CurrentAction { get; set; }

    //-------------------------------------------------------------------------DEBUG
#endif

    public State ActivityState
    {
        get { return m_activityState; }
        set { m_activityState = value; }
    }

    public NavMeshAgent Agent
    {
        get { return m_aiAgent; }
    }

    public void SetGroup(PatronGroup group)
    {
        m_group = group;
    }

    public GameObject MovementTarget
    {
        get { return m_movementTarget; }
        set { m_movementTarget = value; }
    }

    private bool m_hasTask = false;

    [SerializeField]
    private float m_motiveTickTime = 1.0f;

    public Dictionary<Motive.Type, Motive> MotiveDictionary
    {
        get { return m_motiveDictionary; }
    }

    public bool Moving
    {
        get { return m_moving; }
        set { m_moving = value; }
    }

    public bool HasTask
    {
        get { return m_hasTask; }
        set { m_hasTask = value; }
    }

    public float MovementSpeed
    {
        get { return m_movementSpeed; }
    }

    private void Awake()
    {
        m_aiAgent = GetComponent<NavMeshAgent>();
        m_aiAgent.speed = m_movementSpeed;
        m_aiAgent.updateRotation = false;

        float randDogSpawn = Random.Range(0.0f, 100.0f);

        if (randDogSpawn <= m_dogSpawnChance)
        {
            m_hasDog = true;
            Instantiate(m_dogPrefab, new Vector3(transform.position.x + m_dogSpawnXoffSet,
                transform.position.y, transform.position.z), Quaternion.identity,
                transform);
        }

        LeaveMotive leaveMotive = ScriptableObject.CreateInstance<LeaveMotive>() as LeaveMotive;

        m_motiveDictionary.Add(Motive.Type.LeaveMotive, leaveMotive);
        m_motiveDictionary.Add(Motive.Type.RestMotive, ScriptableObject.CreateInstance<RestMotive>());
        m_motiveDictionary.Add(Motive.Type.ShopMotive, ScriptableObject.CreateInstance<ShopMotive>());
        m_motiveDictionary.Add(Motive.Type.EatMotive, ScriptableObject.CreateInstance<EatMotive>());

        leaveMotive.SetRestMotive(m_motiveDictionary[Motive.Type.RestMotive]);

        //------------------------------------------------Debug
#if UNITY_EDITOR
        m_debugCanvas = GameObject.Find("PatronDebugCanvas");
#endif
        //-----------------------------------------------Debug
    }

    void Start()
    {
        StartCoroutine(FindNewTask());
        StartCoroutine(TickMotives());
        StartCoroutine(IdleCheck());
    }

    void Update()
    {
        OnMouseDown();
    }


    public void SetMovementSpeed(float movementSpeed)
    {
        m_movementSpeed = movementSpeed;
        m_aiAgent.speed = movementSpeed;
    }


    private IEnumerator TickMotives()
    {
        while (true)
        {
            UpdateMotives();
            yield return new WaitForSeconds(m_motiveTickTime);
        }
    }


    public void SetMovementTarget(GameObject position)
    {
        m_movementTarget = position;
        m_moving = true;
        m_aiAgent.SetDestination(m_movementTarget.transform.position);
    }


    private Action FindNewChoice()
    {
        m_recentMotiveScores.Clear();
        m_actionsConsidered.Clear();

        float totalMotiveScore = 0.0f;

        foreach (KeyValuePair<Motive.Type, Motive> motive in m_motiveDictionary)
        {
            float motiveScore = motive.Value.Score();
            totalMotiveScore += motiveScore;

            if (!(m_patronActionsDone < m_minimumPatronActions && motive.Key == Motive.Type.LeaveMotive))
            {
                m_recentMotiveScores.Add(new KeyValuePair<Action, KeyValuePair<string, float>>(motive.Value.Action, (new KeyValuePair<string, float>(motive.Value.Name(), motiveScore))));

            }
        }

        m_recentMotiveScores.Sort((x, y) => y.Value.Value.CompareTo(x.Value.Value));

#if UNITY_EDITOR
        for (int i = 0; i < m_storedActionsConsidered; ++i)
        {
            m_actionsConsidered.Add(m_recentMotiveScores[i].Value.Key + " " + m_recentMotiveScores[i].Value.Value);
        }
#endif
        // using top 3 highest scoring motives
        float combinedTopMotiveScore = 0.0f;

        for (int i = 0; i < m_motivesToConsider; ++i)
        {
            combinedTopMotiveScore += m_recentMotiveScores[i].Value.Value;
        }

        float pickedMotiveChoice = Random.Range(0.0f, 1.0f) * (combinedTopMotiveScore);
        float motiveAccumulation = 0;
        for (int i = 0; i < m_motivesToConsider; ++i)
        {
            motiveAccumulation += m_recentMotiveScores[i].Value.Value;
            if (pickedMotiveChoice < motiveAccumulation)
            {
                return m_recentMotiveScores[i].Key;
            }
        }
        return m_recentMotiveScores[0].Key;
    }


    public void DisableSprite()
    {
        gameObject.GetComponentInChildren<Renderer>().enabled = false;
        if (m_hasDog)
        {
            gameObject.GetComponentInChildren<Dog>().GetComponentInChildren<Renderer>().enabled = false;
        }
    }

    public void EnableSprite()
    {
        gameObject.GetComponentInChildren<Renderer>().enabled = true;
        if (m_hasDog)
        {
            gameObject.GetComponentInChildren<Dog>().GetComponentInChildren<Renderer>().enabled = true;
        }
    }

    public void EnableHeart()
    {
        m_heartSprite.SetActive(true);
    }

    public void DisableHeart()
    {
        m_heartSprite.SetActive(false);
    }


    private IEnumerator FindNewTask()
    {
        while (true)
        {
            //Debug.Log("Finding new task");
            if (m_hasTask == false)
            {
                Action result = FindNewChoice();
                ++m_patronActionsDone;
                m_hasTask = result.Invoke(this);
            }
            yield return new WaitForSeconds(m_aiTickTime);
        }
    }

    // not good but using it for now due to patrons getting stuck at restAreas and only rest areas
    private IEnumerator IdleCheck()
    {
        while (true)
        {
            Vector3 location = transform.position;
            yield return new WaitForSeconds(10.0f);

            if (location == transform.position)
            {
                m_hasTask = false;
                m_activityState = State.kFree;
                m_moving = false;
            }
        }
    }


    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject == m_movementTarget ||
            (collision.gameObject.CompareTag(Constants.kDog) && m_hasDog == false))
        {
            Activity activity = collision.gameObject.GetComponent<Activity>();
            if (activity != null)
            {
                activity.DoAction(this);
            }
        }

    }


    private void UpdateMotives()
    {
        foreach (KeyValuePair<Motive.Type, Motive> pair in m_motiveDictionary)
        {
            pair.Value.TickMotive();
        }
    }

#if UNITY_EDITOR
    //-----------------------------------------------------------------------Debug
    private void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.gameObject == gameObject)
                {
                    m_debugCanvas.GetComponent<CanvasDebug>().SetPatron(this);
                }
            }
        }
    }

#endif
    //-----------------------------------------------------------------------Debug
}

