using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO fix SpawnPatrons to keep spawning some after we have reached max and deleted some

//TODO add groups
// check population score stuff
// dogs
// change food and rest centors to be able to take groups of people
// not directly to goal

public class PatronSpawner : MonoBehaviour
{
    [SerializeField][Tooltip("The patron prefab that will be spawned")]
    private Patron m_patronInstance;

    [SerializeField][Tooltip("The spawnpoint that patrons are instantiated at")]
    private Transform m_spawnPoint;

    [SerializeField][Tooltip("Maximum number of patrons that can spawn")]
    private int m_maxNumberOfPatrons = 15;

    [SerializeField][Tooltip("Time it takes for patrons to spawn")]
    private float m_patronSpawnTime = 5.0f;

    //TODO get this variable a new name
    // 
    [SerializeField][Tooltip("Start spawning patrons when the number is less than this")]
    private int m_spawnPatronsWhenLessThanThis = 10;

    [SerializeField][Tooltip("How many patrons are spawned at the start of the game")]
    private int m_prePopulatedCount = 10;

    [SerializeField][Tooltip("The list of patrons")]
    private List<Patron> m_patrons = new List<Patron>();

    [SerializeField][Tooltip("The list of the patron groups")]
    private List<PatronGroup> m_patronGroups = new List<PatronGroup>();

    private bool m_spawningPatrons = true;

    private float m_timer = 0.0f;

    [SerializeField][Tooltip("The minimum movement speed that Patrons can have")]
    private float m_minMovementSpeed = 1.0f;

    [SerializeField][Tooltip("The maximum movement speed that Patrons can have")]
    private float m_maxMovementSpeed = 2.0f;

    [SerializeField][Tooltip("The chance that a patron group will spawn. 0-100")]
    [Range(0.0f, 100.0f)]
    private float m_patronGroupSpawnChance = 25.0f;

	void Awake()
    {
        for (int i = 0; i < m_prePopulatedCount; ++i)
        {
            Patron patron = Instantiate(m_patronInstance, m_spawnPoint.position, Quaternion.identity, transform) as Patron;
            float randomMovementSpeed = Random.Range(m_minMovementSpeed, m_maxMovementSpeed);
            patron.SetMovementSpeed(randomMovementSpeed);
            m_patrons.Add(patron);

            Mall.Instance.Population = m_patrons.Count;
            Mall.Instance.MaxPopulation = m_maxNumberOfPatrons;
        }
	}
    private void Update()
    {
        m_timer += Time.deltaTime;
        if (m_spawningPatrons && (m_timer >= m_patronSpawnTime))
        {
            float randGroup = Random.Range(0.0f, 100.0f);
            if (randGroup <= m_patronGroupSpawnChance)
            {
                PatronGroup group = ScriptableObject.CreateInstance<PatronGroup>();
                group.GenerateGroupSize(m_maxNumberOfPatrons - m_patrons.Count);
                for (int i = 0; i < group.GroupSize; ++i)
                {
                    Patron patron = Instantiate(m_patronInstance, m_spawnPoint.position, Quaternion.identity, transform) as Patron;
                    m_patrons.Add(patron);
                    group.AddPatronToGroup(patron);
                    patron.SetGroup(group);
                }
                group.SyncMovementSpeed();
            }
            else
            {
                Patron patron = Instantiate(m_patronInstance, m_spawnPoint.position, Quaternion.identity, transform) as Patron;
                m_patrons.Add(patron);
            }

            m_timer = 0.0f;
            Mall.Instance.Population = m_patrons.Count;

            if (m_patrons.Count == m_maxNumberOfPatrons)
            {
                m_spawningPatrons = false;
            }
        }

        if (m_patrons.Count < m_spawnPatronsWhenLessThanThis)
        {
            m_spawningPatrons = true;
        }
    }

    // 
    private IEnumerator SpawnPatrons()
    {
        while (true)
        {
            if(m_spawningPatrons)
            {
                Patron patron = Instantiate(m_patronInstance, m_spawnPoint.position, m_spawnPoint.rotation, transform) as Patron;
                float randomMovementSpeed = Random.Range(m_minMovementSpeed, m_maxMovementSpeed);
                patron.SetMovementSpeed(randomMovementSpeed);
                m_patrons.Add(patron);
                Mall.Instance.Population = m_patrons.Count;

                yield return new WaitForSeconds(m_patronSpawnTime);

                if (m_patrons.Count == m_maxNumberOfPatrons)
                {
                    m_spawningPatrons = false;
                }
            }
            else if(m_patrons.Count < m_spawnPatronsWhenLessThanThis)
            {
                m_spawningPatrons = true;
            }
            yield return new WaitForSeconds(5);
        }
    }

    public void RemovePatron(Patron patron)
    {
        m_patrons.Remove(patron);
        Destroy(patron.gameObject);
        Mall.Instance.Population = m_patrons.Count;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == Constants.kPatron)
        {
            RemovePatron(collision.gameObject.GetComponent<Patron>());
        }
    }

    private PatronGroup AddGroup()
    {
        PatronGroup group = ScriptableObject.CreateInstance<PatronGroup>();
        m_patronGroups.Add(group);

        return group;
    }
}
